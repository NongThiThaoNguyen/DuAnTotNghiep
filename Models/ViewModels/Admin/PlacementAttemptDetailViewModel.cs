using System;
using System.Collections.Generic;
using DuAnTotNghiep.Models.DTOs.PlacementTest;

namespace DuAnTotNghiep.Models.ViewModels.Admin
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

        public List<DuAnTotNghiep.Models.DTOs.PlacementTest.SkillScoreDto> SkillScores { get; set; } = new List<DuAnTotNghiep.Models.DTOs.PlacementTest.SkillScoreDto>();

        public List<DuAnTotNghiep.Models.DTOs.PlacementTest.TopicScoreDto> TopicScores { get; set; } = new List<DuAnTotNghiep.Models.DTOs.PlacementTest.TopicScoreDto>();

        public string AiAnalysisStatus { get; set; } = null!;
    }
}
