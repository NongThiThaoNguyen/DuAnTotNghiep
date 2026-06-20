using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersAsync();
        Task LockUserAsync(int userId, DateTime lockoutEnd);
        Task UnlockUserAsync(int userId);
    }
}
