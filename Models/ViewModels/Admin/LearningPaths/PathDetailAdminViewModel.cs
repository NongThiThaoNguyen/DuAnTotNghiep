namespace DuAnTotNghiep.Models.ViewModels.Admin.LearningPaths;

/// <summary>
/// Represents admin path details with generated nodes and AI usage context.
/// </summary>
public class PathDetailAdminViewModel
{
    public int PathId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string AiPlanSummary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<PathDetailNodeAdminViewModel> Nodes { get; set; } = new();

    public string AiModel { get; set; } = string.Empty;

    public string PromptVersion { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Represents one node row in an admin learning path detail.
/// </summary>
public class PathDetailNodeAdminViewModel
{
    public int NodeId { get; set; }

    public int OrderIndex { get; set; }

    public string Title { get; set; } = string.Empty;

    public string NodeType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int? EstimatedMinutes { get; set; }

    public string AiReason { get; set; } = string.Empty;
}
