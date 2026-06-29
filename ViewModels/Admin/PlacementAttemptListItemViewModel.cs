using System;

namespace DuAnTotNghiep.ViewModels.Admin
{
    public class PlacementAttemptListItemViewModel
    {
        public int AttemptId { get; set; }
        
        public int StudentId { get; set; }
        
        public string StudentName { get; set; } = null!;
        
        public string? Email { get; set; }
        
        public int PlacementTestId { get; set; }
        
        public string TestTitle { get; set; } = null!;
        
        public decimal TotalScore { get; set; }
        
        public string? EstimatedLevel { get; set; }
        
        public string Status { get; set; } = null!;
        
        public DateTime? SubmittedAt { get; set; }
    }
}
