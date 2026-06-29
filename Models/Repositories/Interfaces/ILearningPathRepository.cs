using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.Repositories.Interfaces;

/// <summary>
/// Provides data access operations for student learning paths and their nodes.
/// </summary>
public interface ILearningPathRepository
{
    Task<StudentLearningPath?> GetActivePathByStudentIdAsync(int studentId);

    Task<StudentLearningPath?> GetPathWithNodesAsync(int pathId);

    Task<List<StudentLearningPath>> GetPathHistoryByStudentIdAsync(int studentId);

    Task AddPathAsync(StudentLearningPath path);

    Task UpdatePathAsync(StudentLearningPath path);

    Task AddNodesAsync(IEnumerable<LearningPathNode> nodes);

    Task<(IEnumerable<StudentLearningPath> Paths, int TotalCount)> GetAllPathsPagedAsync(
        int page,
        int pageSize,
        string? statusFilter);
}
