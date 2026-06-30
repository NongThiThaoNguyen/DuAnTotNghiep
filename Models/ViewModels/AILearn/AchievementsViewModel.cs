using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class AchievementsViewModel
{
    public int EarnedCount { get; set; }
    public int TotalCount => Badges.Count;
    public double ProgressPercent => TotalCount > 0 ? (double)EarnedCount / TotalCount * 100 : 0;
    
    public List<AchievementBadgeViewModel> Badges { get; set; } = new();
}

public class AchievementBadgeViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string IconUrl { get; set; } = "";
    public int XpReward { get; set; }
    public bool IsUnlocked { get; set; }
    public string? UnlockedAtLabel { get; set; }
    public int ProgressValue { get; set; }
    public int TargetValue { get; set; }
}
