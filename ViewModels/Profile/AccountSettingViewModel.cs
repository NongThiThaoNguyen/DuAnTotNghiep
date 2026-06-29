using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Profile;

public class AccountSettingViewModel
{
    [MaxLength(50)]
    [Display(Name = "Ngôn ngữ")]
    public string? Language { get; set; }

    [MaxLength(50)]
    [Display(Name = "Múi giờ")]
    public string? Timezone { get; set; }

    [Display(Name = "Nhận thông báo qua Email")]
    public bool EmailNotifications { get; set; }

    [Display(Name = "Nhắc nhở học tập")]
    public bool StudyReminderEnabled { get; set; }

    [MaxLength(20)]
    [Display(Name = "Giao diện (Theme)")]
    public string? Theme { get; set; }
}
