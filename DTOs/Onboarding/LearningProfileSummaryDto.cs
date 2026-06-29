namespace DuAnTotNghiep.DTOs.Onboarding
{
    public class LearningProfileSummaryDto
    {
        public int UserId { get; set; }
        public string MainGoalName { get; set; } = string.Empty;
        public string CurrentLevelName { get; set; } = string.Empty;
        public string TargetInfo { get; set; } = string.Empty;
        public string OnboardingStatus { get; set; } = string.Empty;
    }
}
