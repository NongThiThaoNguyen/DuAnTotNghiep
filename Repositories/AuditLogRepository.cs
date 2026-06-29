using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Repositories
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AuditLog>> GetAuditsByUserAsync(int userId)
        {
            return await _dbSet.Where(a => a.UserId == userId)
                               .OrderByDescending(a => a.CreatedAt)
                               .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAuditsAsync(int count)
        {
            return await _dbSet.OrderByDescending(a => a.CreatedAt)
                               .Take(count)
                               .ToListAsync();
        }
    }
}
