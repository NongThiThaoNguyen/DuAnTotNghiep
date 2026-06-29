using System;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class AdminUserProfileViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string? Phone { get; set; }
        
        public string RoleName { get; set; } = null!;
        public string Status { get; set; } = null!;

        // Profile details
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Country { get; set; }
        public string? Bio { get; set; }

        // Settings
        public string? Language { get; set; }
        public string? Timezone { get; set; }
        public bool EmailNotifications { get; set; }
        public bool StudyReminderEnabled { get; set; }
        public string? Theme { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginCount { get; set; }
        public DateTime? LockoutUntil { get; set; }
    }
}
