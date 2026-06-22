using System;

namespace DuAnTotNghiep.Models;

public partial class UserSetting
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Language { get; set; }

    public string? Timezone { get; set; }

    public bool EmailNotifications { get; set; }

    public bool StudyReminderEnabled { get; set; }

    public string? Theme { get; set; }

    public virtual User User { get; set; } = null!;
}
