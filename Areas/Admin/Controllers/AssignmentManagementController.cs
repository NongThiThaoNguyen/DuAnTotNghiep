using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class AssignmentManagementController : Controller
{
    private readonly ApplicationDbContext _context;

    public AssignmentManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? topicId)
    {
        var query = _context.PracticeTasks.AsNoTracking()
            .Include(t => t.Topic)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.PracticeSubmissions)
            .AsQueryable();

        if (topicId.HasValue)
        {
            query = query.Where(t => t.TopicId == topicId.Value);
        }

        var model = new AssignmentManagementIndexViewModel
        {
            TopicId = topicId,
            Topics = await _context.LearningTopics.AsNoTracking()
                .OrderBy(t => t.Title)
                .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
                .ToListAsync(),
            Items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new AssignmentManagementRowViewModel
                {
                    AssignmentId = t.Id,
                    Title = t.Title,
                    TopicTitle = t.Topic != null ? t.Topic.Title : "Chưa gắn topic",
                    TeacherName = t.CreatedByNavigation != null ? t.CreatedByNavigation.FullName : "Không rõ",
                    Status = t.Status,
                    SubmissionCount = t.PracticeSubmissions.Count,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Submissions(int assignmentId)
    {
        var assignment = await _context.PracticeTasks.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == assignmentId);

        if (assignment == null)
        {
            return NotFound();
        }

        var submissions = await _context.PracticeSubmissions.AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.PracticeTask)
                .ThenInclude(t => t.Topic)
            .Where(s => s.PracticeTaskId == assignmentId)
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new StudentAssignmentRowViewModel
            {
                SubmissionId = s.Id,
                StudentName = s.Student.FullName,
                TaskTitle = s.PracticeTask.Title,
                TopicTitle = s.PracticeTask.Topic != null ? s.PracticeTask.Topic.Title : "Chưa gắn topic",
                Status = s.Status,
                Score = s.Score,
                SubmittedAt = s.SubmittedAt
            })
            .ToListAsync();

        return View(new AssignmentSubmissionsAdminViewModel
        {
            AssignmentId = assignment.Id,
            AssignmentTitle = assignment.Title,
            Submissions = submissions
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int assignmentId)
    {
        var assignment = await _context.PracticeTasks
            .Include(t => t.PracticeSubmissions)
            .FirstOrDefaultAsync(t => t.Id == assignmentId);

        if (assignment == null)
        {
            return NotFound();
        }

        if (assignment.PracticeSubmissions.Any())
        {
            assignment.Status = "ARCHIVED";
            TempData["SuccessMessage"] = "Bài tập đã có submission nên được chuyển sang ARCHIVED.";
        }
        else
        {
            _context.PracticeTasks.Remove(assignment);
            TempData["SuccessMessage"] = "Đã xóa bài tập.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
