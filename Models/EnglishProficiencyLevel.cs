using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class EnglishProficiencyLevel
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int OrderIndex { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<CompetencyAnalysis> CompetencyAnalysisCurrentLevels { get; set; } = new List<CompetencyAnalysis>();

    public virtual ICollection<CompetencyAnalysis> CompetencyAnalysisRecommendedLevels { get; set; } = new List<CompetencyAnalysis>();

    public virtual ICollection<CompetencySkillScore> CompetencySkillScores { get; set; } = new List<CompetencySkillScore>();

    public virtual ICollection<LearningPathTemplate> LearningPathTemplateStartLevels { get; set; } = new List<LearningPathTemplate>();

    public virtual ICollection<LearningPathTemplate> LearningPathTemplateTargetLevels { get; set; } = new List<LearningPathTemplate>();

    public virtual ICollection<LearningTopic> LearningTopics { get; set; } = new List<LearningTopic>();

    public virtual ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();

    public virtual ICollection<QuestionBank> QuestionBanks { get; set; } = new List<QuestionBank>();

    public virtual ICollection<StudentLearningProfile> StudentLearningProfileCurrentLevels { get; set; } = new List<StudentLearningProfile>();

    public virtual ICollection<StudentLearningProfile> StudentLearningProfileTargetLevels { get; set; } = new List<StudentLearningProfile>();

    public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
}
