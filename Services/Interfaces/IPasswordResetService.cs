namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task<bool> SendOtpAsync(string email);
        Task<bool> VerifyOtpAsync(string email, string otp);
        Task<bool> ResetPasswordAsync(string email, string otp, string newPassword, string ipAddress);
    }
}
