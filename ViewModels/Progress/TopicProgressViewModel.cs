namespace DuAnTotNghiep.ViewModels.Progress
{
    public class TopicProgressViewModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = string.Empty;
        public string TopicCode { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; }
        public decimal? AverageScore { get; set; }
        public bool IsWeakArea { get; set; }
    }
}
