using System.Collections.Generic;

namespace DuAnTotNghiep.Models.DTOs.Onboarding
{
    public class LearningProfileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public GoalDto? MainGoal { get; set; }
        public LevelDto? CurrentLevel { get; set; }
        public LevelDto? TargetLevel { get; set; }
        public decimal? TargetScore { get; set; }
        public int? DailyStudyMinutes { get; set; }
        public int? WeeklyStudyDays { get; set; }
        public string? PreferredStudyTime { get; set; }
        public string? LearningNote { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
        public List<SkillDto> PrioritySkills { get; set; } = new List<SkillDto>();
    }
}
