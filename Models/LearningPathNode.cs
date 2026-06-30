using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class LearningPathNode
{
    public int Id { get; set; }

    public int LearningPathId { get; set; }

    public int? TopicId { get; set; }

    public int? LessonId { get; set; }

    public int? QuizId { get; set; }

    public int? PracticeTaskId { get; set; }

    public string NodeTitle { get; set; } = null!;

    public string? NodeDescription { get; set; }

    public string NodeType { get; set; } = null!;

    public string? PathPhase { get; set; }

    public DateOnly? ScheduledDate { get; set; }

    public int? EstimatedMinutes { get; set; }

    public int OrderIndex { get; set; }

    public string Status { get; set; } = null!;

    public string? AiReason { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? RequiredNodeId { get; set; }

    public DateOnly? RescheduledFrom { get; set; }

    public string? SkippedReason { get; set; }

    public virtual ICollection<AiTutorConversation> AiTutorConversations { get; set; } = new List<AiTutorConversation>();

    public virtual ICollection<LearningPathNode> InverseRequiredNode { get; set; } = new List<LearningPathNode>();

    public virtual StudentLearningPath LearningPath { get; set; } = null!;

    public virtual OriginalLesson? Lesson { get; set; }

    public virtual PracticeTask? PracticeTask { get; set; }

    public virtual Quiz? Quiz { get; set; }

    public virtual ICollection<StudyActivityLog> StudyActivityLogs { get; set; } = new List<StudyActivityLog>();

    public virtual LearningPathNode? RequiredNode { get; set; }

    public virtual LearningTopic? Topic { get; set; }
}
