using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.PlacementTest;
using DuAnTotNghiep.DTOs.PlacementTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestService : IPlacementTestService
    {
        private readonly ILearningProfileRepository _profileRepository;
        private readonly IGenericRepository<PlacementTest> _testRepository;
        private readonly IGenericRepository<TestAttempt> _attemptRepository;
        private readonly IGenericRepository<TestAnswer> _answerRepository;
        private readonly ITestScoringService _scoringService;
        private readonly DuAnTotNghiep.Data.ApplicationDbContext _dbContext;
        private readonly DuAnTotNghiep.Services.Background.IAiAnalysisQueue _aiQueue;
        private readonly IAuditService _auditService;

        public PlacementTestService(
            ILearningProfileRepository profileRepository,
            IGenericRepository<PlacementTest> testRepository,
            IGenericRepository<TestAttempt> attemptRepository,
            IGenericRepository<TestAnswer> answerRepository,
            ITestScoringService scoringService,
            DuAnTotNghiep.Data.ApplicationDbContext dbContext,
            DuAnTotNghiep.Services.Background.IAiAnalysisQueue aiQueue,
            IAuditService auditService)
        {
            _profileRepository = profileRepository;
            _testRepository = testRepository;
            _attemptRepository = attemptRepository;
            _answerRepository = answerRepository;
            _scoringService = scoringService;
            _dbContext = dbContext;
            _aiQueue = aiQueue;
            _auditService = auditService;
        }

        public async Task<PlacementTestSuggestionViewModel?> BuildPlacementTestSuggestionAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            var activeTests = (await _testRepository.GetAllAsync())
                .Where(t => t.Status == "ACTIVE" || t.Status == "PUBLISHED")
                .ToList();

            if (!activeTests.Any()) return null;

            PlacementTest? suggestedTest = null;

            // 1. Map theo Goal
            if (profile.MainGoal != null)
            {
                var goalKeyword = profile.MainGoal.GoalCode ?? profile.MainGoal.GoalName;
                if (!string.IsNullOrEmpty(goalKeyword))
                {
                    suggestedTest = activeTests.FirstOrDefault(t => t.Title.Contains(goalKeyword, System.StringComparison.OrdinalIgnoreCase));
                }
            }

            // 2. Fallback sang General nếu không tìm thấy theo Goal
            if (suggestedTest == null)
            {
                suggestedTest = activeTests.FirstOrDefault(t => t.Title.Contains("General", System.StringComparison.OrdinalIgnoreCase));
            }

            // 3. Fallback lấy test đầu tiên
            if (suggestedTest == null)
            {
                suggestedTest = activeTests.FirstOrDefault();
            }

            return new PlacementTestSuggestionViewModel
            {
                GoalName = profile.MainGoal?.GoalName ?? "Chưa xác định",
                CurrentLevelName = profile.CurrentLevel?.Name ?? "Chưa đánh giá",
                PrioritySkills = profile.StudentSkillPreferences.OrderBy(s => s.PriorityLevel).Select(s => s.SkillCode).ToList(),
                SuggestedTestId = suggestedTest!.Id,
                SuggestedTestTitle = suggestedTest.Title,
                SuggestedTestDescription = suggestedTest.Description,
                TimeLimitMinutes = suggestedTest.TimeLimitMinutes
            };
        }

        public async Task<PlacementTestDto?> GetAvailableTestForStudentAsync(int studentId)
        {
            // Simple logic: return the first published test that hasn't been completed by student
            var completedAttempts = (await _attemptRepository.GetAllAsync())
                .Where(a => a.StudentId == studentId && (a.Status == "SUBMITTED" || a.Status == "GRADED"))
                .Select(a => a.PlacementTestId)
                .ToList();

            var availableTest = (await _testRepository.GetAllAsync())
                .FirstOrDefault(t => t.Status == "PUBLISHED" && !completedAttempts.Contains(t.Id));

            if (availableTest == null) return null;

            return new PlacementTestDto
            {
                Id = availableTest.Id,
                Title = availableTest.Title,
                Description = availableTest.Description,
                TargetLevelId = availableTest.TargetLevelId,
                TimeLimitMinutes = availableTest.TimeLimitMinutes,
                TotalScore = availableTest.TotalScore,
                Status = availableTest.Status
            };
        }

        public async Task<bool> CanStartAttemptAsync(int studentId, int placementTestId)
        {
            var test = await _testRepository.GetByIdAsync(placementTestId);
            if (test == null || test.Status != "PUBLISHED")
            {
                return false;
            }

            var allAttempts = await _attemptRepository.GetAllAsync();
            var existingCompleted = allAttempts
                .Any(a => a.StudentId == studentId && a.PlacementTestId == placementTestId && 
                         (a.Status == "SUBMITTED" || a.Status == "GRADED"));

            if (existingCompleted)
            {
                return false;
            }

            return true;
        }

        public async Task<TestAttemptDto?> GetCurrentAttemptAsync(int studentId, int placementTestId)
        {
            var allAttempts = await _attemptRepository.GetAllAsync();
            var latestAttempt = allAttempts
                .Where(a => a.StudentId == studentId && a.PlacementTestId == placementTestId)
                .OrderByDescending(a => a.StartedAt)
                .FirstOrDefault();

            if (latestAttempt == null) return null;

            return new TestAttemptDto
            {
                Id = latestAttempt.Id,
                PlacementTestId = latestAttempt.PlacementTestId,
                StudentId = latestAttempt.StudentId,
                StartedAt = latestAttempt.StartedAt,
                SubmittedAt = latestAttempt.SubmittedAt,
                TotalScore = latestAttempt.TotalScore,
                EstimatedLevelId = latestAttempt.EstimatedLevelId,
                Status = latestAttempt.Status
            };
        }

        public async Task<TestTakingViewModel?> GetTestTakingViewModelAsync(int attemptId, int studentId)
        {
            var attemptInfo = await _dbContext.TestAttempts
                .Where(a => a.Id == attemptId && a.StudentId == studentId)
                .Select(a => new
                {
                    a.Id,
                    a.StartedAt,
                    a.Status,
                    TestTitle = a.PlacementTest.Title,
                    TimeLimitMinutes = a.PlacementTest.TimeLimitMinutes,
                    Sections = a.PlacementTest.PlacementTestSections
                        .OrderBy(s => s.OrderIndex)
                        .Select(s => new TestSectionViewModel
                        {
                            SectionId = s.Id,
                            SectionTitle = s.SectionName,
                            Instruction = s.Instruction,
                            OrderIndex = s.OrderIndex,
                            SkillName = s.Skill.SkillName,
                            Questions = s.PlacementTestQuestions
                                .OrderBy(pq => pq.OrderIndex)
                                .Select(pq => new TestQuestionViewModel
                                {
                                    QuestionId = pq.Question.Id,
                                    QuestionText = pq.Question.QuestionText,
                                    QuestionType = pq.Question.QuestionType,
                                    AudioUrl = pq.Question.AudioUrl,
                                    ImageUrl = pq.Question.ImageUrl,
                                    Points = pq.Points,
                                    OrderIndex = pq.OrderIndex,
                                    ExistingAnswer = a.TestAnswers
                                        .Where(ta => ta.QuestionId == pq.Question.Id)
                                        .Select(ta => ta.SelectedOptionId.HasValue ? ta.SelectedOptionId.Value.ToString() : ta.AnswerText)
                                        .FirstOrDefault(),
                                    Options = pq.Question.QuestionOptions
                                        .OrderBy(o => o.OrderIndex)
                                        .Select(o => new QuestionOptionViewModel
                                        {
                                            Id = o.Id,
                                            Text = o.OptionText
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (attemptInfo == null) return null;

            var serverTime = DateTime.UtcNow;

            // Fix Timezone bug: specify UTC kind if Unspecified
            var startedAtUtc = attemptInfo.StartedAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(attemptInfo.StartedAt, DateTimeKind.Utc)
                : attemptInfo.StartedAt;

            // Fix TimeLimitMinutes == 0 meaning no limit
            DateTime? endTime = (attemptInfo.TimeLimitMinutes.HasValue && attemptInfo.TimeLimitMinutes.Value > 0) 
                ? startedAtUtc.AddMinutes(attemptInfo.TimeLimitMinutes.Value) 
                : null;

            int? remainingSeconds = null;
            if (endTime.HasValue)
            {
                remainingSeconds = (int)(endTime.Value - serverTime).TotalSeconds;
                if (remainingSeconds < 0) remainingSeconds = 0;
            }

            // Debug log
            System.Console.WriteLine($"[DEBUG PlacementTest] StartedAt: {attemptInfo.StartedAt}, EndTime: {endTime}, ServerTime: {serverTime}, TimeLimitMinutes: {attemptInfo.TimeLimitMinutes}, RemainingSeconds: {remainingSeconds}, AttemptStatus: {attemptInfo.Status}");

            // Fix Infinite Loop: If expired, mark as EXPIRED so RequirePlacementTestFilter doesn't keep redirecting
            var currentStatus = attemptInfo.Status;
            if (remainingSeconds == 0 && currentStatus == "IN_PROGRESS")
            {
                var attemptToUpdate = await _dbContext.TestAttempts.FindAsync(attemptId);
                if (attemptToUpdate != null)
                {
                    attemptToUpdate.Status = "EXPIRED";
                    _dbContext.TestAttempts.Update(attemptToUpdate);
                    await _dbContext.SaveChangesAsync();
                    currentStatus = "EXPIRED";
                    
                    System.Console.WriteLine($"[DEBUG PlacementTest] Attempt {attemptId} status updated to EXPIRED due to remainingSeconds == 0");
                }
            }

            return new TestTakingViewModel
            {
                AttemptId = attemptInfo.Id,
                TestTitle = attemptInfo.TestTitle,
                ServerTime = serverTime,
                EndTime = endTime,
                RemainingSeconds = remainingSeconds,
                Status = currentStatus,
                Sections = attemptInfo.Sections
            };
        }

        public async Task<SaveAnswerResultDto> SaveAnswerAsync(SaveAnswerInputDto input, int studentId)
        {
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.PlacementTest)
                .ThenInclude(pt => pt.PlacementTestSections)
                .ThenInclude(s => s.PlacementTestQuestions)
                .ThenInclude(pq => pq.Question)
                .FirstOrDefaultAsync(a => a.Id == input.AttemptId);

            if (attempt == null)
            {
                return new SaveAnswerResultDto { Success = false, Message = "Attempt not found", SavedAt = DateTime.UtcNow };
            }

            if (attempt.StudentId != studentId)
            {
                return new SaveAnswerResultDto { Success = false, Message = "Forbidden: Cannot save answer for another student's attempt", SavedAt = DateTime.UtcNow };
            }

            if (attempt.Status != "IN_PROGRESS")
            {
                return new SaveAnswerResultDto { Success = false, Message = $"Cannot save answer because attempt is {attempt.Status}", SavedAt = DateTime.UtcNow };
            }

            // Verify that the QuestionId belongs to this placement test
            var questionBelongsToTest = attempt.PlacementTest.PlacementTestSections
                .SelectMany(s => s.PlacementTestQuestions)
                .Any(pq => pq.QuestionId == input.QuestionId);

            if (!questionBelongsToTest)
            {
                return new SaveAnswerResultDto { Success = false, Message = "Question does not belong to this test", SavedAt = DateTime.UtcNow };
            }

            var existingAnswer = await _dbContext.TestAnswers
                .FirstOrDefaultAsync(ta => ta.AttemptId == input.AttemptId && ta.QuestionId == input.QuestionId);

            var questionType = attempt.PlacementTest.PlacementTestSections
                .SelectMany(s => s.PlacementTestQuestions)
                .First(pq => pq.QuestionId == input.QuestionId).Question.QuestionType;

            int? selectedOptionId = null;
            string? answerText = null;

            if (questionType == "SHORT_ANSWER")
            {
                answerText = input.AnswerValue;
            }
            else
            {
                if (int.TryParse(input.AnswerValue, out int optId))
                {
                    selectedOptionId = optId;
                }
            }

            if (existingAnswer != null)
            {
                existingAnswer.SelectedOptionId = selectedOptionId;
                existingAnswer.AnswerText = answerText;
                existingAnswer.AnsweredAt = DateTime.UtcNow;
                _dbContext.TestAnswers.Update(existingAnswer);
            }
            else
            {
                var newAnswer = new TestAnswer
                {
                    AttemptId = input.AttemptId,
                    QuestionId = input.QuestionId,
                    SelectedOptionId = selectedOptionId,
                    AnswerText = answerText,
                    AnsweredAt = DateTime.UtcNow
                };
                _dbContext.TestAnswers.Add(newAnswer);
            }

            await _dbContext.SaveChangesAsync();

            return new SaveAnswerResultDto
            {
                Success = true,
                Message = "Saved successfully",
                SavedAt = DateTime.UtcNow
            };
        }

        public async Task<TestAttemptDto> StartAttemptAsync(int studentId, int placementTestId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var canStart = await CanStartAttemptAsync(studentId, placementTestId);
                if (!canStart)
                {
                    throw new InvalidOperationException("Cannot start attempt. Test may not be published or you have already completed it.");
                }

                var allAttempts = await _attemptRepository.GetAllAsync();
                var existingAttempt = allAttempts
                    .FirstOrDefault(a => a.StudentId == studentId && a.PlacementTestId == placementTestId && a.Status == "IN_PROGRESS");

                if (existingAttempt != null)
                {
                    var test = await _testRepository.GetByIdAsync(placementTestId);
                    if (test!.TimeLimitMinutes.HasValue && test.TimeLimitMinutes.Value > 0)
                    {
                        var expireTime = existingAttempt.StartedAt.AddMinutes(test.TimeLimitMinutes.Value);
                        if (DateTime.UtcNow > expireTime)
                        {
                            existingAttempt.Status = "EXPIRED";
                            _dbContext.TestAttempts.Update(existingAttempt);
                            await _dbContext.SaveChangesAsync();
                            // Continue to create new attempt below
                        }
                        else
                        {
                            await transaction.CommitAsync();
                            return new TestAttemptDto
                            {
                                Id = existingAttempt.Id,
                                PlacementTestId = existingAttempt.PlacementTestId,
                                StudentId = existingAttempt.StudentId,
                                StartedAt = existingAttempt.StartedAt,
                                Status = existingAttempt.Status
                            };
                        }
                    }
                    else
                    {
                        await transaction.CommitAsync();
                        return new TestAttemptDto
                        {
                            Id = existingAttempt.Id,
                            PlacementTestId = existingAttempt.PlacementTestId,
                            StudentId = existingAttempt.StudentId,
                            StartedAt = existingAttempt.StartedAt,
                            Status = existingAttempt.Status
                        };
                    }
                }

                // Create new
                var newAttempt = new TestAttempt
                {
                    StudentId = studentId,
                    PlacementTestId = placementTestId,
                    StartedAt = DateTime.UtcNow,
                    Status = "IN_PROGRESS"
                };

                await _attemptRepository.AddAsync(newAttempt);
                await _attemptRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                await _auditService.LogAsync(studentId, "START_ATTEMPT", "TestAttempt", newAttempt.Id);

                return new TestAttemptDto
                {
                    Id = newAttempt.Id,
                    PlacementTestId = newAttempt.PlacementTestId,
                    StudentId = newAttempt.StudentId,
                    StartedAt = newAttempt.StartedAt,
                    Status = newAttempt.Status
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<SubmitResultDto> SubmitAttemptAsync(int attemptId, int studentId, List<AnswerInputDto> answers)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var attempt = await _dbContext.TestAttempts
                    .Include(a => a.PlacementTest)
                    .FirstOrDefaultAsync(a => a.Id == attemptId);
                    
                if (attempt == null || attempt.StudentId != studentId)
                {
                    return new SubmitResultDto { IsSuccess = false, Message = "Attempt invalid." };
                }

                if (attempt.Status == "SUBMITTED" || attempt.Status == "GRADED" || attempt.Status == "EXPIRED")
                {
                    return new SubmitResultDto { IsSuccess = false, Message = $"Attempt already {attempt.Status.ToLower()}." };
                }

                // Server-side Timer validation (Grace period: 2 minutes)
                if (attempt.PlacementTest.TimeLimitMinutes.HasValue && attempt.PlacementTest.TimeLimitMinutes.Value > 0)
                {
                    var startedAtUtc = attempt.StartedAt.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(attempt.StartedAt, DateTimeKind.Utc)
                        : attempt.StartedAt;
                        
                    var maxEndTime = startedAtUtc.AddMinutes(attempt.PlacementTest.TimeLimitMinutes.Value + 2); // 2 minutes grace
                    if (DateTime.UtcNow > maxEndTime)
                    {
                        attempt.Status = "EXPIRED";
                        _dbContext.TestAttempts.Update(attempt);
                        await _dbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                        return new SubmitResultDto { IsSuccess = false, Message = "Thời gian làm bài đã hết. Bài thi của bạn không được công nhận." };
                    }
                }

                // Luôn cập nhật trạng thái Submit trước khi chấm
                attempt.SubmittedAt = System.DateTime.UtcNow;
                attempt.Status = "SUBMITTED";
                _dbContext.TestAttempts.Update(attempt);

                // Insert answers
                foreach (var input in answers)
                {
                    var ans = new TestAnswer
                    {
                        AttemptId = attemptId,
                        QuestionId = input.QuestionId,
                        SelectedOptionId = input.SelectedOptionId,
                        AnswerText = input.AnswerText,
                        AnsweredAt = System.DateTime.UtcNow
                    };
                    await _answerRepository.AddAsync(ans);
                }
                
                await _dbContext.SaveChangesAsync();

                // Log Submit
                await _auditService.LogAsync(studentId, "SUBMIT_ATTEMPT", "TestAttempt", attemptId);

                // Chấm bài
                var scoreResult = await _scoringService.GradeAttemptAsync(attemptId);
                var estimatedLevel = await _scoringService.EstimateLevelAsync(attemptId);

                // Lưu kết quả điểm vào Attempt
                attempt.TotalScore = scoreResult.TotalScore;
                attempt.EstimatedLevelId = estimatedLevel.LevelId;
                attempt.Status = "GRADED";
                _dbContext.TestAttempts.Update(attempt);
                
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Log Grade
                await _auditService.LogAsync(studentId, "GRADE_ATTEMPT", "TestAttempt", attemptId, null, $"Score: {scoreResult.TotalScore}");

                // Queue AI Analysis
                await _aiQueue.QueueAttemptForAnalysisAsync(attemptId, studentId);

                return new SubmitResultDto
                {
                    IsSuccess = true,
                    Message = "Submitted successfully",
                    ScoreResult = scoreResult
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new SubmitResultDto { IsSuccess = false, Message = "Failed to submit: " + ex.Message };
            }
        }

        public async Task<TestResultViewModel> GetResultForStudentAsync(int attemptId, int studentId)
        {
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.PlacementTest)
                .Include(a => a.TestAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null)
            {
                throw new KeyNotFoundException("Attempt not found");
            }

            if (attempt.StudentId != studentId)
            {
                throw new UnauthorizedAccessException("Forbidden: Cannot view another student's attempt");
            }

            if (attempt.Status == "IN_PROGRESS")
            {
                throw new InvalidOperationException("IN_PROGRESS");
            }

            var estimatedLevel = await _scoringService.EstimateLevelAsync(attemptId);
            var skillScores = await _scoringService.CalculateSkillScoresAsync(attemptId);
            var topicScores = await _scoringService.CalculateTopicScoresAsync(attemptId);

            var weakestSkill = skillScores.OrderBy(s => s.Percentage).FirstOrDefault()?.SkillName;
            var weakestTopic = topicScores.OrderBy(t => t.Percentage).FirstOrDefault()?.TopicName;

            var correctAnswers = attempt.TestAnswers.Count(a => a.IsCorrect == true);
            var incorrectAnswers = attempt.TestAnswers.Count(a => a.IsCorrect == false);
            var totalQuestions = attempt.TestAnswers.Count();

            // AI Status
            var aiAnalysis = await _dbContext.CompetencyAnalyses.FirstOrDefaultAsync(c => c.TestAttemptId == attemptId);
            bool aiCompleted = aiAnalysis != null;
            string aiStatus = aiCompleted ? "Completed" : "Analyzing";

            if (!aiCompleted)
            {
                var aiLog = await _dbContext.AiUsageLogs
                    .FirstOrDefaultAsync(l => l.UserId == studentId && l.ModuleCode == $"M6_ATTEMPT_{attemptId}");
                if (aiLog != null)
                {
                    aiStatus = aiLog.RequestStatus switch
                    {
                        "PENDING" => "Pending",
                        "RUNNING" => "Running",
                        "FAILED" => "Failed",
                        _ => "Analyzing"
                    };
                }
            }

            return new TestResultViewModel
            {
                AttemptId = attempt.Id,
                TotalScore = attempt.TotalScore ?? 0,
                MaxScore = attempt.PlacementTest.TotalScore,
                Percentage = estimatedLevel.Percentage,
                EstimatedLevel = estimatedLevel.LevelName ?? "Chưa đánh giá",
                SkillScores = skillScores.OrderByDescending(s => s.Percentage).ToList(),
                TopicScores = topicScores.OrderByDescending(t => t.Percentage).ToList(),
                WeakestSkill = weakestSkill,
                WeakestTopic = weakestTopic,
                CorrectAnswers = correctAnswers,
                IncorrectAnswers = incorrectAnswers,
                TotalQuestions = totalQuestions,
                AiCompleted = aiCompleted,
                AiAnalysisStatus = aiStatus,
                SubmittedAt = attempt.SubmittedAt
            };
        }
    }
}
