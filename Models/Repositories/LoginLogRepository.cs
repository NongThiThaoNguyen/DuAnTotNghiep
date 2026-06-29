using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Models.Repositories
{
    public class LoginLogRepository : GenericRepository<LoginLog>, ILoginLogRepository
    {
        public LoginLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LoginLog>> GetLogsByUserAsync(int userId)
        {
            return await _dbSet.Where(l => l.UserId == userId)
                               .OrderByDescending(l => l.CreatedAt)
                               .ToListAsync();
        }

        public async Task<IEnumerable<LoginLog>> GetRecentLogsAsync(int count)
        {
            return await _dbSet.OrderByDescending(l => l.CreatedAt)
                               .Take(count)
                               .ToListAsync();
        }
    }
}
