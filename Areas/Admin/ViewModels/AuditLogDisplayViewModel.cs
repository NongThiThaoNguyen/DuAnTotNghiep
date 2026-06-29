using System;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class AuditLogDisplayViewModel
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? UserId { get; set; }
        public string? ActorName { get; set; }
        public string? ActorRole { get; set; }
        public string ActionType { get; set; } = null!;
        public int? EntityId { get; set; }
        public string? IpAddress { get; set; }
        public string FriendlyMessage { get; set; } = null!;
    }
}
