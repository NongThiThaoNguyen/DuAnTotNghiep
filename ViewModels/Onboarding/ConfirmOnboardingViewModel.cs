using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels.Onboarding;

public class ConfirmOnboardingViewModel
{
    public string GoalSummary { get; set; } = null!;
    
    public string LevelSummary { get; set; } = null!;
    
    public List<string> SkillSummary { get; set; } = new List<string>();
    
    public string StudyTimeSummary { get; set; } = null!;
}
