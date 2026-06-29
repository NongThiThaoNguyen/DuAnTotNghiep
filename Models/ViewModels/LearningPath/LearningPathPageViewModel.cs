namespace DuAnTotNghiep.Models.ViewModels.LearningPath
{
    public class LearningPathPageViewModel
    {
        public int? PathId { get; set; }
        public string PathTitle { get; set; } = string.Empty;
        public string? PathDescription { get; set; }
        public string PathStatus { get; set; } = string.Empty;
        public DateOnly? StartDate { get; set; }
        public DateOnly? TargetEndDate { get; set; }
        public bool GeneratedByAi { get; set; }
        public string? AiPlanSummary { get; set; }
        public bool HasPath { get; set; }
        public List<PathNodeViewModel> Nodes { get; set; } = new();
        public List<TodayTaskViewModel> TodayTasks { get; set; } = new();
        public PathProgressSummaryViewModel Progress { get; set; } = new();
    }
}
