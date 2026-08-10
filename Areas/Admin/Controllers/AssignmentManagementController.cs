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
    private const int PageSize = 15;
    private readonly ApplicationDbContext _context;

    public AssignmentManagementController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? keyword, int? topicId, int? teacherId, string? status, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _context.PracticeTasks.AsNoTracking()
            .Include(t => t.Topic)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.PracticeSubmissions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(t => t.Title.Contains(k) || t.Instruction.Contains(k));
        }

        if (topicId.HasValue)
        {
            query = query.Where(t => t.TopicId == topicId.Value);
        }

        if (teacherId.HasValue)
        {
            query = query.Where(t => t.CreatedBy == teacherId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(t => t.Status == status);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Min(page, totalPages);

        var totalStudents = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "STUDENT");

        var topics = await _context.LearningTopics.AsNoTracking()
            .OrderBy(t => t.Title)
            .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
            .ToListAsync();

        var teachers = await _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .Select(u => new AdminOptionViewModel { Id = u.Id, Text = u.FullName })
            .ToListAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(t => new AssignmentManagementRowViewModel
            {
                AssignmentId = t.Id,
                Title = t.Title,
                TaskType = t.TaskType ?? "ASSIGNMENT",
                DifficultyLevel = t.DifficultyLevel ?? "MEDIUM",
                TopicTitle = t.Topic != null ? t.Topic.Title : "Chưa gắn topic",
                TeacherName = t.CreatedByNavigation != null ? t.CreatedByNavigation.FullName : "Hệ thống",
                Status = t.Status,
                SubmissionCount = t.PracticeSubmissions.Count,
                TotalStudentCount = totalStudents,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        var model = new AssignmentManagementIndexViewModel
        {
            Keyword = keyword,
            TopicId = topicId,
            TeacherId = teacherId,
            Status = status,
            CurrentPage = page,
            PageSize = PageSize,
            TotalItems = totalItems,
            Topics = topics,
            Teachers = teachers,
            Items = items
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int assignmentId)
    {
        var assignment = await _context.PracticeTasks.AsNoTracking()
            .Include(t => t.Topic)
            .Include(t => t.CreatedByNavigation)
            .Include(t => t.PracticeSubmissions)
                .ThenInclude(s => s.Student)
            .FirstOrDefaultAsync(t => t.Id == assignmentId);

        if (assignment == null)
        {
            return NotFound();
        }

        var totalStudents = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "STUDENT");

        var submissions = assignment.PracticeSubmissions
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => new StudentAssignmentRowViewModel
            {
                SubmissionId = s.Id,
                StudentName = s.Student?.FullName ?? "Học viên",
                TaskTitle = assignment.Title,
                TopicTitle = assignment.Topic?.Title ?? "Chưa gắn topic",
                Status = s.Status,
                Score = s.Score,
                SubmittedAt = s.SubmittedAt
            })
            .ToList();

        var model = new AssignmentDetailsAdminViewModel
        {
            AssignmentId = assignment.Id,
            Title = assignment.Title,
            Instruction = assignment.Instruction,
            TaskType = assignment.TaskType ?? "ASSIGNMENT",
            DifficultyLevel = assignment.DifficultyLevel ?? "MEDIUM",
            TopicTitle = assignment.Topic?.Title ?? "Chưa gắn topic",
            TeacherName = assignment.CreatedByNavigation?.FullName ?? "Hệ thống",
            Status = assignment.Status,
            CreatedAt = assignment.CreatedAt,
            SubmissionCount = submissions.Count,
            TotalStudentCount = totalStudents,
            Submissions = submissions
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int assignmentId)
    {
        var assignment = await _context.PracticeTasks.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == assignmentId);

        if (assignment == null)
        {
            return NotFound();
        }

        var topics = await _context.LearningTopics.AsNoTracking()
            .OrderBy(t => t.Title)
            .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
            .ToListAsync();

        var model = new EditAssignmentAdminViewModel
        {
            AssignmentId = assignment.Id,
            Title = assignment.Title,
            Instruction = assignment.Instruction,
            TaskType = assignment.TaskType ?? "ASSIGNMENT",
            DifficultyLevel = assignment.DifficultyLevel ?? "MEDIUM",
            TopicId = assignment.TopicId,
            Status = assignment.Status,
            Topics = topics
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditAssignmentAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Topics = await _context.LearningTopics.AsNoTracking()
                .OrderBy(t => t.Title)
                .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
                .ToListAsync();
            return View(model);
        }

        var assignment = await _context.PracticeTasks.FirstOrDefaultAsync(t => t.Id == model.AssignmentId);
        if (assignment == null)
        {
            return NotFound();
        }

        assignment.Title = model.Title.Trim();
        assignment.Instruction = model.Instruction;
        assignment.TaskType = model.TaskType;
        assignment.DifficultyLevel = model.DifficultyLevel;
        assignment.TopicId = model.TopicId;
        assignment.Status = model.Status;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cập nhật bài tập thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int assignmentId)
    {
        var assignment = await _context.PracticeTasks.FirstOrDefaultAsync(t => t.Id == assignmentId);
        if (assignment == null)
        {
            return NotFound();
        }

        if (assignment.Status == "ACTIVE" || assignment.Status == "PUBLISHED")
        {
            assignment.Status = "CLOSED";
            TempData["SuccessMessage"] = "Đã đóng bài tập.";
        }
        else
        {
            assignment.Status = "ACTIVE";
            TempData["SuccessMessage"] = "Đã mở lại bài tập.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Delete(int assignmentId, string? keyword, int? topicId, int? teacherId, string? status, int page = 1)
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
            TempData["SuccessMessage"] = "Bài tập đã có lượt làm nên được chuyển sang trạng thái Lưu trữ (ARCHIVED) để giữ lịch sử.";
        }
        else
        {
            _context.PracticeTasks.Remove(assignment);
            TempData["SuccessMessage"] = "Đã xóa bài tập khỏi hệ thống thành công.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { keyword, topicId, teacherId, status, page });
    }
}
