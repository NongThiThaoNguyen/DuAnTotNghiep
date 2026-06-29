using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models;

public partial class AiFeedback
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int? QuizAttemptId { get; set; }

    public int? PracticeSubmissionId { get; set; }

    public string FeedbackType { get; set; } = null!;

    public string FeedbackText { get; set; } = null!;

    public string? MistakeAnalysis { get; set; }

    public string? RecommendedAction { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual PracticeSubmission? PracticeSubmission { get; set; }

    public virtual QuizAttempt? QuizAttempt { get; set; }

    public virtual User Student { get; set; } = null!;
}
