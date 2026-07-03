using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class AdminQuizManagementService : IAdminQuizManagementService
{
    private readonly ApplicationDbContext _context;

    public AdminQuizManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizManagementIndexViewModel> GetAllQuizzesAsync(int? topicId = null, int? teacherId = null)
    {
        var query = _context.Quizzes.AsNoTracking()
            .Include(q => q.Topic)
            .Include(q => q.CreatedByNavigation)
            .Include(q => q.QuizQuestions)
            .Include(q => q.QuizAttempts)
            .AsQueryable();

        if (topicId.HasValue)
        {
            query = query.Where(q => q.TopicId == topicId.Value);
        }

        if (teacherId.HasValue)
        {
            query = query.Where(q => q.CreatedBy == teacherId.Value);
        }

        var quizzes = await query
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();

        return new QuizManagementIndexViewModel
        {
            TopicId = topicId,
            TeacherId = teacherId,
            Topics = await GetTopicOptionsAsync(),
            Teachers = await GetTeacherOptionsAsync(),
            Items = quizzes.Select(quiz => new QuizManagementRowViewModel
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                TopicTitle = quiz.Topic?.Title ?? "Chưa gắn topic",
                TeacherName = quiz.CreatedByNavigation?.FullName ?? "Không rõ",
                Status = quiz.Status,
                QuestionCount = quiz.QuizQuestions.Count,
                AttemptCount = quiz.QuizAttempts.Count,
                AverageScore = quiz.QuizAttempts.Any(a => a.Score != null)
                    ? Math.Round(quiz.QuizAttempts.Where(a => a.Score != null).Average(a => a.Score!.Value), 2)
                    : 0
            }).ToList()
        };
    }

    public async Task<QuizManagementDetailsViewModel?> GetQuizDetailsAsync(int quizId)
    {
        var quiz = await _context.Quizzes.AsNoTracking()
            .Include(q => q.Topic)
            .Include(q => q.CreatedByNavigation)
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            return null;
        }

        return new QuizManagementDetailsViewModel
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            TopicTitle = quiz.Topic?.Title ?? "Chưa gắn topic",
            TeacherName = quiz.CreatedByNavigation?.FullName ?? "Không rõ",
            Status = quiz.Status,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassingScore = quiz.PassingScore,
            Questions = quiz.QuizQuestions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuizQuestionAdminRowViewModel
                {
                    QuestionText = q.Question.QuestionText,
                    QuestionType = q.Question.QuestionType,
                    Points = q.Points,
                    CorrectAnswer = q.Question.CorrectAnswer
                })
                .ToList()
        };
    }

    public async Task<QuizAttemptsAdminViewModel?> GetQuizAttemptsAsync(int quizId)
    {
        var quiz = await _context.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == quizId);
        if (quiz == null)
        {
            return null;
        }

        var attempts = await _context.QuizAttempts.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Topic)
            .Where(a => a.QuizId == quizId)
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
            .ToListAsync();

        return new QuizAttemptsAdminViewModel
        {
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            Attempts = attempts
        };
    }

    private Task<List<AdminOptionViewModel>> GetTopicOptionsAsync()
    {
        return _context.LearningTopics.AsNoTracking()
            .OrderBy(t => t.Title)
            .Select(t => new AdminOptionViewModel { Id = t.Id, Text = t.Title })
            .ToListAsync();
    }

    private Task<List<AdminOptionViewModel>> GetTeacherOptionsAsync()
    {
        return _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .Select(u => new AdminOptionViewModel { Id = u.Id, Text = u.FullName })
            .ToListAsync();
    }
}
