using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class CompetencyAnalysis
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int? TestAttemptId { get; set; }

    public string Summary { get; set; } = null!;

    public int? CurrentLevelId { get; set; }

    public int? RecommendedLevelId { get; set; }

    public string? Strengths { get; set; }

    public string? Weaknesses { get; set; }

    public string? GapAnalysis { get; set; }

    public string? PrioritizedTopicsJson { get; set; }

    public string? KnowledgeGapsJson { get; set; }

    public string? MetadataJson { get; set; }

    public string? AiModel { get; set; }

    public decimal? ConfidenceScore { get; set; }

    public bool IsLatest { get; set; } = true;

    public string Status { get; set; } = "COMPLETED";

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CompetencySkillScore> CompetencySkillScores { get; set; } = new List<CompetencySkillScore>();

    public virtual EnglishProficiencyLevel? CurrentLevel { get; set; }

    public virtual EnglishProficiencyLevel? RecommendedLevel { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();

    public virtual TestAttempt? TestAttempt { get; set; }
}
