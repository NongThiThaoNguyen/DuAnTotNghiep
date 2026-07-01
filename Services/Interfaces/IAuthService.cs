using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterViewModel request);
        Task<LoginResultDto> LoginAsync(string email, string password, string ipAddress, string userAgent);
        Task<(bool IsSuccess, string ErrorMessage)> ChangePasswordAsync(int userId, string oldPassword, string newPassword, string ipAddress);
    }
}
