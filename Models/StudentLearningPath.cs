using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class StudentLearningPath
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int? TemplateId { get; set; }

    public int? CompetencyAnalysisId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? GoalId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? TargetEndDate { get; set; }

    public string? AiPlanSummary { get; set; }

    public string Status { get; set; } = null!;

    public bool GeneratedByAi { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int PathVersion { get; set; } = 1;

    public DateTime? ArchivedAt { get; set; }

    public int? ReplacedByPathId { get; set; }

    public virtual ICollection<AiReplanningEvent> AiReplanningEvents { get; set; } = new List<AiReplanningEvent>();

    public virtual CompetencyAnalysis? CompetencyAnalysis { get; set; }

    public virtual LearningGoal? Goal { get; set; }

    public virtual ICollection<StudentLearningPath> InverseReplacedByPath { get; set; } = new List<StudentLearningPath>();

    public virtual ICollection<LearningPathNode> LearningPathNodes { get; set; } = new List<LearningPathNode>();

    public virtual StudentLearningPath? ReplacedByPath { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual LearningPathTemplate? Template { get; set; }
}
