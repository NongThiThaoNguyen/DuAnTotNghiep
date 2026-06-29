using System.Collections.Generic;

namespace DuAnTotNghiep.DTOs.PlacementTest
{
    public class ScoreResultDto
    {
        public decimal TotalScore { get; set; }
        
        public int CorrectAnswers { get; set; }
        
        public int WrongAnswers { get; set; }
        
        public string? EstimatedLevel { get; set; }
        
        public List<SkillScoreDto> SkillScores { get; set; } = new List<SkillScoreDto>();
    }

    public class SkillScoreDto
    {
        public int SkillId { get; set; }
        
        public string SkillName { get; set; } = null!;
        
        public decimal EarnedScore { get; set; }
        
        public decimal MaxScore { get; set; }
        
        public decimal Percentage { get; set; }
    }

    public class TopicScoreDto
    {
        public int TopicId { get; set; }
        
        public string TopicName { get; set; } = null!;
        
        public decimal EarnedScore { get; set; }
        
        public decimal MaxScore { get; set; }
        
        public decimal Percentage { get; set; }
    }
}
