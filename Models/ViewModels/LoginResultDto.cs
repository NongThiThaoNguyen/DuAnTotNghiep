using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class LoginResultDto
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}
