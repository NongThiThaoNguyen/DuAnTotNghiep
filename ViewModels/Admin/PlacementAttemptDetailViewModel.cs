using System;
using System.Collections.Generic;
using DuAnTotNghiep.DTOs.PlacementTest;

namespace DuAnTotNghiep.ViewModels.Admin
{
    public class PlacementAttemptDetailViewModel
    {
        public int AttemptId { get; set; }

        public string StudentName { get; set; } = null!;

        public string TestTitle { get; set; } = null!;

        public decimal TotalScore { get; set; }

        public string? EstimatedLevel { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public string Status { get; set; } = null!;

        public List<DuAnTotNghiep.DTOs.PlacementTest.SkillScoreDto> SkillScores { get; set; } = new List<DuAnTotNghiep.DTOs.PlacementTest.SkillScoreDto>();

        public List<DuAnTotNghiep.DTOs.PlacementTest.TopicScoreDto> TopicScores { get; set; } = new List<DuAnTotNghiep.DTOs.PlacementTest.TopicScoreDto>();

        public string AiAnalysisStatus { get; set; } = null!;
    }
}
