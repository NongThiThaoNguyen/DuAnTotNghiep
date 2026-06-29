using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces;

/// <summary>
/// Validates learning path nodes against content status and source compliance rules.
/// </summary>
public interface ILearningPathComplianceService
{
    Task<(bool IsCompliant, List<string> Violations)> ValidateContentComplianceAsync(
        List<LearningPathNode> nodes);
}
