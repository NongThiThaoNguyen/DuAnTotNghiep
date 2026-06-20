using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.Include(u => u.Role)
                               .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            return await _dbSet.Include(u => u.Role).ToListAsync();
        }

        public async Task LockUserAsync(int userId, DateTime lockoutEnd)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.LockoutUntil = lockoutEnd;
                user.Status = "LOCKED";
                _dbSet.Update(user);
            }
        }

        public async Task UnlockUserAsync(int userId)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.LockoutUntil = null;
                user.FailedLoginCount = 0;
                user.Status = "ACTIVE";
                _dbSet.Update(user);
            }
        }
    }
}
