using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class NotificationRead
{
    public int Id { get; set; }

    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public DateTime ReadAt { get; set; }

    public virtual Notification Notification { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
