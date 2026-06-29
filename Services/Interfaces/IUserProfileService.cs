using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.Profile;
using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IUserProfileService
{
    // Client Side
    Task<ProfileViewModel> GetProfileAsync(int userId);
    Task UpdateProfileAsync(int userId, EditProfileViewModel model);
    Task<AccountSettingViewModel> GetAccountSettingsAsync(int userId);
    Task UpdateAccountSettingsAsync(int userId, AccountSettingViewModel model);
    Task UpdateAvatarAsync(int userId, string avatarUrl);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword);

    // Admin Side
    Task<AdminUserProfileViewModel> GetAdminUserProfileAsync(int userId);
}
