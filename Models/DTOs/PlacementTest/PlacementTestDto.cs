using System;

namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class PlacementTestDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? TargetLevelId { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public decimal TotalScore { get; set; }
        public string Status { get; set; } = null!;
    }
}
