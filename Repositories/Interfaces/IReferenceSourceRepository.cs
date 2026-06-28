using DuAnTotNghiep.Models;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.ViewModels.Admin.ReferenceSources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IReferenceSourceRepository : IGenericRepository<ReferenceSource>
    {
        Task<IEnumerable<ReferenceSource>> GetListAsync(ReferenceSourceType? sourceType, ReferenceReviewStatus? status, string? keyword);
        Task<ReferenceSource?> GetDetailsAsync(int id);
        Task CreateAsync(ReferenceSource source);
        Task UpdateAsync(ReferenceSource source);
        Task ArchiveAsync(int id);
        Task<bool> ExistsByUrlAsync(string url, int? excludeId = null);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<IEnumerable<ReferenceSource>> GetPendingListAsync();
        Task<bool> IsUsedInTopicAsync(int id);
        Task<bool> IsUsedInLessonAsync(int id);
        Task<bool> IsUsedInAiWorkflowAsync(int id);
        
        // Task 6 Additions
        Task<(IEnumerable<ReferenceSourceListItemViewModel> Items, int TotalItems)> GetPagedListAsync(string? keyword, ReferenceSourceType? sourceType, ReferenceReviewStatus? status, int page, int pageSize);
        Task<Dictionary<ReferenceReviewStatus, int>> CountByStatusAsync();

        // Task 7 Additions
        Task<List<OriginalLesson>> GetLinkedLessonsAsync(int referenceSourceId);
        Task<List<AuditLog>> GetRecentAuditLogsAsync(int referenceSourceId, int limit);

        // Task 10 Additions
        Task AddComplianceReviewAsync(ContentComplianceReview review);
    }
}
