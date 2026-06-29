using System;
using System.Collections.Generic;
using DuAnTotNghiep.DTOs.PlacementTest;

namespace DuAnTotNghiep.ViewModels.PlacementTest
{
    public class TestResultViewModel
    {
        public int AttemptId { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public string EstimatedLevel { get; set; } = "Chưa đánh giá";

        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }

        public List<DuAnTotNghiep.DTOs.PlacementTest.SkillScoreDto> SkillScores { get; set; } = new List<DuAnTotNghiep.DTOs.PlacementTest.SkillScoreDto>();
        public List<DuAnTotNghiep.DTOs.PlacementTest.TopicScoreDto> TopicScores { get; set; } = new List<DuAnTotNghiep.DTOs.PlacementTest.TopicScoreDto>();

        public string? WeakestSkill { get; set; }
        public string? WeakestTopic { get; set; }

        public string AiAnalysisStatus { get; set; } = "Analyzing";
        public bool AiCompleted { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
