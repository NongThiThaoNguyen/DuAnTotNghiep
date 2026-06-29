using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.ViewModels
{
    public class LoginResultDto
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}
