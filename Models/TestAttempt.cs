using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models;

public partial class TestAttempt
{
    public int Id { get; set; }

    public int PlacementTestId { get; set; }

    public int StudentId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public decimal? TotalScore { get; set; }

    public int? EstimatedLevelId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!;

    public virtual ICollection<CompetencyAnalysis> CompetencyAnalyses { get; set; } = new List<CompetencyAnalysis>();

    public virtual EnglishProficiencyLevel? EstimatedLevel { get; set; }

    public virtual PlacementTest PlacementTest { get; set; } = null!;

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
}
