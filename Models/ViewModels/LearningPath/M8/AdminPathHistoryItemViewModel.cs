namespace DuAnTotNghiep.Models.ViewModels.LearningPath.M8;

/// <summary>
/// Represents one generated learning path row in admin history.
/// </summary>
public class AdminPathHistoryItemViewModel
{
    public int PathId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool GeneratedByAi { get; set; }

    public string AiModel { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;
}
