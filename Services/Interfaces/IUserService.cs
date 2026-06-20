using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter);
        Task<UserDetailsViewModel?> GetUserDetailsAsync(int id);
        Task<bool> LockUserAsync(int userId, int adminId, string ipAddress);
        Task<bool> UnlockUserAsync(int userId, int adminId, string ipAddress);
        Task<bool> ResetPasswordAsync(int userId, int adminId, string ipAddress);
    }
}
