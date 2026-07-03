using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class TeacherAssignmentAdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public TeacherAssignmentAdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _context.Schedules.AsNoTracking()
            .Include(s => s.Teacher)
            .Include(s => s.Topic)
            .OrderByDescending(s => s.StartTime)
            .Select(s => new TeacherAssignmentRowViewModel
            {
                AssignmentId = s.Id,
                TeacherId = s.TeacherId,
                TeacherName = s.Teacher.FullName,
                TopicId = s.TopicId,
                TopicTitle = s.Topic != null ? s.Topic.Title : "Chưa gắn topic",
                Title = s.Title,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Classroom = s.Classroom
            })
            .ToListAsync();

        return View(new TeacherAssignmentIndexViewModel { Items = items });
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildCreateModelAsync(new TeacherAssignmentCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(TeacherAssignmentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", await BuildCreateModelAsync(model));
        }

        var teacherExists = await _context.Users.AnyAsync(u => u.Id == model.TeacherId && u.Role.RoleCode == "TEACHER");
        var topicExists = await _context.LearningTopics.AnyAsync(t => t.Id == model.TopicId);
        if (!teacherExists || !topicExists)
        {
            ModelState.AddModelError(string.Empty, "Giáo viên hoặc topic không hợp lệ.");
            return View("Create", await BuildCreateModelAsync(model));
        }

        _context.Schedules.Add(new Schedule
        {
            TeacherId = model.TeacherId,
            TopicId = model.TopicId,
            Title = string.IsNullOrWhiteSpace(model.Title) ? "Phân công giảng dạy" : model.Title,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            Classroom = model.Classroom,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã phân công giáo viên cho topic.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unassign(int assignmentId)
    {
        var assignment = await _context.Schedules.FindAsync(assignmentId);
        if (assignment == null)
        {
            return NotFound();
        }

        _context.Schedules.Remove(assignment);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã hủy phân công giáo viên.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<TeacherAssignmentCreateViewModel> BuildCreateModelAsync(TeacherAssignmentCreateViewModel model)
    {
        model.Teachers = await _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .Select(u => new AdminOptionViewModel { Id = u.Id, Text = u.FullName })
            .ToListAsync();
        model.Topics = await _context.LearningTopics.AsNoTracking()
            .OrderBy(t => t.Title)
            .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
            .ToListAsync();
        return model;
    }
}
