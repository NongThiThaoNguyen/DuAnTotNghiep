using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuizController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }

    [HttpGet]
    public async Task<IActionResult> Take(int id) // id is course/topic ID
    {
        var quiz = await _context.Quizzes
            .Include(q => q.Topic)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(qb => qb.QuestionOptions)
            .FirstOrDefaultAsync(q => q.TopicId == id);

        if (quiz == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy bài trắc nghiệm cho khóa học này.";
            return RedirectToAction("Index", "Courses");
        }

        var vm = new QuizViewModel
        {
            QuizId = quiz.Id,
            CourseTitle = quiz.Topic?.Title ?? "Khóa học",
            Title = quiz.Title,
            Description = quiz.Description ?? "Luyện tập trắc nghiệm củng cố kiến thức.",
            TimeLimitMinutes = quiz.TimeLimitMinutes ?? 15,
            Questions = quiz.QuizQuestions.OrderBy(qq => qq.OrderIndex).Select(qq => new QuizQuestionViewModel
            {
                Id = qq.QuestionId,
                QuestionText = qq.Question.QuestionText,
                Explanation = qq.Question.Explanation ?? "Không có giải thích chi tiết.",
                CorrectAnswer = qq.Question.CorrectAnswer ?? "A",
                Options = qq.Question.QuestionOptions.OrderBy(o => o.OrderIndex).Select(o => new QuizOptionViewModel
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int quizId, Dictionary<int, string> answers)
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

        var quiz = await _context.Quizzes
            .Include(q => q.Topic)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(qb => qb.QuestionOptions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null) return NotFound();

        int correctCount = 0;
        int totalQuestions = quiz.QuizQuestions.Count;

        foreach (var qq in quiz.QuizQuestions)
        {
            answers.TryGetValue(qq.QuestionId, out string? studentAnswer);
            if (studentAnswer != null && studentAnswer.Trim().ToUpper() == qq.Question.CorrectAnswer?.Trim().ToUpper())
            {
                correctCount++;
            }
        }

        decimal score = totalQuestions > 0 ? (decimal)correctCount / totalQuestions * 10 : 0m;

        // Save Attempt
        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            StudentId = userId,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            SubmittedAt = DateTime.UtcNow,
            Score = score,
            Status = "COMPLETED"
        };
        _context.QuizAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        // Save Activity Log
        var log = new StudyActivityLog
        {
            StudentId = userId,
            ActivityType = "QUIZ",
            TopicId = quiz.TopicId,
            LearningPathNodeId = quizId,
            DurationMinutes = 10,
            Score = score,
            CreatedAt = DateTime.UtcNow
        };
        _context.StudyActivityLogs.Add(log);
        await _context.SaveChangesAsync();

        ViewBag.Score = score;
        ViewBag.CorrectCount = correctCount;
        ViewBag.TotalCount = totalQuestions;
        ViewBag.Answers = answers;

        var vm = new QuizViewModel
        {
            QuizId = quiz.Id,
            CourseTitle = quiz.Topic?.Title ?? "Khóa học",
            Title = quiz.Title,
            Questions = quiz.QuizQuestions.OrderBy(qq => qq.OrderIndex).Select(qq => new QuizQuestionViewModel
            {
                Id = qq.QuestionId,
                QuestionText = qq.Question.QuestionText,
                Explanation = qq.Question.Explanation ?? "Không có giải thích chi tiết.",
                CorrectAnswer = qq.Question.CorrectAnswer ?? "A",
                Options = qq.Question.QuestionOptions.OrderBy(o => o.OrderIndex).Select(o => new QuizOptionViewModel
                {
                    Id = o.Id,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList()
        };

        return View("Result", vm);
    }
}
