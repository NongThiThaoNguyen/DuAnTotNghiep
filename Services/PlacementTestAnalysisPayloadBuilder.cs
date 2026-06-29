using System;
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestAnalysisPayloadBuilder : IPlacementTestAnalysisPayloadBuilder
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ITestScoringService _scoringService;

        public PlacementTestAnalysisPayloadBuilder(ApplicationDbContext dbContext, ITestScoringService scoringService)
        {
            _dbContext = dbContext;
            _scoringService = scoringService;
        }

        public async Task<PlacementTestAnalysisPayload?> BuildPayloadAsync(int attemptId, int studentId)
        {
            // Load Attempt and Answers
            var attempt = await _dbContext.TestAttempts
                .Include(a => a.TestAnswers)
                .ThenInclude(ta => ta.Question)
                .ThenInclude(q => q.Skill)
                .Include(a => a.TestAnswers)
                .ThenInclude(ta => ta.Question)
                .ThenInclude(q => q.Topic)
                .Include(a => a.PlacementTest)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);

            if (attempt == null)
            {
                return null;
            }

            // Load Profile
            var profile = await _dbContext.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .Include(p => p.TargetLevel)
                .Include(p => p.MainGoal)
                .Include(p => p.StudentSkillPreferences)
                .FirstOrDefaultAsync(p => p.UserId == studentId);

            if (profile == null)
            {
                return null; // Cannot analyze without profile
            }

            // Generate Wrong Answers
            var wrongAnswers = attempt.TestAnswers
                .Where(ta => ta.IsCorrect == false)
                .Select(ta => new WrongAnswerDto
                {
                    QuestionId = ta.QuestionId,
                    Skill = ta.Question.Skill?.SkillName ?? "Unknown",
                    Topic = ta.Question.Topic?.Title ?? "Unknown",
                    Difficulty = ta.Question.DifficultyLevel ?? "Unknown",
                    QuestionType = ta.Question.QuestionType
                }).ToList();

            // Load Skill Scores & Topic Scores from existing service
            var skillScores = await _scoringService.CalculateSkillScoresAsync(attemptId);
            var topicScores = await _scoringService.CalculateTopicScoresAsync(attemptId);
            var estimatedLevelDto = await _scoringService.EstimateLevelAsync(attemptId);

            var payload = new PlacementTestAnalysisPayload
            {
                StudentId = studentId,
                AttemptId = attemptId,
                CurrentLevel = profile.CurrentLevel?.Name ?? "Unknown",
                TargetLevel = profile.TargetLevel?.Name ?? "Unknown",
                LearningGoal = profile.MainGoal?.GoalName ?? "Unknown",
                PreferredTopics = profile.StudentSkillPreferences.Select(s => s.SkillCode).ToList(),
                StudyTimePerDay = profile.DailyStudyMinutes,
                TotalScore = attempt.TotalScore ?? 0,
                EstimatedLevel = estimatedLevelDto.LevelName ?? "Unknown",
                SkillScores = skillScores,
                TopicScores = topicScores,
                WrongAnswers = wrongAnswers
            };

            return payload;
        }
    }
}
