using System;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class PlacementTestListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? TargetLevelName { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public decimal TotalScore { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int AttemptCount { get; set; }
    }
}
