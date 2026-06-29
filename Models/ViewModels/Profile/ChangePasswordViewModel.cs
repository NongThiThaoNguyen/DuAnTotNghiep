using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels.Profile;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu hiện tại")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} ký tự.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string NewPassword { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = null!;
}
