using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class OriginalLesson
{
    public int Id { get; set; }

    public int TopicId { get; set; }

    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string ContentType { get; set; } = null!;

    public int? EstimatedMinutes { get; set; }

    public string? VideoUrl { get; set; }

    public string SourceType { get; set; } = null!;

    public string ReviewStatus { get; set; } = null!;

    public bool IsAiGenerated { get; set; }

    public int? CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<LearningPathNode> LearningPathNodes { get; set; } = new List<LearningPathNode>();

    public virtual LearningTopic Topic { get; set; } = null!;
}
