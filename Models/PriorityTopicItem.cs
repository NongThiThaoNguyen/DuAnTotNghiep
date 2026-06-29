namespace DuAnTotNghiep.Models
{
    public class PriorityTopicItem
    {
        public int TopicId { get; set; }
        public string TopicCode { get; set; } = null!;
        public string TopicTitle { get; set; } = null!;
        public string SkillCode { get; set; } = null!;
        public decimal AccuracyPercentage { get; set; }
        public decimal PriorityScore { get; set; }
        public int Rank { get; set; }
        public string ReasonCode { get; set; } = null!;
        public string PriorityReason { get; set; } = null!;
        public bool PrerequisitesMet { get; set; } = true;
    }
}
