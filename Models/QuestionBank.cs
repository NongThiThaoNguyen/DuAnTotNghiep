using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class QuestionBank
{
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public int SkillId { get; set; }

    public int? LevelId { get; set; }

    public string QuestionType { get; set; } = null!;

    public string QuestionText { get; set; } = null!;

    public string? AudioUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? CorrectAnswer { get; set; }

    public string? Explanation { get; set; }

    public string DifficultyLevel { get; set; } = null!;

    public string SourceType { get; set; } = null!;

    public string ReviewStatus { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual EnglishProficiencyLevel? Level { get; set; }

    public virtual ICollection<PlacementTestQuestion> PlacementTestQuestions { get; set; } = new List<PlacementTestQuestion>();

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual EnglishSkill Skill { get; set; } = null!;

    public virtual ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();

    public virtual LearningTopic? Topic { get; set; }
}
