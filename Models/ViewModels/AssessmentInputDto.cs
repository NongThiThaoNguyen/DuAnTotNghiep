using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels
{
    public class AssessmentInputDto
    {
        public int AttemptId { get; set; }
        public int StudentId { get; set; }
        public int PlacementTestId { get; set; }
        public string TestTitle { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxPossibleScore { get; set; }

        // Quantitative statistics by English Skills
        public List<SkillScoreDto> SkillScores { get; set; } = new List<SkillScoreDto>();

        // Quantitative statistics by Learning Topics
        public List<TopicScoreDto> TopicScores { get; set; } = new List<TopicScoreDto>();

        // Quantitative statistics by Difficulty Levels
        public List<DifficultyScoreDto> DifficultyScores { get; set; } = new List<DifficultyScoreDto>();

        // List of wrong answers for learning gap analysis
        public List<WrongAnswerDto> WrongAnswers { get; set; } = new List<WrongAnswerDto>();

        // System warnings (e.g. missing metadata, unmapped topic/skill)
        public List<string> ValidationWarnings { get; set; } = new List<string>();
    }

    public class SkillScoreDto
    {
        public int SkillId { get; set; }
        public string SkillCode { get; set; } = null!;
        public string SkillName { get; set; } = null!;
        public decimal EarnedScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal AccuracyPercentage { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }

    public class TopicScoreDto
    {
        public int TopicId { get; set; }
        public string TopicCode { get; set; } = null!;
        public string TopicTitle { get; set; } = null!;
        public int SkillId { get; set; }
        public decimal EarnedScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal AccuracyPercentage { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }

    public class DifficultyScoreDto
    {
        public string DifficultyLevel { get; set; } = null!; // BASIC, MEDIUM, ADVANCED
        public decimal EarnedScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal AccuracyPercentage { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
    }

    public class WrongAnswerDto
    {
        public int QuestionId { get; set; }
        public int SkillId { get; set; }
        public int? TopicId { get; set; }
        public string QuestionText { get; set; } = null!;
        public string QuestionType { get; set; } = null!; // MCQ, SHORT_ANSWER, etc.
        public string? CorrectAnswerText { get; set; }
        public string? StudentAnswerText { get; set; }
        public string? Explanation { get; set; }
    }

    public class LearningTopicDto
    {
        public int TopicId { get; set; }
        public string TopicCode { get; set; } = null!;
        public string TopicTitle { get; set; } = null!;
        public int SkillId { get; set; }
        public int? ParentTopicId { get; set; }
        public string Status { get; set; } = null!;
    }
}
