using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class StudentQuizController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentQuizController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? studentId)
    {
        var query = _context.QuizAttempts.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Topic)
            .AsQueryable();

        if (studentId.HasValue)
        {
            query = query.Where(a => a.StudentId == studentId.Value);
        }

        var model = new StudentQuizListViewModel
        {
            StudentId = studentId,
            Students = await GetStudentOptionsAsync(),
            Items = await query
                .OrderByDescending(a => a.StartedAt)
                .Select(a => new StudentQuizAttemptRowViewModel
                {
                    Id = a.Id,
                    StudentName = a.Student.FullName,
                    QuizTitle = a.Quiz.Title,
                    TopicTitle = a.Quiz.Topic != null ? a.Quiz.Topic.Title : "Chưa gắn topic",
                    Score = a.Score,
                    Status = a.Status,
                    StartedAt = a.StartedAt,
                    SubmittedAt = a.SubmittedAt
                })
                .ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int attemptId)
    {
        var attempt = await _context.QuizAttempts.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Quiz)
            .Include(a => a.QuizAnswers)
                .ThenInclude(answer => answer.Question)
            .Include(a => a.QuizAnswers)
                .ThenInclude(answer => answer.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null)
        {
            return NotFound();
        }

        var model = new StudentQuizDetailsViewModel
        {
            AttemptId = attempt.Id,
            StudentName = attempt.Student.FullName,
            QuizTitle = attempt.Quiz.Title,
            Score = attempt.Score,
            Status = attempt.Status,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            Answers = attempt.QuizAnswers
                .OrderBy(a => a.AnsweredAt)
                .Select(answer => new QuizAnswerRowViewModel
                {
                    QuestionText = answer.Question.QuestionText,
                    AnswerText = answer.AnswerText,
                    SelectedOptionText = answer.SelectedOption?.OptionText,
                    IsCorrect = answer.IsCorrect,
                    Score = answer.Score,
                    AiExplanation = answer.AiExplanation
                })
                .ToList()
        };

        return View(model);
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
