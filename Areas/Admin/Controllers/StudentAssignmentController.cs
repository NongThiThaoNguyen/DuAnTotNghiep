using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class StudentAssignmentController : Controller
{
    private const int PageSize = 20;
    private readonly ApplicationDbContext _context;

    public StudentAssignmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? studentId, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _context.PracticeSubmissions.AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.PracticeTask)
                .ThenInclude(t => t.Topic)
            .AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(s => s.StudentId == studentId.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Min(page, totalPages);

        var model = new StudentAssignmentListViewModel
        {
            StudentId = studentId,
            CurrentPage = page,
            PageSize = PageSize,
            TotalItems = totalItems,
            Students = await GetStudentOptionsAsync(),
            Items = await query
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
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
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int submissionId)
    {
        var submission = await _context.PracticeSubmissions.AsNoTracking()
            .Include(s => s.Student)
            .Include(s => s.PracticeTask)
                .ThenInclude(t => t.Topic)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission == null)
        {
            return NotFound();
        }

        return View(new StudentAssignmentDetailsViewModel
        {
            SubmissionId = submission.Id,
            StudentName = submission.Student.FullName,
            TaskTitle = submission.PracticeTask.Title,
            TaskInstruction = submission.PracticeTask.Instruction,
            TopicTitle = submission.PracticeTask.Topic?.Title ?? "Chưa gắn topic",
            Status = submission.Status,
            Score = submission.Score,
            SubmissionText = submission.SubmissionText,
            FileUrl = submission.FileUrl,
            AudioUrl = submission.AudioUrl,
            AiFeedback = submission.AiFeedback,
            TeacherFeedback = submission.TeacherFeedback,
            SubmittedAt = submission.SubmittedAt
        });
    }

    private Task<List<AdminOptionViewModel>> GetStudentOptionsAsync()
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "STUDENT")
            .OrderBy(u => u.FullName)
            .Select(u => new AdminOptionViewModel { Id = u.Id, Text = u.FullName })
            .ToListAsync();
    }
}
