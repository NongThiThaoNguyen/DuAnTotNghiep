using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class LoginLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsSuccess { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
