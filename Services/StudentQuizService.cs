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
        private readonly IPathViewService _pathViewService;

        public StudentQuizService(
            ApplicationDbContext context,
            IGamificationService gamificationService,
            IPathViewService pathViewService)
        {
            _context = context;
            _gamificationService = gamificationService;
            _pathViewService = pathViewService;
        }

        public async Task<QuizViewModel?> GetQuizForTakingAsync(int topicId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Topic)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.Question)
                        .ThenInclude(qb => qb.QuestionOptions)
                .FirstOrDefaultAsync(q => q.TopicId == topicId);

            if (quiz == null)
            {
                var topic = await _context.LearningTopics.FindAsync(topicId);
                if (topic == null) return null;

                // Auto-create a default quiz for this topic
                quiz = new Quiz
                {
                    TopicId = topicId,
                    SkillId = topic.SkillId,
                    Title = $"Bài trắc nghiệm: {topic.Title}",
                    Description = $"Bài kiểm tra trắc nghiệm củng cố kiến thức cho chủ đề {topic.Title}.",
                    QuizType = "TOPIC_QUIZ",
                    TimeLimitMinutes = 15,
                    PassingScore = 7.0m,
                    Status = "PUBLISHED",
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Quizzes.AddAsync(quiz);
                await _context.SaveChangesAsync();

                // Add sample questions
                var defaultQuestions = new[]
                {
                    ("Choose the correct word: 'He is very _______ in learning English.'", "interested", new[] { "interested", "interest", "interesting", "interestingly" }, "Adjective describing feelings."),
                    ("What is the synonym of 'essential'?", "vital", new[] { "vital", "optional", "minor", "trivial" }, "'Essential' means vital."),
                    ("She succeeded _______ passing the final exam.", "in", new[] { "in", "on", "at", "with" }, "Succeed in V-ing."),
                    ("Which sentence is grammatically correct?", "If I were you, I would accept the offer.", new[] { "If I were you, I would accept the offer.", "If I am you, I will accept the offer.", "If I was you, I will accept the offer.", "If I had been you, I accept." }, "Second conditional rule."),
                    ("What does 'break a leg' mean?", "Good luck!", new[] { "Good luck!", "Break your leg", "Run fast", "Be quiet" }, "Idiom for wishing good luck.")
                };

                int qIndex = 1;
                foreach (var (qText, correctAns, opts, exp) in defaultQuestions)
                {
                    var qBank = new QuestionBank
                    {
                        TopicId = topicId,
                        SkillId = topic.SkillId,
                        QuestionType = "MULTIPLE_CHOICE",
                        QuestionText = qText,
                        CorrectAnswer = correctAns,
                        Explanation = exp,
                        DifficultyLevel = "MEDIUM",
                        SourceType = "SYSTEM",
                        ReviewStatus = "APPROVED",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.QuestionBanks.AddAsync(qBank);
                    await _context.SaveChangesAsync();

                    var qq = new QuizQuestion
                    {
                        QuizId = quiz.Id,
                        QuestionId = qBank.Id,
                        Points = 2.0m,
                        OrderIndex = qIndex++
                    };
                    await _context.QuizQuestions.AddAsync(qq);

                    int oIndex = 1;
                    foreach (var opt in opts)
                    {
                        var qOpt = new QuestionOption
                        {
                            QuestionId = qBank.Id,
                            OptionText = opt,
                            IsCorrect = (opt == correctAns),
                            OrderIndex = oIndex++
                        };
                        await _context.QuestionOptions.AddAsync(qOpt);
                    }
                }
                await _context.SaveChangesAsync();

                // Re-fetch full quiz with includes
                quiz = await _context.Quizzes
                    .Include(q => q.Topic)
                    .Include(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.Question)
                            .ThenInclude(qb => qb.QuestionOptions)
                    .FirstOrDefaultAsync(q => q.Id == quiz.Id);

                if (quiz == null) return null;
            }

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

            int? nodeId = await _context.LearningPathNodes
                .Where(n => n.QuizId == quizId || (quiz.TopicId.HasValue && n.TopicId == quiz.TopicId && n.NodeType == "QUIZ"))
                .Select(n => (int?)n.Id)
                .FirstOrDefaultAsync();

            if (nodeId.HasValue)
            {
                await _pathViewService.MarkNodeCompletedAsync(nodeId.Value, userId, "QUIZ", 10, score, $"Quiz: {quiz.Title}");
            }
            else
            {
                // Save Activity Log if no direct node mapped
                var log = new StudyActivityLog
                {
                    StudentId = userId,
                    ActivityType = "QUIZ",
                    TopicId = quiz.TopicId,
                    LearningPathNodeId = null,
                    DurationMinutes = 10,
                    Score = score,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.StudyActivityLogs.AddAsync(log);
                await _context.SaveChangesAsync();
            }

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
