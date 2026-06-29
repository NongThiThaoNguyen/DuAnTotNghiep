using System.Collections.Generic;
using DuAnTotNghiep.DTOs.Onboarding;

namespace DuAnTotNghiep.ViewModels.Onboarding
{
    public class LearningProfileViewModel
    {
        public GoalDto? MainGoal { get; set; }
        public LevelDto? CurrentLevel { get; set; }
        public LevelDto? TargetLevel { get; set; }
        public decimal? TargetScore { get; set; }
        public List<SkillDto> PrioritySkills { get; set; } = new List<SkillDto>();
        public int? DailyStudyMinutes { get; set; }
        public int? WeeklyStudyDays { get; set; }
        public string? PreferredStudyTime { get; set; }
        public string? LearningNote { get; set; }
        public string OnboardingStatus { get; set; } = string.Empty;
    }
}
