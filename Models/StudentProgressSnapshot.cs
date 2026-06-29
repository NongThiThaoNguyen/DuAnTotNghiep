using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class StudentProgressSnapshot
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int? SkillId { get; set; }

    public int? TopicId { get; set; }

    public decimal ProgressPercent { get; set; }

    public decimal? AverageScore { get; set; }

    public int TotalStudyMinutes { get; set; }

    public int CompletedNodes { get; set; }

    public string? WeakPoints { get; set; }

    public DateOnly SnapshotDate { get; set; }

    public virtual EnglishSkill? Skill { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
