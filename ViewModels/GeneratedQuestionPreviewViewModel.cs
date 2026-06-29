using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels
{
    public class GeneratedQuestionPreviewViewModel
    {
        public string? QuestionText { get; set; }
        public List<string>? Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
        public string? Explanation { get; set; }
        public string? Difficulty { get; set; }
        public List<string>? Tags { get; set; }
    }
}
