using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class LearningPathTemplateNode
{
    public int Id { get; set; }

    public int TemplateId { get; set; }

    public int? TopicId { get; set; }

    public int? SkillId { get; set; }

    public string NodeTitle { get; set; } = null!;

    public string NodeType { get; set; } = null!;

    public int? EstimatedMinutes { get; set; }

    public int OrderIndex { get; set; }

    public string? UnlockCondition { get; set; }

    public virtual EnglishSkill? Skill { get; set; }

    public virtual LearningPathTemplate Template { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
