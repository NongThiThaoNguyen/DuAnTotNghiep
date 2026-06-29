namespace DuAnTotNghiep.ViewModels.LearningPath
{
    public class TodayTaskViewModel
    {
        public int NodeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;
        public string? AiReason { get; set; }
        public int? EstimatedMinutes { get; set; }
        public string? TargetUrl { get; set; }
        public bool IsOverdue { get; set; }
    }
}
