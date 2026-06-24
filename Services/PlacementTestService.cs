using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.PlacementTest;
using DuAnTotNghiep.DTOs.PlacementTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public PlacementTestService(
            ILearningProfileRepository profileRepository,
            IGenericRepository<PlacementTest> testRepository,
            IGenericRepository<TestAttempt> attemptRepository,
            IGenericRepository<TestAnswer> answerRepository,
            ITestScoringService scoringService,
            DuAnTotNghiep.Data.ApplicationDbContext dbContext)
        {
            _profileRepository = profileRepository;
            _testRepository = testRepository;
            _attemptRepository = attemptRepository;
            _answerRepository = answerRepository;
            _scoringService = scoringService;
            _dbContext = dbContext;
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

        public async Task<TestAttemptDto> StartAttemptAsync(int studentId, int placementTestId)
        {
            var test = await _testRepository.GetByIdAsync(placementTestId);
            if (test == null || test.Status != "PUBLISHED")
            {
                throw new Exception("Test is not available.");
            }

            var allAttempts = await _attemptRepository.GetAllAsync();
            var existingAttempt = allAttempts
                .FirstOrDefault(a => a.StudentId == studentId && a.PlacementTestId == placementTestId && a.Status == "IN_PROGRESS");

            if (existingAttempt != null)
            {
                return new TestAttemptDto
                {
                    Id = existingAttempt.Id,
                    PlacementTestId = existingAttempt.PlacementTestId,
                    StudentId = existingAttempt.StudentId,
                    StartedAt = existingAttempt.StartedAt,
                    Status = existingAttempt.Status
                };
            }

            // Create new
            var newAttempt = new TestAttempt
            {
                StudentId = studentId,
                PlacementTestId = placementTestId,
                StartedAt = System.DateTime.UtcNow,
                Status = "IN_PROGRESS"
            };

            await _attemptRepository.AddAsync(newAttempt);
            await _attemptRepository.SaveChangesAsync();

            return new TestAttemptDto
            {
                Id = newAttempt.Id,
                PlacementTestId = newAttempt.PlacementTestId,
                StudentId = newAttempt.StudentId,
                StartedAt = newAttempt.StartedAt,
                Status = newAttempt.Status
            };
        }

        public async Task<SubmitResultDto> SubmitAttemptAsync(int attemptId, int studentId, List<AnswerInputDto> answers)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var attempt = await _dbContext.TestAttempts.FindAsync(attemptId);
                if (attempt == null || attempt.StudentId != studentId || attempt.Status != "IN_PROGRESS")
                {
                    return new SubmitResultDto { IsSuccess = false, Message = "Attempt invalid or already submitted." };
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
    }
}
