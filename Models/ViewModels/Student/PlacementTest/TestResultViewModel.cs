using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Student.PlacementTest
{
    public class TestResultViewModel
    {
        public int AttemptId { get; set; }
        
        public decimal TotalScore { get; set; }
        
        public string? EstimatedLevel { get; set; }
        
        public DateTime SubmittedAt { get; set; }

        public List<SkillScoreViewModel> SkillScores { get; set; } = new List<SkillScoreViewModel>();
    }

    public class SkillScoreViewModel
    {
        public string SkillName { get; set; } = null!;
        public int TotalQuestions { get; set; }
        public int CorrectQuestions { get; set; }
        public decimal Score { get; set; }
    }
}
