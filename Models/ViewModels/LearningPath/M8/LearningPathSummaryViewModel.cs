namespace DuAnTotNghiep.Models.ViewModels.LearningPath.M8;

/// <summary>
/// Represents a compact summary of a student's AI-generated learning path.
/// </summary>
public class LearningPathSummaryViewModel
{
    public int PathId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string AiPlanSummary { get; set; } = string.Empty;

    public int TotalNodes { get; set; }

    public int CompletedNodes { get; set; }

    public string CurrentNodeTitle { get; set; } = string.Empty;

    public string NextNodeTitle { get; set; } = string.Empty;

    public DateOnly? StartDate { get; set; }

    public DateOnly? TargetEndDate { get; set; }

    public List<string> PriorityTopics { get; set; } = new();

    public bool GeneratedByAi { get; set; }

    public int PathVersion { get; set; } = 1;
}
