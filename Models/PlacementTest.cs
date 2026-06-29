using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models;

public partial class PlacementTest
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TargetLevelId { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public decimal TotalScore { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<PlacementTestSection> PlacementTestSections { get; set; } = new List<PlacementTestSection>();

    public virtual EnglishProficiencyLevel? TargetLevel { get; set; }

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}
