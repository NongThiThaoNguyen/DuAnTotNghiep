using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class Quiz
{
    public int Id { get; set; }

    public int? TopicId { get; set; }

    public int SkillId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string QuizType { get; set; } = null!;

    public int? TimeLimitMinutes { get; set; }

    public decimal? PassingScore { get; set; }

    public string Status { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<LearningPathNode> LearningPathNodes { get; set; } = new List<LearningPathNode>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public virtual ICollection<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();

    public virtual EnglishSkill Skill { get; set; } = null!;

    public virtual LearningTopic? Topic { get; set; }
}
