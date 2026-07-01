namespace DuAnTotNghiep.Models.ViewModels.Teacher;

public class TeacherDashboardViewModel
{
    public string TeacherName { get; set; } = "Giao vien";
    public string AvatarUrl { get; set; } = string.Empty;
    public int CoursesCount { get; set; }
    public int StudentsCount { get; set; }
    public int PendingSubmissionsCount { get; set; }
    public int TodaySchedulesCount { get; set; }
    public List<RecentSubmissionViewModel> RecentSubmissions { get; set; } = new();
    public List<TodayScheduleViewModel> TodaySchedules { get; set; } = new();
    public List<RecentQuizAttemptViewModel> RecentQuizAttempts { get; set; } = new();
    public List<string> ChartLabels { get; set; } = new();
    public List<int> ChartValues { get; set; } = new();
}

public class RecentSubmissionViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TaskTitle { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TodayScheduleViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Classroom { get; set; }
}

public class RecentQuizAttemptViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
