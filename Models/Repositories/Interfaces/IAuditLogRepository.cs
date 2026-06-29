using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetAuditsByUserAsync(int userId);
        Task<IEnumerable<AuditLog>> GetRecentAuditsAsync(int count);
    }
}
