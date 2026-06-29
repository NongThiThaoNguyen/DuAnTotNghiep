using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.LearningPath;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPathViewService
    {
        Task<LearningPathPageViewModel> GetCurrentPathPageAsync(int userId);
        Task<bool> EnsurePathOwnerAsync(int pathId, int userId);
        Task<string?> BuildNodeTargetUrlAsync(LearningPathNode node);
        Task<bool> CanOpenNodeAsync(int nodeId, int userId);
        Task<bool> TryUnlockNextNodesAsync(int completedNodeId, int userId);
        Task<bool> MarkNodeCompletedAsync(
            int nodeId,
            int userId,
            string? activityType = null,
            int? durationMinutes = null,
            decimal? score = null,
            string? metadata = null);
    }
}
