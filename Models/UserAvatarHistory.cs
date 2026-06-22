using System;

namespace DuAnTotNghiep.Models;

public partial class UserAvatarHistory
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? OldAvatarUrl { get; set; }

    public string? NewAvatarUrl { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
