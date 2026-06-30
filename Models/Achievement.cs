using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class Achievement
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string IconUrl { get; set; } = null!;

    public int XpReward { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
