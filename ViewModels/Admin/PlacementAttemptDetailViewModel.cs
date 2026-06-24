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

        public List<SkillScoreDto> SkillScores { get; set; } = new List<SkillScoreDto>();

        public List<TopicScoreDto> TopicScores { get; set; } = new List<TopicScoreDto>();

        public string AiAnalysisStatus { get; set; } = null!;
    }
}
