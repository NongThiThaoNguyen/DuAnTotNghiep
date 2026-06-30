namespace DuAnTotNghiep.Models.ViewModels.StudyPlan
{
    public class DailyStudyTaskViewModel
    {
        public int NodeId { get; set; }
        public string Title { get; set; } = "";
        public string? SkillName { get; set; }
        public string NodeType { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusLabel { get; set; } = "";
        public string StatusCssClass { get; set; } = "";
        public int EstimatedMinutes { get; set; }
        public string? TargetUrl { get; set; }
        public string? AiReason { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsTodayTask { get; set; }
        public bool HasConfigError { get; set; }
        public DateOnly? RescheduledFrom { get; set; }
    }
}
