using System;

namespace DuAnTotNghiep.Models.DTOs.Progress
{
    public class ProgressSnapshotDto
    {
        public int StudentId { get; set; }
        public int? SkillId { get; set; }
        public int? TopicId { get; set; }
        public decimal ProgressPercent { get; set; }
        public decimal? AverageScore { get; set; }
        public int TotalStudyMinutes { get; set; }
        public int CompletedNodes { get; set; }
        public string? WeakPoints { get; set; }
        public DateOnly SnapshotDate { get; set; }
    }
}
