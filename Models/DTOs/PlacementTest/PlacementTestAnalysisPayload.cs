using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class PlacementTestAnalysisPayload
    {
        public int StudentId { get; set; }

        public int AttemptId { get; set; }

        public string CurrentLevel { get; set; } = string.Empty;

        public string TargetLevel { get; set; } = string.Empty;

        public string LearningGoal { get; set; } = string.Empty;

        public List<string> PreferredTopics { get; set; } = new List<string>();

        public int? StudyTimePerDay { get; set; }

        public decimal TotalScore { get; set; }

        public string EstimatedLevel { get; set; } = string.Empty;

        public List<SkillScoreDto> SkillScores { get; set; } = new List<SkillScoreDto>();

        public List<TopicScoreDto> TopicScores { get; set; } = new List<TopicScoreDto>();

        public List<WrongAnswerDto> WrongAnswers { get; set; } = new List<WrongAnswerDto>();
    }
}
