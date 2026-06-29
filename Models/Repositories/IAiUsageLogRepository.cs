using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories
{
    public interface IAiUsageLogRepository
    {
        Task<AiUsageLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AiUsageLog>> ListByTemplateIdAsync(int promptTemplateId, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(AiUsageLog usageLog, CancellationToken cancellationToken = default);
    }
}
