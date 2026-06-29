using System;

namespace DuAnTotNghiep.Models.DTOs.Progress
{
    public class ActivityLogCreateDto
    {
        public int StudentId { get; set; }
        public string ActivityType { get; set; } = null!;
        public int? TopicId { get; set; }
        public int? LearningPathNodeId { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Score { get; set; }
        public string? Metadata { get; set; }
    }
}
