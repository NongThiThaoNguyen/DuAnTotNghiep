using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AuditLog
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? EntityName { get; set; }

    public int? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
