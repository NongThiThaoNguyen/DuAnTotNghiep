using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class Notification
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public int? TargetUserId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<NotificationRead> NotificationReads { get; set; } = new List<NotificationRead>();

    public virtual User? TargetUser { get; set; }
}
