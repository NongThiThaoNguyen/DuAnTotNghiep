using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class StudyActivityLog
{
    public long Id { get; set; }

    public int StudentId { get; set; }

    public string ActivityType { get; set; } = null!;

    public int? TopicId { get; set; }

    public int? LearningPathNodeId { get; set; }

    public int? DurationMinutes { get; set; }

    public decimal? Score { get; set; }

    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LearningPathNode? LearningPathNode { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
