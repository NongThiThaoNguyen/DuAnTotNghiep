using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Repositories
{
    public interface IAiPromptTemplateRepository
    {
        Task<AiPromptTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AiPromptTemplate>> ListAsync(CancellationToken cancellationToken = default);
        Task<int> CreateAsync(AiPromptTemplate template, CancellationToken cancellationToken = default);
        Task UpdateAsync(AiPromptTemplate template, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}
