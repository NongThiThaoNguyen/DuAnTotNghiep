namespace DuAnTotNghiep.ViewModels.LearningPath
{
    public class PathNodeViewModel
    {
        public int NodeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string NodeType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int? EstimatedMinutes { get; set; }
        public string? AiReason { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsClickable { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
        public DateOnly? ScheduledDate { get; set; }
        public string? TopicName { get; set; }
        public string? PathPhase { get; set; }
    }
}
