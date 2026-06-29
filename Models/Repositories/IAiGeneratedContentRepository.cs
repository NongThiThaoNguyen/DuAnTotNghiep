using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories
{
    public interface IAiGeneratedContentRepository
    {
        Task<AiGeneratedContent?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AiGeneratedContent>> ListAsync(int? relatedTopicId = null, string? contentType = null, string? reviewStatus = null, CancellationToken cancellationToken = default);
        Task<int> CreateAsync(AiGeneratedContent entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(AiGeneratedContent entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// Soft-delete the AI content. Implementations MUST NOT hard-delete reviewed content.
        /// </summary>
        Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    }
}
