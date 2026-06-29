using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class CompetencySkillScore
{
    public int Id { get; set; }

    public int CompetencyAnalysisId { get; set; }

    public int SkillId { get; set; }

    public decimal Score { get; set; }

    public int? LevelId { get; set; }

    public string? WeaknessNote { get; set; }

    public int PriorityLevel { get; set; }

    public int? TopicId { get; set; }

    public virtual CompetencyAnalysis CompetencyAnalysis { get; set; } = null!;

    public virtual EnglishProficiencyLevel? Level { get; set; }

    public virtual EnglishSkill Skill { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
