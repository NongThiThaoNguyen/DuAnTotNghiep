using DuAnTotNghiep.Models.ViewModels.LearningPath;

namespace DuAnTotNghiep.Models.ViewModels.LearningPath.M8;

/// <summary>
/// Represents detailed path information including all student path nodes.
/// </summary>
public class LearningPathDetailViewModel : LearningPathSummaryViewModel
{
    public List<PathNodeViewModel> Nodes { get; set; } = new();
}
