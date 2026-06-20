using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class LearningPathTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = null!;

    public int? GoalId { get; set; }

    public int? StartLevelId { get; set; }

    public int? TargetLevelId { get; set; }

    public int? DurationWeeks { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual LearningGoal? Goal { get; set; }

    public virtual ICollection<LearningPathTemplateNode> LearningPathTemplateNodes { get; set; } = new List<LearningPathTemplateNode>();

    public virtual EnglishProficiencyLevel? StartLevel { get; set; }

    public virtual ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();

    public virtual EnglishProficiencyLevel? TargetLevel { get; set; }
}
