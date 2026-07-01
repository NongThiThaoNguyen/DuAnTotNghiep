namespace DuAnTotNghiep.Models.ViewModels.Teacher;

public class PendingSubmissionViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}

public class PracticeSubmissionDetailViewModel
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string? TaskDescription { get; set; }
    public string? StudentAnswer { get; set; }
    public string? FileUrl { get; set; }
    public string? AudioUrl { get; set; }
    public decimal? Score { get; set; }
    public string? TeacherFeedback { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class GradeOverviewViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public decimal QuizScore { get; set; }
    public decimal PracticeScore { get; set; }
    public decimal TotalScore { get; set; }
}
