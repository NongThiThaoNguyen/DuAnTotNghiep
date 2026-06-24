using System;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class TestAttemptDto
    {
        public int Id { get; set; }
        public int PlacementTestId { get; set; }
        public int StudentId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal? TotalScore { get; set; }
        public int? EstimatedLevelId { get; set; }
        public string Status { get; set; } = null!;
    }
}
