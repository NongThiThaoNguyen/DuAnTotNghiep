using DuAnTotNghiep.Models;
using System.Linq;

namespace DuAnTotNghiep.DTOs.Onboarding
{
    public static class MappingExtensions
    {
        public static GoalDto? ToDto(this LearningGoal? entity)
        {
            if (entity == null) return null;
            return new GoalDto
            {
                Id = entity.Id,
                GoalCode = entity.GoalCode,
                GoalName = entity.GoalName,
                Description = entity.Description
            };
        }

        public static LevelDto? ToDto(this EnglishProficiencyLevel? entity)
        {
            if (entity == null) return null;
            return new LevelDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                OrderIndex = entity.OrderIndex
            };
        }

        public static SkillDto? ToDto(this EnglishSkill? entity)
        {
            if (entity == null) return null;
            return new SkillDto
            {
                Id = entity.Id,
                SkillCode = entity.SkillCode,
                SkillName = entity.SkillName,
                Description = entity.Description
            };
        }

        public static LearningProfileDto? ToDto(this StudentLearningProfile? entity)
        {
            if (entity == null) return null;
            return new LearningProfileDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                MainGoal = entity.MainGoal?.ToDto(),
                CurrentLevel = entity.CurrentLevel?.ToDto(),
                TargetLevel = entity.TargetLevel?.ToDto(),
                TargetScore = entity.TargetScore,
                DailyStudyMinutes = entity.DailyStudyMinutes,
                WeeklyStudyDays = entity.WeeklyStudyDays,
                PreferredStudyTime = entity.PreferredStudyTime,
                LearningNote = entity.LearningNote,
                OnboardingStatus = entity.OnboardingStatus
                // PrioritySkills should be mapped at service level since StudentSkillPreference only contains SkillCode
            };
        }

        public static LearningProfileSummaryDto? ToSummaryDto(this StudentLearningProfile? entity)
        {
            if (entity == null) return null;
            
            string targetInfo = string.Empty;
            if (entity.TargetScore.HasValue)
            {
                targetInfo = $"{entity.MainGoal?.GoalName} {entity.TargetScore.Value}";
            }
            else if (entity.TargetLevel != null)
            {
                targetInfo = entity.TargetLevel.Name;
            }
            else if (!string.IsNullOrEmpty(entity.LearningNote))
            {
                targetInfo = entity.LearningNote;
            }

            return new LearningProfileSummaryDto
            {
                UserId = entity.UserId,
                MainGoalName = entity.MainGoal?.GoalName ?? "Chưa xác định",
                CurrentLevelName = entity.CurrentLevel?.Name ?? "Chưa xác định",
                TargetInfo = targetInfo,
                OnboardingStatus = entity.OnboardingStatus
            };
        }
    }
}
