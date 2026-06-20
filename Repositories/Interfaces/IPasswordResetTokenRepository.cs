using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository : IGenericRepository<PasswordResetToken>
    {
        Task<PasswordResetToken?> GetValidTokenAsync(int userId, string tokenHash);
        Task InvalidateOldTokensAsync(int userId);
    }
}
