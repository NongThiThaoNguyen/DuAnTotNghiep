using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter);
        Task<UserDetailsViewModel?> GetUserDetailsAsync(int id);
    }
}
