using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class PlacementTestDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? TargetLevelId { get; set; }
        public string? TargetLevelName { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public decimal TotalScore { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        
        public int SectionCount { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
    }
}
