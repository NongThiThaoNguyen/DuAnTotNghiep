using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.ViewModels.LearningPath;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class LevelUpAssessmentService : ILevelUpAssessmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<LevelUpAssessmentService> _logger;

        public LevelUpAssessmentService(
            ApplicationDbContext context,
            IAuditService auditService,
            ILogger<LevelUpAssessmentService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<LevelUpAssessmentViewModel?> GetLevelUpAssessmentAsync(int studentId)
        {
            var profile = await _context.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .FirstOrDefaultAsync(p => p.UserId == studentId);

            if (profile == null) return null;

            int currentLevelOrder = profile.CurrentLevel?.OrderIndex ?? 1;

            var nextLevel = await _context.EnglishProficiencyLevels
                .Where(l => l.OrderIndex > currentLevelOrder && l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefaultAsync();

            if (nextLevel == null)
            {
                // Đã đạt trình độ cao nhất
                nextLevel = profile.CurrentLevel;
            }

            // Tìm bài test đánh giá phù hợp với trình độ mục tiêu
            var placementTest = await _context.PlacementTests
                .Include(pt => pt.PlacementTestSections)
                    .ThenInclude(s => s.Skill)
                .Include(pt => pt.PlacementTestSections)
                    .ThenInclude(s => s.PlacementTestQuestions)
                        .ThenInclude(pq => pq.Question)
                            .ThenInclude(q => q.QuestionOptions)
                .Where(pt => (pt.Status == "ACTIVE" || pt.Status == "PUBLISHED"))
                .OrderByDescending(pt => pt.TargetLevelId == nextLevel!.Id)
                .ThenByDescending(pt => pt.CreatedAt)
                .FirstOrDefaultAsync();

            if (placementTest == null)
            {
                return null;
            }

            var sections = placementTest.PlacementTestSections
                .OrderBy(s => s.OrderIndex)
                .Select(s => new LevelUpSectionViewModel
                {
                    SectionId = s.Id,
                    SectionName = s.SectionName,
                    SkillName = s.Skill?.SkillName ?? "Tổng hợp",
                    Instruction = s.Instruction,
                    Questions = s.PlacementTestQuestions
                        .OrderBy(pq => pq.OrderIndex)
                        .Select(pq => new LevelUpQuestionViewModel
                        {
                            QuestionId = pq.QuestionId,
                            QuestionText = pq.Question.QuestionText,
                            QuestionType = pq.Question.QuestionType,
                            Points = pq.Points,
                            Options = pq.Question.QuestionOptions
                                .OrderBy(o => o.OrderIndex)
                                .Select(o => new LevelUpOptionViewModel
                                {
                                    OptionId = o.Id,
                                    OptionText = o.OptionText
                                }).ToList()
                        }).ToList()
                }).ToList();

            return new LevelUpAssessmentViewModel
            {
                TestId = placementTest.Id,
                TestTitle = $"Bài Đánh Giá Thăng Hạng Lên Trình Độ {nextLevel!.Name}",
                Description = placementTest.Description ?? $"Hoàn thành bài thi đánh giá năng lực toàn diện để chính thức thăng hạng lên trình độ {nextLevel.Name}.",
                TimeLimitMinutes = placementTest.TimeLimitMinutes ?? 45,
                TotalScore = placementTest.TotalScore,
                CurrentLevelName = profile.CurrentLevel?.Name ?? "A1",
                TargetLevelName = nextLevel.Name,
                TargetLevelId = nextLevel.Id,
                Sections = sections
            };
        }

        public async Task<LevelUpResultViewModel> SubmitLevelUpAssessmentAsync(int studentId, int testId, Dictionary<int, string> answers)
        {
            var test = await _context.PlacementTests
                .Include(pt => pt.PlacementTestSections)
                    .ThenInclude(s => s.Skill)
                .Include(pt => pt.PlacementTestSections)
                    .ThenInclude(s => s.PlacementTestQuestions)
                        .ThenInclude(pq => pq.Question)
                            .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(pt => pt.Id == testId);

            if (test == null) throw new ArgumentException("Bài kiểm tra không tồn tại.");

            var profile = await _context.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .FirstOrDefaultAsync(p => p.UserId == studentId);

            if (profile == null) throw new InvalidOperationException("Không tìm thấy hồ sơ học tập.");

            int currentLevelOrder = profile.CurrentLevel?.OrderIndex ?? 1;
            var nextLevel = await _context.EnglishProficiencyLevels
                .Where(l => l.OrderIndex > currentLevelOrder && l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefaultAsync();

            decimal totalEarnedPoints = 0;
            decimal totalMaxPoints = 0;

            var skillBreakdown = new List<LevelUpSkillScoreViewModel>();

            foreach (var section in test.PlacementTestSections)
            {
                decimal sectionEarned = 0;
                decimal sectionMax = 0;

                foreach (var pq in section.PlacementTestQuestions)
                {
                    sectionMax += pq.Points;
                    answers.TryGetValue(pq.QuestionId, out string? studentAnswer);

                    if (!string.IsNullOrEmpty(studentAnswer))
                    {
                        var correctOpt = pq.Question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                        if (correctOpt != null && studentAnswer.Trim() == correctOpt.Id.ToString())
                        {
                            sectionEarned += pq.Points;
                        }
                        else if (pq.Question.QuestionType == "SHORT_ANSWER" &&
                                 string.Equals(studentAnswer.Trim(), correctOpt?.OptionText.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            sectionEarned += pq.Points;
                        }
                    }
                }

                totalEarnedPoints += sectionEarned;
                totalMaxPoints += sectionMax;

                decimal sectionPct = sectionMax > 0 ? Math.Round(sectionEarned / sectionMax * 100m, 1) : 0;
                skillBreakdown.Add(new LevelUpSkillScoreViewModel
                {
                    SkillName = section.Skill?.SkillName ?? section.SectionName,
                    Score = sectionEarned,
                    MaxScore = sectionMax,
                    Percentage = sectionPct,
                    Status = sectionPct >= 70m ? "Đạt" : "Cần củng cố"
                });
            }

            decimal totalPercentage = totalMaxPoints > 0 ? Math.Round(totalEarnedPoints / totalMaxPoints * 100m, 1) : 0;
            bool passed = totalPercentage >= 70m && nextLevel != null;

            string currentLevelName = profile.CurrentLevel?.Name ?? "A1";

            if (passed && nextLevel != null)
            {
                // Thăng hạng trình độ
                profile.CurrentLevelId = nextLevel.Id;
                profile.UpdatedAt = DateTime.UtcNow;
                _context.StudentLearningProfiles.Update(profile);

                // Lưu TestAttempt
                var attempt = new TestAttempt
                {
                    StudentId = studentId,
                    PlacementTestId = testId,
                    StartedAt = DateTime.UtcNow.AddMinutes(-30),
                    SubmittedAt = DateTime.UtcNow,
                    TotalScore = totalPercentage,
                    EstimatedLevelId = nextLevel.Id,
                    Status = "GRADED"
                };
                await _context.TestAttempts.AddAsync(attempt);

                // Hoàn thành & lưu trữ lộ trình cũ
                var activePath = await _context.StudentLearningPaths
                    .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "ACTIVE");

                if (activePath != null)
                {
                    activePath.Status = "COMPLETED";
                    activePath.UpdatedAt = DateTime.UtcNow;
                    _context.StudentLearningPaths.Update(activePath);
                }

                await _context.SaveChangesAsync();
                await _auditService.LogAsync(studentId, "LEVEL_UP_PROMOTION", "StudentLearningProfile", profile.Id, currentLevelName, nextLevel.Name);

                _logger.LogInformation("Student {StudentId} passed level-up assessment and was promoted to {NewLevel}", studentId, nextLevel.Name);

                return new LevelUpResultViewModel
                {
                    IsSuccess = true,
                    Promoted = true,
                    EarnedScore = totalEarnedPoints,
                    MaxScore = totalMaxPoints,
                    Percentage = totalPercentage,
                    CurrentLevelName = currentLevelName,
                    NewLevelName = nextLevel.Name,
                    Message = $"Chúc mừng bạn! Bạn đã xuất sắc vượt qua bài đánh giá với {totalPercentage}% điểm và chính thức thăng hạng lên trình độ {nextLevel.Name}.",
                    SkillBreakdown = skillBreakdown
                };
            }
            else
            {
                // Chưa đạt điểm yêu cầu
                return new LevelUpResultViewModel
                {
                    IsSuccess = true,
                    Promoted = false,
                    EarnedScore = totalEarnedPoints,
                    MaxScore = totalMaxPoints,
                    Percentage = totalPercentage,
                    CurrentLevelName = currentLevelName,
                    NewLevelName = null,
                    Message = $"Bạn đạt {totalPercentage}%. Điểm yêu cầu để thăng hạng là 70%. Hãy ôn tập lại các phần kỹ năng chưa đạt và thử lại nhé!",
                    SkillBreakdown = skillBreakdown
                };
            }
        }
    }
}
