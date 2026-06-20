using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class UserSession
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string SessionToken { get; set; } = null!;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? DeviceType { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
