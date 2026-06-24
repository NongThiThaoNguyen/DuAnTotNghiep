using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels.PlacementTest
{
    public class TestTakingViewModel
    {
        public int AttemptId { get; set; }
        public string TestTitle { get; set; } = null!;
        public DateTime ServerTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? RemainingSeconds { get; set; }
        public string Status { get; set; } = null!;
        public List<TestSectionViewModel> Sections { get; set; } = new();
    }

    public class TestSectionViewModel
    {
        public int SectionId { get; set; }
        public string SectionTitle { get; set; } = null!;
        public string SkillName { get; set; } = null!;
        public string? Instruction { get; set; }
        public int OrderIndex { get; set; }
        public List<TestQuestionViewModel> Questions { get; set; } = new();
    }

    public class TestQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Points { get; set; }
        public int OrderIndex { get; set; }
        public string? ExistingAnswer { get; set; }
        public List<QuestionOptionViewModel> Options { get; set; } = new();
    }

    public class QuestionOptionViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;
    }
}
