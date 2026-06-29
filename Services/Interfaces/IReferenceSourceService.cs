using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.ViewModels.Admin.ReferenceSources;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public class LicenseSummaryResult
    {
        public string LicenseNote { get; set; } = string.Empty;
        public string UsagePolicy { get; set; } = string.Empty;
        public string WarningLevel { get; set; } = "NONE"; // "NONE" | "WARNING" | "BLOCK_INFO"
    }

    public interface IReferenceSourceService
    {
        Task<IEnumerable<ReferenceSource>> GetListAsync(ReferenceSourceType? sourceType, ReferenceReviewStatus? status, string? keyword);
        Task<ReferenceSource?> GetDetailsAsync(int id);
        Task<int> CreateAsync(ReferenceSource source, int userId);
        Task ApproveAsync(int id, int adminId, string userRole, string? note = null);
        Task RejectAsync(int id, string reason, int adminId, string userRole);
        Task ArchiveAsync(int id, int userId, string userRole);
        
        // Task 6 Additions
        Task<ReferenceSourcePagedViewModel> GetPagedListAsync(string? keyword, ReferenceSourceType? sourceType, ReferenceReviewStatus? status, int page, int pageSize);

        // Task 7 Additions
        Task<ReferenceSourceDetailsViewModel?> GetDetailsViewModelAsync(int id, int currentUserId, string userRole);

        // Task 8 Additions
        Task<EditReferenceSourceViewModel?> GetEditModelAsync(int id, int currentUserId, string userRole);
        Task UpdateAsync(EditReferenceSourceViewModel model, int currentUserId, string userRole);

        // Task 9 Additions
        Task SubmitForReviewAsync(int id, int currentUserId, string userRole);

        // Task 11 Additions
        Task LinkSourceToTopicAsync(int topicId, int sourceId, int userId, string? note = null);
        Task UnlinkSourceFromTopicAsync(int topicId, int sourceId, int userId);

        // Task 13 Additions
        Task<LicenseSummaryResult> GetLicenseSummaryAsync(int sourceId);
    }
}
