using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface ILoginLogRepository : IGenericRepository<LoginLog>
    {
        Task<IEnumerable<LoginLog>> GetLogsByUserAsync(int userId);
        Task<IEnumerable<LoginLog>> GetRecentLogsAsync(int count);
    }
}
