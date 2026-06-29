using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.LearningPath.M8;

namespace DuAnTotNghiep.Services.Interfaces;

/// <summary>
/// Coordinates AI learning path input building, generation, querying, and lifecycle actions.
/// </summary>
public interface ILearningPathEngineService
{
    Task<LearningPathInputDto> BuildInputAsync(int studentId);

    Task<StudentLearningPath> GenerateInitialPathAsync(int studentId, int competencyAnalysisId);

    Task<StudentLearningPath?> GetActivePathAsync(int studentId);

    Task<LearningPathDetailViewModel> GetPathDetailAsync(int pathId, int userId);

    Task<LearningPathSummaryViewModel> GetPathSummaryAsync(int studentId);

    Task ArchivePathAsync(int pathId, int userId);

    Task<StudentLearningPath> RegeneratePathAsync(int studentId, string reason);

    Task<LearningPathGenerateViewModel> CanGeneratePathAsync(int studentId);

    Task<bool> UnlockNextNodeAsync(int completedNodeId, int studentId);
}
