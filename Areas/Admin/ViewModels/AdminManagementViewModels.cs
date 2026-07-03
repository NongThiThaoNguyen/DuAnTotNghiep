namespace DuAnTotNghiep.Areas.Admin.ViewModels;

public class AdminOptionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class AdminPagedViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalItems { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
}

public class StudentListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LevelName { get; set; } = "Chưa có";
    public string PathStatus { get; set; } = "Chưa có";
    public decimal AttendanceRate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentManagementListViewModel : AdminPagedViewModel
{
    public string? Keyword { get; set; }
    public string? PathStatus { get; set; }
    public int? LevelId { get; set; }
    public decimal? MinAttendanceRate { get; set; }
    public List<StudentListItemViewModel> Items { get; set; } = new();
    public List<AdminOptionViewModel> Levels { get; set; } = new();
}

public class StudentAttendanceRowViewModel
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class StudentAttendanceListViewModel
{
    public int? StudentId { get; set; }
    public int? TopicId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public List<StudentAttendanceRowViewModel> Items { get; set; } = new();
    public List<AdminOptionViewModel> Students { get; set; } = new();
    public List<AdminOptionViewModel> Topics { get; set; } = new();
}

public class StudentQuizAttemptRowViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

public class StudentQuizListViewModel
{
    public int? StudentId { get; set; }
    public List<AdminOptionViewModel> Students { get; set; } = new();
    public List<StudentQuizAttemptRowViewModel> Items { get; set; } = new();
}

public class StudentQuizDetailsViewModel
{
    public int AttemptId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<QuizAnswerRowViewModel> Answers { get; set; } = new();
}

public class QuizAnswerRowViewModel
{
    public string QuestionText { get; set; } = string.Empty;
    public string? AnswerText { get; set; }
    public string? SelectedOptionText { get; set; }
    public bool? IsCorrect { get; set; }
    public decimal? Score { get; set; }
    public string? AiExplanation { get; set; }
}

public class StudentAssignmentRowViewModel
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class StudentAssignmentListViewModel
{
    public int? StudentId { get; set; }
    public List<AdminOptionViewModel> Students { get; set; } = new();
    public List<StudentAssignmentRowViewModel> Items { get; set; } = new();
}

public class StudentAssignmentDetailsViewModel
{
    public int SubmissionId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public string TaskInstruction { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? SubmissionText { get; set; }
    public string? FileUrl { get; set; }
    public string? AudioUrl { get; set; }
    public string? AiFeedback { get; set; }
    public string? TeacherFeedback { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class TeacherListItemViewModel
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AssignedTopicCount { get; set; }
    public int ResourceCount { get; set; }
    public int QuizCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TeacherManagementIndexViewModel
{
    public List<TeacherListItemViewModel> Items { get; set; } = new();
}

public class TeacherProfileAdminViewModel
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public List<string> AssignedTopics { get; set; } = new();
}

public class TeacherPerformanceAdminViewModel
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int AssignedTopicCount { get; set; }
    public int QuizCount { get; set; }
    public int AssignmentCount { get; set; }
    public int GradedSubmissionCount { get; set; }
    public decimal AverageSubmissionScore { get; set; }
    public int CompletedLearningPaths { get; set; }
}

public class TeacherAssignmentRowViewModel
{
    public int AssignmentId { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string TopicTitle { get; set; } = "Chưa gắn topic";
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Classroom { get; set; }
}

public class TeacherAssignmentIndexViewModel
{
    public List<TeacherAssignmentRowViewModel> Items { get; set; } = new();
}

public class TeacherAssignmentCreateViewModel
{
    public int TeacherId { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.Today.AddHours(8);
    public DateTime EndTime { get; set; } = DateTime.Today.AddHours(9);
    public string? Classroom { get; set; }
    public List<AdminOptionViewModel> Teachers { get; set; } = new();
    public List<AdminOptionViewModel> Topics { get; set; } = new();
}

public class TeacherResourceRowViewModel
{
    public int ResourceId { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = "Không rõ";
    public DateTime CreatedAt { get; set; }
}

public class TeacherResourceAdminIndexViewModel
{
    public int? TeacherId { get; set; }
    public List<AdminOptionViewModel> Teachers { get; set; } = new();
    public List<TeacherResourceRowViewModel> Items { get; set; } = new();
}

public class QuizManagementRowViewModel
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = "Không rõ";
    public string Status { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int AttemptCount { get; set; }
    public decimal AverageScore { get; set; }
}

public class QuizManagementIndexViewModel
{
    public int? TopicId { get; set; }
    public int? TeacherId { get; set; }
    public List<AdminOptionViewModel> Topics { get; set; } = new();
    public List<AdminOptionViewModel> Teachers { get; set; } = new();
    public List<QuizManagementRowViewModel> Items { get; set; } = new();
}

public class QuizManagementDetailsViewModel
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = "Không rõ";
    public string Status { get; set; } = string.Empty;
    public int? TimeLimitMinutes { get; set; }
    public decimal? PassingScore { get; set; }
    public List<QuizQuestionAdminRowViewModel> Questions { get; set; } = new();
}

public class QuizQuestionAdminRowViewModel
{
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal Points { get; set; }
    public string? CorrectAnswer { get; set; }
}

public class QuizAttemptsAdminViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public List<StudentQuizAttemptRowViewModel> Attempts { get; set; } = new();
}

public class AssignmentManagementRowViewModel
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = "Không rõ";
    public string Status { get; set; } = string.Empty;
    public int SubmissionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AssignmentManagementIndexViewModel
{
    public int? TopicId { get; set; }
    public List<AdminOptionViewModel> Topics { get; set; } = new();
    public List<AssignmentManagementRowViewModel> Items { get; set; } = new();
}

public class AssignmentSubmissionsAdminViewModel
{
    public int AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public List<StudentAssignmentRowViewModel> Submissions { get; set; } = new();
}

public class StudentProgressReportRowViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LevelName { get; set; } = "Chưa có";
    public int LearningPathCount { get; set; }
    public int CompletedPathCount { get; set; }
    public decimal AverageProgress { get; set; }
}

public class StudentProgressReportViewModel
{
    public List<StudentProgressReportRowViewModel> Items { get; set; } = new();
}

public class TeacherActivityReportRowViewModel
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int TopicCount { get; set; }
    public int QuizCount { get; set; }
    public int AssignmentCount { get; set; }
    public int ResourceCount { get; set; }
}

public class TeacherActivityReportViewModel
{
    public List<TeacherActivityReportRowViewModel> Items { get; set; } = new();
}

public class AttendanceSummaryRowViewModel
{
    public string TopicTitle { get; set; } = string.Empty;
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public decimal AttendanceRate { get; set; }
}

public class AttendanceSummaryReportViewModel
{
    public List<AttendanceSummaryRowViewModel> Items { get; set; } = new();
}

public class QuizPerformanceReportRowViewModel
{
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public decimal AverageScore { get; set; }
    public decimal PassRate { get; set; }
}

public class QuizPerformanceReportViewModel
{
    public List<QuizPerformanceReportRowViewModel> Items { get; set; } = new();
}

public class AdminReportsIndexViewModel
{
    public int StudentCount { get; set; }
    public int TeacherCount { get; set; }
    public int AttendanceCount { get; set; }
    public int QuizAttemptCount { get; set; }
}

public class ChatStatsViewModel
{
    public int TeacherStudentConversationCount { get; set; }
    public int TeacherStudentMessageCount { get; set; }
    public int AiTutorSessionCount { get; set; }
    public int AiTutorMessageCount { get; set; }
    public List<TeacherStudentChatRowViewModel> RecentTeacherChats { get; set; } = new();
    public List<AiTutorSessionRowViewModel> RecentAiSessions { get; set; } = new();
}

public class TeacherStudentChatRowViewModel
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTime LastMessageAt { get; set; }
}

public class AiTutorSessionRowViewModel
{
    public int ConversationId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = "Không gắn topic";
    public string Status { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}
