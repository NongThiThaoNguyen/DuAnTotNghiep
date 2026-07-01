using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class AchievementFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã huy hiệu không được để trống")]
        [StringLength(50, ErrorMessage = "Mã huy hiệu không được vượt quá 50 ký tự")]
        [Display(Name = "Mã huy hiệu")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Tên huy hiệu không được để trống")]
        [StringLength(200, ErrorMessage = "Tên huy hiệu không được vượt quá 200 ký tự")]
        [Display(Name = "Tên huy hiệu")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Mô tả không được để trống")]
        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Icon URL không được để trống")]
        [Display(Name = "Icon URL")]
        public string IconUrl { get; set; } = null!;

        [Required(ErrorMessage = "Điểm kinh nghiệm thưởng không được để trống")]
        [Range(0, 10000, ErrorMessage = "XP phải từ 0 đến 10,000")]
        [Display(Name = "XP Thưởng")]
        public int XpReward { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;
    }

    public class UserAchievementDetailViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string AvatarUrl { get; set; } = null!;

        public List<UserAchievementItem> Achievements { get; set; } = new List<UserAchievementItem>();
    }

    public class UserAchievementItem
    {
        public int AchievementId { get; set; }
        public string Title { get; set; } = null!;
        public string IconUrl { get; set; } = null!;
        public bool IsUnlocked { get; set; }
        public string? UnlockedAtFormatted { get; set; }
    }
}
