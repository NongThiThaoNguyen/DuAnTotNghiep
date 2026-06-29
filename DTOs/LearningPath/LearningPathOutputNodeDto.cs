namespace DuAnTotNghiep.DTOs.LearningPath;

/// <summary>
/// Represents one actionable node in an AI-generated learning path.
/// </summary>
public class LearningPathOutputNodeDto
{
    public string NodeTitle { get; set; } = string.Empty;

    public string NodeDescription { get; set; } = string.Empty;

    public string ActionType { get; set; } = string.Empty;

    public int? TopicId { get; set; }

    public int? LessonId { get; set; }

    public int? QuizId { get; set; }

    public int? PracticeTaskId { get; set; }

    public int EstimatedMinutes { get; set; }

    public string AiReason { get; set; } = string.Empty;

    public int? ScheduledDay { get; set; }

    public string PathPhase { get; set; } = string.Empty;
}
