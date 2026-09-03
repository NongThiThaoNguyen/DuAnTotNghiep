using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.LearningPath
{
    public class LevelUpAssessmentViewModel
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public decimal TotalScore { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public string TargetLevelName { get; set; } = string.Empty;
        public int TargetLevelId { get; set; }
        public bool IsExamMode { get; set; } = true;
        public int MaxViolations { get; set; } = 3;
        public List<LevelUpSectionViewModel> Sections { get; set; } = new();
    }

    public class LevelUpSectionViewModel
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public string? SkillName { get; set; }
        public string? Instruction { get; set; }
        public List<LevelUpQuestionViewModel> Questions { get; set; } = new();
    }

    public class LevelUpQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public decimal Points { get; set; }
        public List<LevelUpOptionViewModel> Options { get; set; } = new();
    }

    public class LevelUpOptionViewModel
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    public class LevelUpResultViewModel
    {
        public bool IsSuccess { get; set; }
        public bool Promoted { get; set; }
        public decimal EarnedScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public string CurrentLevelName { get; set; } = string.Empty;
        public string? NewLevelName { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<LevelUpSkillScoreViewModel> SkillBreakdown { get; set; } = new();
    }

    public class LevelUpSkillScoreViewModel
    {
        public string SkillName { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public string Status { get; set; } = "DAT";
    }
}
