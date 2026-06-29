using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersAsync();
        Task LockUserAsync(int userId, DateTime lockoutEnd);
        Task UnlockUserAsync(int userId);
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(string keyword, string role, string status, int page, int pageSize);
        Task<User?> GetUserWithRoleByIdAsync(int id);
    }
}
