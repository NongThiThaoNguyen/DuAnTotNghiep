<<<<<<< HEAD
﻿using System;
=======
using System;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
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

<<<<<<< HEAD
=======
    public string? PrioritizedTopicsJson { get; set; }

    public string? KnowledgeGapsJson { get; set; }

    public string? MetadataJson { get; set; }

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public string? AiModel { get; set; }

    public decimal? ConfidenceScore { get; set; }

<<<<<<< HEAD
=======
    public bool IsLatest { get; set; } = true;

    public string Status { get; set; } = "COMPLETED";

>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CompetencySkillScore> CompetencySkillScores { get; set; } = new List<CompetencySkillScore>();

    public virtual EnglishProficiencyLevel? CurrentLevel { get; set; }

    public virtual EnglishProficiencyLevel? RecommendedLevel { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<StudentLearningPath> StudentLearningPaths { get; set; } = new List<StudentLearningPath>();

    public virtual TestAttempt? TestAttempt { get; set; }
}
