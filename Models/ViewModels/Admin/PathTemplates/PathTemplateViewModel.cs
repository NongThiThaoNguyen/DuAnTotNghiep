namespace DuAnTotNghiep.Models.ViewModels.Admin.PathTemplates;

/// <summary>
/// Represents a reusable learning path template in admin screens.
/// </summary>
public class PathTemplateViewModel
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public int? GoalId { get; set; }

    public int? StartLevelId { get; set; }

    public int? TargetLevelId { get; set; }

    public int? DurationWeeks { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public List<PathTemplateNodeViewModel> Nodes { get; set; } = new();
}

/// <summary>
/// Represents one node inside an admin learning path template form.
/// </summary>
public class PathTemplateNodeViewModel
{
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public int? SkillId { get; set; }

    public string NodeTitle { get; set; } = string.Empty;

    public string NodeType { get; set; } = string.Empty;

    public int? EstimatedMinutes { get; set; }

    public int OrderIndex { get; set; }

    public string UnlockCondition { get; set; } = string.Empty;
}
