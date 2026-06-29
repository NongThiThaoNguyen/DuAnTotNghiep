namespace DuAnTotNghiep.Models.ViewModels.LearningPath.M8;

/// <summary>
/// Represents the admin-facing history of generated learning paths.
/// </summary>
public class AdminPathHistoryViewModel
{
    public List<AdminPathHistoryItemViewModel> Paths { get; set; } = new();
}
