using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class TeacherQuizService : ITeacherQuizService
{
    private readonly ApplicationDbContext _context;

    public TeacherQuizService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuizListItemViewModel>> GetQuizzesByTopicAsync(int topicId)
    {
        return await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.Topic)
            .Include(q => q.QuizQuestions)
            .Include(q => q.QuizAttempts)
            .Where(q => q.TopicId == topicId)
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => new QuizListItemViewModel
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                TopicId = q.TopicId,
                TopicName = q.Topic != null ? q.Topic.Title : "Tong quat",
                QuestionCount = q.QuizQuestions.Count,
                AttemptCount = q.QuizAttempts.Count,
                AverageScore = q.QuizAttempts.Any(a => a.Score.HasValue) ? q.QuizAttempts.Where(a => a.Score.HasValue).Average(a => a.Score!.Value) : 0,
                TimeLimitMinutes = q.TimeLimitMinutes,
                PassScore = q.PassingScore,
                Status = q.Status
            })
            .ToListAsync();
    }

    public async Task<QuizManageViewModel?> GetQuizDetailAsync(int quizId)
    {
        var quiz = await _context.Quizzes
            .AsNoTracking()
            .Include(q => q.QuizQuestions)
                .ThenInclude(qq => qq.Question)
                    .ThenInclude(qb => qb.QuestionOptions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz == null)
        {
            return null;
        }

        return new QuizManageViewModel
        {
            Id = quiz.Id,
            TopicId = quiz.TopicId ?? 0,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassScore = quiz.PassingScore,
            Status = quiz.Status,
            Questions = quiz.QuizQuestions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuizQuestionFormViewModel
                {
                    QuestionText = q.Question.QuestionText,
                    QuestionType = q.Question.QuestionType,
                    Options = q.Question.QuestionOptions.OrderBy(o => o.OrderIndex).Select(o => o.OptionText).ToList(),
                    CorrectAnswer = q.Question.CorrectAnswer,
                    Points = q.Points
                })
                .ToList()
        };
    }

    public async Task<int> CreateQuizAsync(CreateQuizViewModel model, int teacherId)
    {
        var topic = await _context.LearningTopics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == model.TopicId);
        var now = DateTime.UtcNow;
        var quiz = new Quiz
        {
            TopicId = model.TopicId,
            SkillId = topic?.SkillId ?? 1,
            Title = model.Title,
            Description = model.Description,
            QuizType = "PRACTICE",
            TimeLimitMinutes = model.TimeLimitMinutes,
            PassingScore = model.PassScore,
            Status = "PUBLISHED",
            CreatedBy = teacherId,
            CreatedAt = now
        };

        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        for (var i = 0; i < model.Questions.Count; i++)
        {
            var formQuestion = model.Questions[i];
            if (string.IsNullOrWhiteSpace(formQuestion.QuestionText))
            {
                continue;
            }

            var question = new QuestionBank
            {
                TopicId = model.TopicId,
                SkillId = topic?.SkillId ?? 1,
                QuestionType = formQuestion.QuestionType,
                QuestionText = formQuestion.QuestionText,
                CorrectAnswer = formQuestion.CorrectAnswer,
                Explanation = null,
                DifficultyLevel = "INTERMEDIATE",
                SourceType = "TEACHER_CREATED",
                ReviewStatus = "APPROVED",
                CreatedBy = teacherId,
                ApprovedBy = teacherId,
                ApprovedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var option in formQuestion.Options.Where(o => !string.IsNullOrWhiteSpace(o)).Select((value, index) => new { value, index }))
            {
                question.QuestionOptions.Add(new QuestionOption
                {
                    OptionText = option.value,
                    IsCorrect = string.Equals(option.value, formQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase),
                    OrderIndex = option.index + 1
                });
            }

            _context.QuestionBanks.Add(question);
            await _context.SaveChangesAsync();

            _context.QuizQuestions.Add(new QuizQuestion
            {
                QuizId = quiz.Id,
                QuestionId = question.Id,
                Points = formQuestion.Points,
                OrderIndex = i + 1
            });
        }

        await _context.SaveChangesAsync();
        return quiz.Id;
    }

    public async Task UpdateQuizAsync(EditQuizViewModel model)
    {
        var quiz = await _context.Quizzes.FindAsync(model.Id);
        if (quiz == null)
        {
            return;
        }

        var topic = await _context.LearningTopics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == model.TopicId);
        quiz.TopicId = model.TopicId;
        quiz.SkillId = topic?.SkillId ?? quiz.SkillId;
        quiz.Title = model.Title;
        quiz.Description = model.Description;
        quiz.TimeLimitMinutes = model.TimeLimitMinutes;
        quiz.PassingScore = model.PassScore;
        quiz.Status = model.Status;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteQuizAsync(int quizId)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz == null)
        {
            return;
        }

        quiz.Status = "ARCHIVED";
        await _context.SaveChangesAsync();
    }

    public async Task<List<QuizAttemptResultViewModel>> GetQuizAttemptsAsync(int quizId)
    {
        return await _context.QuizAttempts
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId && a.SubmittedAt.HasValue)
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new QuizAttemptResultViewModel
            {
                StudentName = a.Student.FullName,
                Score = a.Score,
                SubmittedAt = a.SubmittedAt,
                TimeTaken = a.SubmittedAt.HasValue ? a.SubmittedAt.Value - a.StartedAt : null
            })
            .ToListAsync();
    }
}
