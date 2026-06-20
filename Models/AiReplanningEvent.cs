using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiReplanningEvent
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int LearningPathId { get; set; }

    public string TriggerType { get; set; } = null!;

    public string? OldPlanSummary { get; set; }

    public string? NewPlanSummary { get; set; }

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual StudentLearningPath LearningPath { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
