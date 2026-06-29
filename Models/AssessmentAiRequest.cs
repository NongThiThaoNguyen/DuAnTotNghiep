using System.Collections.Generic;

namespace DuAnTotNghiep.Models
{
    public class AssessmentAiRequest
    {
        public int StudentId { get; set; }
        public int TestAttemptId { get; set; }
        public decimal TotalScore { get; set; }
        public string CurrentLevel { get; set; } = "";
        public string TargetLevel { get; set; } = "";
        public string GoalDescription { get; set; } = "";
        public string SkillScoresSummary { get; set; } = "";
        public string IncorrectQuestionsSummary { get; set; } = "";
        public string TopicScoresSummary { get; set; } = "";
    }
}
