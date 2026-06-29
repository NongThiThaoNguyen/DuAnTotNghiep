namespace DuAnTotNghiep.Models.ViewModels.Admin.LearningPaths;

/// <summary>
/// Represents paged admin history for generated learning paths.
/// </summary>
public class PathHistoryViewModel
{
    public List<PathHistoryItemViewModel> Paths { get; set; } = new();

    public int Page { get; set; } = 1;

    public int TotalPages { get; set; } = 1;

    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Represents one row in the admin learning path history.
/// </summary>
public class PathHistoryItemViewModel
{
    public int PathId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool GeneratedByAi { get; set; }

    public DateTime CreatedAt { get; set; }
}
