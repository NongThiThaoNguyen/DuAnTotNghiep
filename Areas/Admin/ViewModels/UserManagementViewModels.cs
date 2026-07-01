using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class ChangeRoleViewModel
    {
        public int UserId { get; set; }

        [Display(Name = "Current Role")]
        public int CurrentRoleId { get; set; }

        [Required(ErrorMessage = "Please select a new role.")]
        [Display(Name = "New Role")]
        public int NewRoleId { get; set; }

        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }

    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} đến {1} ký tự", MinimumLength = 6)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserStatisticsViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int QuizzesCompleted { get; set; }
        public int LessonsCompleted { get; set; }
        public int ActiveDays { get; set; }
        public int TotalStudyMinutes { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public double CompetencyScore { get; set; }
    }
}
