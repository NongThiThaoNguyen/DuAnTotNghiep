using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models
{
    /// <summary>
    /// Root object representing the AI's JSON response for the competency assessment.
    /// </summary>
    public class AssessmentAiResponse
    {
        public string Summary { get; set; } = null!;
        public List<StrengthItem> Strengths { get; set; } = new();
        public List<WeaknessItem> Weaknesses { get; set; } = new();
        public List<GapItem> Gaps { get; set; } = new();
        public string CurrentLevel { get; set; } = null!;
        public string RecommendedLevel { get; set; } = null!;
        public decimal ConfidenceScore { get; set; }
        public List<PriorityTopicItem> PriorityTopics { get; set; } = new();
        public List<string> RecommendedActions { get; set; } = new();
    }
}
