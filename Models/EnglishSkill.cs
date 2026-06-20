using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class EnglishSkill
{
    public int Id { get; set; }

    public string SkillCode { get; set; } = null!;

    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<CompetencySkillScore> CompetencySkillScores { get; set; } = new List<CompetencySkillScore>();

    public virtual ICollection<LearningPathTemplateNode> LearningPathTemplateNodes { get; set; } = new List<LearningPathTemplateNode>();

    public virtual ICollection<LearningTopic> LearningTopics { get; set; } = new List<LearningTopic>();

    public virtual ICollection<PlacementTestSection> PlacementTestSections { get; set; } = new List<PlacementTestSection>();

    public virtual ICollection<PracticeTask> PracticeTasks { get; set; } = new List<PracticeTask>();

    public virtual ICollection<QuestionBank> QuestionBanks { get; set; } = new List<QuestionBank>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public virtual ICollection<StudentProgressSnapshot> StudentProgressSnapshots { get; set; } = new List<StudentProgressSnapshot>();
}
