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
    public string EvaluationScore { get; set; } = "Chưa có";
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

public class StudentAttendanceListViewModel : AdminPagedViewModel
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

public class StudentQuizListViewModel : AdminPagedViewModel
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

public class StudentAssignmentListViewModel : AdminPagedViewModel
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
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int AssignedTopicCount { get; set; }
    public int ResourceCount { get; set; }
    public int QuizCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TeacherManagementIndexViewModel
{
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public List<TeacherListItemViewModel> Items { get; set; } = new();
}

public class CreateTeacherAdminViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Bio { get; set; }
}

public class EditTeacherAdminViewModel
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string? Gender { get; set; }
    public string? Country { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Bio { get; set; }
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
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int AssignedTopicCount { get; set; }
    public int QuizCount { get; set; }
    public int ResourceCount { get; set; }
    public int StudentCount { get; set; }
    public int AssignmentCount { get; set; }
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

public class TeacherResourceAdminIndexViewModel : AdminPagedViewModel
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

public class QuizManagementIndexViewModel : AdminPagedViewModel
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
    public string TaskType { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = "Không rõ";
    public string Status { get; set; } = string.Empty;
    public int SubmissionCount { get; set; }
    public int TotalStudentCount { get; set; }
    public string SubmissionDisplayRatio => $"{SubmissionCount} bài nộp / {TotalStudentCount} học viên";
    public DateTime CreatedAt { get; set; }
}

public class AssignmentManagementIndexViewModel : AdminPagedViewModel
{
    public string? Keyword { get; set; }
    public int? TopicId { get; set; }
    public int? TeacherId { get; set; }
    public string? Status { get; set; }
    public List<AdminOptionViewModel> Topics { get; set; } = new();
    public List<AdminOptionViewModel> Teachers { get; set; } = new();
    public List<AssignmentManagementRowViewModel> Items { get; set; } = new();
}

public class EditAssignmentAdminViewModel
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "MEDIUM";
    public int? TopicId { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public List<AdminOptionViewModel> Topics { get; set; } = new();
}

public class AssignmentDetailsAdminViewModel
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Instruction { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public string TopicTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = "Không rõ";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SubmissionCount { get; set; }
    public int TotalStudentCount { get; set; }
    public string SubmissionDisplayRatio => $"{SubmissionCount} bài nộp / {TotalStudentCount} học viên";
    public List<StudentAssignmentRowViewModel> Submissions { get; set; } = new();
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

    // Time filter
    public string? Period { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Chart data: quiz attempts per day
    public List<ChartDataPoint> QuizAttemptsOverTime { get; set; } = new();
    // Chart data: attendance per day
    public List<ChartDataPoint> AttendanceOverTime { get; set; } = new();
    // Chart data: new students per day
    public List<ChartDataPoint> NewStudentsOverTime { get; set; } = new();

    // Course completion
    public int TotalLearningPaths { get; set; }
    public int CompletedLearningPaths { get; set; }
    public decimal CourseCompletionRate { get; set; }

    // Top 5 students by average quiz score
    public List<TopStudentRowViewModel> TopStudents { get; set; } = new();

    // Teacher performance summary
    public List<TeacherPerformanceSummaryRow> TeacherPerformances { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class TopStudentRowViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public decimal AverageScore { get; set; }
}

public class TeacherPerformanceSummaryRow
{
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int TopicCount { get; set; }
    public int QuizCount { get; set; }
    public int AssignmentCount { get; set; }
    public int GradedSubmissionCount { get; set; }
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
    public string LastMessageText { get; set; } = string.Empty;
    public string ClassNames { get; set; } = "Chưa xác định";
    public int UnreadCount { get; set; }
    public string Status => UnreadCount > 0 ? "Có tin chưa đọc" : "Đã đọc";
}

public class AdminChatMessageViewModel
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
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
