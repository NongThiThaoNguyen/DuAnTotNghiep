using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class StudentQuizService : IStudentQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGamificationService _gamificationService;

        public StudentQuizService(ApplicationDbContext context, IGamificationService gamificationService)
        {
            _context = context;
            _gamificationService = gamificationService;
        }

        public async Task<QuizViewModel?> GetQuizForTakingAsync(int topicId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Topic)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(qb => qb.QuestionOptions)
                .FirstOrDefaultAsync(q => q.TopicId == topicId);

            if (quiz == null) return null;

            return new QuizViewModel
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
        }

        public async Task<QuizViewModel?> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Topic)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(qb => qb.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return null;

            return new QuizViewModel
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
        }

        public async Task<QuizResultDto> SubmitQuizAsync(int quizId, int userId, Dictionary<int, string> answers)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Topic)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(qb => qb.QuestionOptions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) throw new ArgumentException("Quiz not found.");

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
            await _context.QuizAttempts.AddAsync(attempt);

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
            await _context.StudyActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            // Gamification hook
            await _gamificationService.CheckAndGrantAchievementsAsync(userId);

            return new QuizResultDto
            {
                Score = score,
                CorrectCount = correctCount,
                TotalCount = totalQuestions
            };
        }

        public async Task<List<DuAnTotNghiep.Models.ViewModels.Student.StudentQuizItemViewModel>> GetAllQuizzesAsync(int userId)
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Topic)
                .Include(q => q.QuizQuestions)
                .Select(q => new DuAnTotNghiep.Models.ViewModels.Student.StudentQuizItemViewModel
                {
                    QuizId = q.Id,
                    QuizTitle = q.Title,
                    CourseName = q.Topic != null ? q.Topic.Title : "Khóa học",
                    TotalQuestions = q.QuizQuestions.Count,
                    TimeLimit = q.TimeLimitMinutes ?? 15,
                    TopicId = q.TopicId ?? 0
                })
                .ToListAsync();

            var attempts = await _context.QuizAttempts
                .Where(a => a.StudentId == userId)
                .ToListAsync();

            foreach (var quiz in quizzes)
            {
                var quizAttempts = attempts.Where(a => a.QuizId == quiz.QuizId).ToList();
                if (quizAttempts.Any())
                {
                    quiz.IsCompleted = true;
                    quiz.HighestScore = quizAttempts.Max(a => a.Score ?? 0);
                    quiz.LastAttemptDate = quizAttempts.Max(a => a.StartedAt);
                }
            }

            return quizzes;
        }
    }
}
