namespace DuAnTotNghiep.ViewModels.Progress
{
    public class SkillProgressViewModel
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string SkillCode { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
        public decimal? AverageScore { get; set; }
        public int CompletedNodes { get; set; }
        public int TotalNodes { get; set; }
    }
}
