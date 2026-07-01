using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter);
        Task<UserDetailsViewModel?> GetUserDetailsAsync(int id);
        Task<bool> ChangeRoleAsync(int userId, int newRoleId, int adminId, string ipAddress);
        Task<bool> ToggleLockAsync(int userId, int adminId, string ipAddress);
        Task<bool> AdminResetPasswordAsync(int userId, string newPassword, int adminId, string ipAddress);
        Task<UserStatisticsViewModel?> GetUserStatisticsAsync(int userId);
        Task<bool> LockUserAsync(int userId, int adminId, string ipAddress);
        Task<bool> UnlockUserAsync(int userId, int adminId, string ipAddress);
        Task<bool> ResetPasswordAsync(int userId, int adminId, string ipAddress);
    }
}
