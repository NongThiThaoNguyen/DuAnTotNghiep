using System;

namespace DuAnTotNghiep.ViewModels.Progress
{
    public class LearningHistoryItemViewModel
    {
        public long LogId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string ActivityType { get; set; } = string.Empty; // Learn, Quiz, Practice, etc.
        public string Title { get; set; } = string.Empty; // Lesson/Quiz title
        public string SkillName { get; set; } = string.Empty;
        public int? DurationMinutes { get; set; }
        public decimal? Score { get; set; }
        public string? MetadataJson { get; set; }
    }
}
