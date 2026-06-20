using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class PracticeTask
{
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public int SkillId { get; set; }

    public string Title { get; set; } = null!;

    public string Instruction { get; set; } = null!;

    public string TaskType { get; set; } = null!;

    public string DifficultyLevel { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<LearningPathNode> LearningPathNodes { get; set; } = new List<LearningPathNode>();

    public virtual ICollection<PracticeSubmission> PracticeSubmissions { get; set; } = new List<PracticeSubmission>();

    public virtual EnglishSkill Skill { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
