using System;

namespace DuAnTotNghiep.ViewModels.Profile;

public class ProfileViewModel
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    
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
}
