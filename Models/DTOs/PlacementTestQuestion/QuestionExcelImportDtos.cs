using System.Collections.Generic;
using System.Linq;

namespace DuAnTotNghiep.Models.DTOs.PlacementTestQuestion
{
    public class QuestionExcelRowDto
    {
        public int RowNumber { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MULTIPLE_CHOICE";
        public string DifficultyLevel { get; set; } = "MEDIUM";
        public decimal Points { get; set; } = 1.0m;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? Explanation { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsValid => !Errors.Any();
    }

    public class QuestionExcelPreviewResultDto
    {
        public int SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int ValidCount { get; set; }
        public int ErrorCount { get; set; }
        public List<QuestionExcelRowDto> Rows { get; set; } = new();
    }

    public class CreateAndAttachQuestionDto
    {
        public int SectionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MULTIPLE_CHOICE";
        public string DifficultyLevel { get; set; } = "MEDIUM";
        public decimal Points { get; set; } = 1.0m;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? Explanation { get; set; }
    }
}
