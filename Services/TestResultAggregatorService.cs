using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services
{
    public class TestResultAggregatorService : ITestResultAggregatorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestResultAggregatorService> _logger;

        public TestResultAggregatorService(ApplicationDbContext context, ILogger<TestResultAggregatorService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AssessmentInputDto> AggregateTestDataAsync(int attemptId, int currentUserId)
        {
            _logger.LogInformation("Starting test result aggregation for Attempt ID: {AttemptId} by User ID: {UserId}", attemptId, currentUserId);

            try
            {
                // 1. Fetch Attempt with Eager Loading to avoid N+1 query issues
                var attempt = await _context.TestAttempts
                    .Include(a => a.PlacementTest)
                    .Include(a => a.TestAnswers)
                        .ThenInclude(ans => ans.Question)
                            .ThenInclude(q => q.Skill)
                    .Include(a => a.TestAnswers)
                        .ThenInclude(ans => ans.Question)
                            .ThenInclude(q => q.Topic)
                    .Include(a => a.TestAnswers)
                        .ThenInclude(ans => ans.SelectedOption)
                    .FirstOrDefaultAsync(a => a.Id == attemptId);

                if (attempt == null)
                {
                    _logger.LogWarning("Placement test attempt ID: {AttemptId} was not found.", attemptId);
                    throw new KeyNotFoundException($"Placement test attempt ID {attemptId} was not found.");
                }

                // 2. Validate Attempt Status
                string status = attempt.Status.ToUpper();
                if (status != "SUBMITTED" && status != "GRADED")
                {
                    _logger.LogWarning("Attempt ID: {AttemptId} has invalid status: {Status}. Must be SUBMITTED or GRADED.", attemptId, attempt.Status);
                    throw new InvalidOperationException($"Placement test attempt is in status '{attempt.Status}'. It must be submitted or graded before it can be analyzed.");
                }

                // 3. Security & Ownership Check
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == currentUserId);

                if (user == null)
                {
                    _logger.LogWarning("Requesting User ID: {UserId} does not exist in the database.", currentUserId);
                    throw new UnauthorizedAccessException("The user requesting the resource could not be found.");
                }

                bool isOwner = attempt.StudentId == currentUserId;
                bool isAdminOrTeacher = user.Role.RoleCode == "ADMIN" || user.Role.RoleCode == "TEACHER";

                if (!isOwner && !isAdminOrTeacher)
                {
                    _logger.LogWarning("User ID: {UserId} ({Role}) attempted to access test attempt ID: {AttemptId} belonging to Student ID: {StudentId} without permission.",
                        currentUserId, user.Role.RoleCode, attemptId, attempt.StudentId);
                    throw new UnauthorizedAccessException("You are not authorized to access this placement test attempt assessment data.");
                }

                // 4. Fetch Question Points from placement_test_questions
                var testQuestions = await _context.PlacementTestQuestions
                    .Where(pq => pq.Section.PlacementTestId == attempt.PlacementTestId)
                    .ToDictionaryAsync(pq => pq.QuestionId, pq => pq.Points);

                // 5. Initialize Aggregate Containers
                var skillMap = new Dictionary<int, SkillScoreDto>();
                var topicMap = new Dictionary<int, TopicScoreDto>();
                var difficultyMap = new Dictionary<string, DifficultyScoreDto>
                {
                    { "BASIC", new DifficultyScoreDto { DifficultyLevel = "BASIC", EarnedScore = 0, MaxScore = 0, TotalQuestions = 0, CorrectAnswers = 0 } },
                    { "MEDIUM", new DifficultyScoreDto { DifficultyLevel = "MEDIUM", EarnedScore = 0, MaxScore = 0, TotalQuestions = 0, CorrectAnswers = 0 } },
                    { "ADVANCED", new DifficultyScoreDto { DifficultyLevel = "ADVANCED", EarnedScore = 0, MaxScore = 0, TotalQuestions = 0, CorrectAnswers = 0 } }
                };

                var warnings = new List<string>();
                var wrongAnswers = new List<WrongAnswerDto>();
                decimal aggregatedTotalScore = 0;
                decimal aggregatedMaxPossibleScore = 0;

                // 6. Process answers and aggregate statistics
                foreach (var answer in attempt.TestAnswers)
                {
                    var question = answer.Question;
                    if (question == null)
                    {
                        warnings.Add($"TestAnswer ID {answer.Id} is missing reference to a valid Question in the Question Bank.");
                        continue;
                    }

                    decimal maxScore = testQuestions.TryGetValue(question.Id, out var pts) ? pts : 1.0m;
                    decimal earnedScore = answer.Score ?? 0.0m;
                    bool isCorrect = answer.IsCorrect ?? (earnedScore >= maxScore && maxScore > 0);

                    aggregatedTotalScore += earnedScore;
                    aggregatedMaxPossibleScore += maxScore;

                    // Group by Skill
                    if (question.Skill == null)
                    {
                        warnings.Add($"Question ID {question.Id} is missing an English Skill mapping.");
                    }
                    else
                    {
                        var skill = question.Skill;
                        if (!skillMap.TryGetValue(skill.Id, out var skillDto))
                        {
                            skillDto = new SkillScoreDto
                            {
                                SkillId = skill.Id,
                                SkillCode = skill.SkillCode,
                                SkillName = skill.SkillName,
                                EarnedScore = 0,
                                MaxScore = 0,
                                TotalQuestions = 0,
                                CorrectAnswers = 0
                            };
                            skillMap[skill.Id] = skillDto;
                        }

                        skillDto.EarnedScore += earnedScore;
                        skillDto.MaxScore += maxScore;
                        skillDto.TotalQuestions++;
                        if (isCorrect)
                        {
                            skillDto.CorrectAnswers++;
                        }
                    }

                    // Group by Topic
                    if (question.TopicId == null || question.Topic == null)
                    {
                        warnings.Add($"Question ID {question.Id} is missing a Learning Topic mapping.");
                    }
                    else
                    {
                        var topic = question.Topic;
                        if (!topicMap.TryGetValue(topic.Id, out var topicDto))
                        {
                            topicDto = new TopicScoreDto
                            {
                                TopicId = topic.Id,
                                TopicCode = topic.TopicCode ?? $"TOPIC_{topic.Id}",
                                TopicTitle = topic.Title,
                                SkillId = question.SkillId,
                                EarnedScore = 0,
                                MaxScore = 0,
                                TotalQuestions = 0,
                                CorrectAnswers = 0
                            };
                            topicMap[topic.Id] = topicDto;
                        }

                        topicDto.EarnedScore += earnedScore;
                        topicDto.MaxScore += maxScore;
                        topicDto.TotalQuestions++;
                        if (isCorrect)
                        {
                            topicDto.CorrectAnswers++;
                        }
                    }

                    // Group by Difficulty
                    string diffLevel = string.IsNullOrEmpty(question.DifficultyLevel) ? "BASIC" : question.DifficultyLevel.ToUpper();
                    if (!difficultyMap.TryGetValue(diffLevel, out var diffDto))
                    {
                        diffDto = new DifficultyScoreDto
                        {
                            DifficultyLevel = diffLevel,
                            EarnedScore = 0,
                            MaxScore = 0,
                            TotalQuestions = 0,
                            CorrectAnswers = 0
                        };
                        difficultyMap[diffLevel] = diffDto;
                    }
                    diffDto.EarnedScore += earnedScore;
                    diffDto.MaxScore += maxScore;
                    diffDto.TotalQuestions++;
                    if (isCorrect)
                    {
                        diffDto.CorrectAnswers++;
                    }

                    // Record Wrong Answers
                    if (!isCorrect)
                    {
                        string? studentAnswerText = answer.AnswerText;
                        if (answer.SelectedOption != null)
                        {
                            studentAnswerText = answer.SelectedOption.OptionText;
                        }

                        wrongAnswers.Add(new WrongAnswerDto
                        {
                            QuestionId = question.Id,
                            SkillId = question.SkillId,
                            TopicId = question.TopicId,
                            QuestionText = question.QuestionText,
                            QuestionType = question.QuestionType,
                            CorrectAnswerText = question.CorrectAnswer,
                            StudentAnswerText = studentAnswerText,
                            Explanation = question.Explanation
                        });
                    }
                }

                // 7. Calculate Accuracy Percentages
                foreach (var sDto in skillMap.Values)
                {
                    sDto.AccuracyPercentage = sDto.MaxScore > 0
                        ? Math.Round((sDto.EarnedScore / sDto.MaxScore) * 100, 2)
                        : 0;
                }

                foreach (var tDto in topicMap.Values)
                {
                    tDto.AccuracyPercentage = tDto.MaxScore > 0
                        ? Math.Round((tDto.EarnedScore / tDto.MaxScore) * 100, 2)
                        : 0;
                }

                foreach (var dDto in difficultyMap.Values.ToList())
                {
                    // If no questions exist for this difficulty, remove it or leave at 0
                    if (dDto.TotalQuestions == 0)
                    {
                        difficultyMap.Remove(dDto.DifficultyLevel);
                        continue;
                    }

                    dDto.AccuracyPercentage = dDto.MaxScore > 0
                        ? Math.Round((dDto.EarnedScore / dDto.MaxScore) * 100, 2)
                        : 0;
                }

                // 8. Construct Output DTO
                var result = new AssessmentInputDto
                {
                    AttemptId = attempt.Id,
                    StudentId = attempt.StudentId,
                    PlacementTestId = attempt.PlacementTestId,
                    TestTitle = attempt.PlacementTest.Title,
                    StartedAt = attempt.StartedAt,
                    SubmittedAt = attempt.SubmittedAt,
                    TotalScore = attempt.TotalScore ?? aggregatedTotalScore,
                    MaxPossibleScore = attempt.PlacementTest.TotalScore > 0 ? attempt.PlacementTest.TotalScore : aggregatedMaxPossibleScore,
                    SkillScores = skillMap.Values.OrderBy(s => s.SkillId).ToList(),
                    TopicScores = topicMap.Values.OrderBy(t => t.TopicId).ToList(),
                    DifficultyScores = difficultyMap.Values.ToList(),
                    WrongAnswers = wrongAnswers,
                    ValidationWarnings = warnings
                };

                _logger.LogInformation("Successfully completed test result aggregation for Attempt ID: {AttemptId}. Warnings count: {WarningCount}, Wrong answers count: {WrongCount}",
                    attemptId, result.ValidationWarnings.Count, result.WrongAnswers.Count);

                return result;
            }
            catch (Exception ex) when (ex is not KeyNotFoundException && ex is not InvalidOperationException && ex is not UnauthorizedAccessException)
            {
                _logger.LogError(ex, "An unexpected error occurred while aggregating data for test attempt ID: {AttemptId}", attemptId);
                throw;
            }
        }
    }
}
