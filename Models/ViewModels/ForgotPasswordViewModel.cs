using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;
    }
}
