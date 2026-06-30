using System;

namespace DuAnTotNghiep.Models;

public partial class UserAchievement
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AchievementId { get; set; }

    public bool IsUnlocked { get; set; }

    public DateTime? UnlockedAt { get; set; }

    public int ProgressValue { get; set; }

    public int TargetValue { get; set; }

    public virtual Achievement Achievement { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
