using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Models.Repositories
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

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(string keyword, string role, string status, int page, int pageSize)
        {
            var query = _dbSet.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role.RoleCode == role);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(u => u.Status == status);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<User?> GetUserWithRoleByIdAsync(int id)
        {
            return await _dbSet.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
