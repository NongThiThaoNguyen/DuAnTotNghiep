using System;

namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class SaveAnswerResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public DateTime SavedAt { get; set; }
    }
}
