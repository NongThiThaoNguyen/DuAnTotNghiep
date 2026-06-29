using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Student.PlacementTest
{
    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        
        public string QuestionText { get; set; } = null!;
        
        public string QuestionType { get; set; } = null!;

        // Chỉ chứa OptionId và Text, KHÔNG CHỨA IsCorrect hay CorrectAnswer
        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();
    }

    public class OptionViewModel
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = null!;
    }
}
