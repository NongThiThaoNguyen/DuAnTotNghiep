using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
