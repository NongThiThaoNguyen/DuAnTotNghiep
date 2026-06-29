using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 chữ số")]
        public string Otp { get; set; } = null!;
    }
}
