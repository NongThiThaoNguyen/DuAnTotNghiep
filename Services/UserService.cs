using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserListViewModel> GetPagedUsersAsync(UserFilterViewModel filter)
        {
            var (users, totalCount) = await _userRepository.GetPagedUsersAsync(
                filter.Keyword, 
                filter.Role, 
                filter.Status, 
                filter.Page, 
                filter.PageSize);

            var userViewModels = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role?.RoleName ?? "Unknown",
                Status = u.Status,
                CreatedAt = u.CreatedAt
            }).ToList();

            return new UserListViewModel
            {
                Users = userViewModels,
                TotalItems = totalCount,
                Filter = filter
            };
        }

        public async Task<UserDetailsViewModel?> GetUserDetailsAsync(int id)
        {
            var user = await _userRepository.GetUserWithRoleByIdAsync(id);
            if (user == null) return null;

            return new UserDetailsViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role?.RoleName ?? "Unknown",
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
