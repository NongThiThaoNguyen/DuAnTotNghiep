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
                var defaultQuestions = GetDefaultQuestions(topic.Title);

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

        private static (string QuestionText, string CorrectAnswer, string[] Options, string Explanation)[] GetDefaultQuestions(string title)
        {
            if (title.Contains("TOEIC", StringComparison.OrdinalIgnoreCase) || title.Contains("công sở", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    ("The manager will ___ the meeting at 9 a.m.", "attend", new[] { "attend", "attendance", "attended", "attending" }, "After will, use the base verb attend."),
                    ("Please ___ the attached form before Friday.", "submit", new[] { "submit", "submission", "submits", "submitted" }, "Submit is the verb needed after please."),
                    ("What does 'deadline' mean?", "the final time to finish something", new[] { "a meeting room", "the final time to finish something", "a salary increase", "a job interview" }, "A deadline is the latest time by which something must be completed."),
                    ("The documents ___ by the assistant yesterday.", "were prepared", new[] { "prepare", "prepared", "were prepared", "are preparing" }, "Use the past passive for documents completed yesterday."),
                    ("Which phrase makes a polite business request?", "Could you please...?", new[] { "Could you please...?", "Do it now.", "You must do it.", "I refuse to..." }, "Could you please...? is a polite business request.")
                };
            }

            if (title.Contains("nghe", StringComparison.OrdinalIgnoreCase) || title.Contains("listening", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    ("Which word signals a correction in a conversation?", "Actually", new[] { "Actually", "Finally", "Usually", "Because" }, "Actually often introduces corrected information."),
                    ("In listening practice, what is a distractor?", "Information that sounds plausible but is wrong", new[] { "The correct answer", "Information that sounds plausible but is wrong", "A speaker's name", "A question title" }, "A distractor is misleading information designed to test careful listening."),
                    ("Choose the phrase that asks someone to repeat information.", "Could you say that again?", new[] { "Could you say that again?", "I agree completely.", "That is mine.", "It starts at noon." }, "This phrase is a request for repetition."),
                    ("What should you listen for when answering a timetable question?", "Times and schedule keywords", new[] { "Times and schedule keywords", "Every adjective", "The speaker's accent", "Unrelated opinions" }, "Times and schedule keywords help locate the answer."),
                    ("Which strategy helps with numbers in a recording?", "Listen for the number and its unit", new[] { "Listen for the number and its unit", "Translate every word", "Ignore the context", "Read the answer aloud" }, "The unit and surrounding context confirm a number.")
                };
            }

            if (title.Contains("đọc", StringComparison.OrdinalIgnoreCase) || title.Contains("reading", StringComparison.OrdinalIgnoreCase))
            {
                return new[]
                {
                    ("What is skimming used for?", "Finding the general idea quickly", new[] { "Finding the general idea quickly", "Checking every spelling error", "Memorising all details", "Translating each sentence" }, "Skimming means reading quickly for the main idea."),
                    ("What is scanning used for?", "Finding a specific fact", new[] { "Finding a specific fact", "Writing a conclusion", "Learning pronunciation", "Guessing the topic" }, "Scanning searches for a particular detail such as a name or date."),
                    ("What should you do with an unfamiliar word?", "Use context clues", new[] { "Use context clues", "Stop immediately", "Choose the longest answer", "Skip the whole passage" }, "Nearby words often reveal the meaning of an unfamiliar word."),
                    ("Which word is closest in meaning to 'decline'?", "decrease", new[] { "increase", "decrease", "explain", "discover" }, "Decline means decrease or fall."),
                    ("A reference question asks you to identify...", "what a word refers to", new[] { "what a word refers to", "the author's age", "the page number", "the font size" }, "Reference questions test links between pronouns and earlier nouns.")
                };
            }

            return new[]
            {
                ("Choose the correct sentence.", "She studies English every day.", new[] { "She studies English every day.", "She study English every day.", "She studying English.", "She studieds English." }, "Use studies with the third-person singular subject she."),
                ("Choose the correct preposition: interested ___ English.", "in", new[] { "in", "on", "at", "for" }, "The correct collocation is interested in."),
                ("Which word means 'important'?", "significant", new[] { "significant", "ordinary", "empty", "brief" }, "Significant means important or notable."),
                ("Which phrase introduces an example?", "For example", new[] { "For example", "In contrast", "As a result", "In conclusion" }, "For example introduces a specific example."),
                ("If it rains, we ___ at home.", "stay", new[] { "stay", "stayed", "would stayed", "staying" }, "The first conditional uses present simple in the if-clause.")
            };
        }
    }
}
