namespace DuAnTotNghiep.ViewModels.LearningPath
{
    public class PathProgressSummaryViewModel
    {
        public int TotalNodes { get; set; }
        public int CompletedNodes { get; set; }
        public int InProgressNodes { get; set; }
        public decimal ProgressPercent { get; set; }
        public int TotalStudyMinutes { get; set; }
        public int CurrentStreak { get; set; }
    }
}
