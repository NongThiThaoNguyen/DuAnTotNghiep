using DuAnTotNghiep.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterViewModel request);
        Task<LoginResultDto> LoginAsync(string email, string password, string ipAddress, string userAgent);
        Task LogoutAsync(int userId, string sessionToken);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }
}
