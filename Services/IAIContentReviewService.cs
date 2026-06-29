using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.DTOs;
using DuAnTotNghiep.Models.ViewModels;

namespace DuAnTotNghiep.Services
{
    public interface IAIContentReviewService
    {
        Task<PagedResult<AiContentListItemViewModel>> GetPendingContentAsync(
            string? skillFilter = null,
            int? topicIdFilter = null,
            string? contentTypeFilter = null,
            int? requestedByFilter = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<AiContentReviewDetailViewModel?> GetContentDetailAsync(int id, CancellationToken cancellationToken = default);

        Task ApproveAsync(int id, int reviewerId, bool copyrightCheck, string? plagiarismRisk, string? reviewNote, string? editedQuestionText, string? editedExplanation, CancellationToken cancellationToken = default);

        Task RejectAsync(int id, int reviewerId, string? reviewNote, CancellationToken cancellationToken = default);

        Task RequestRevisionAsync(int id, int reviewerId, string? reviewNote, CancellationToken cancellationToken = default);

        Task<Dictionary<string, int>> GetPendingCountByTypeAsync(CancellationToken cancellationToken = default);
    }
}
