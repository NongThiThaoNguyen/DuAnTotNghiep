using System.Collections.Generic;

namespace DuAnTotNghiep.ViewModels.Student.PlacementTest
{
    public class TestTakingViewModel
    {
        public int AttemptId { get; set; }
        
        public int PlacementTestId { get; set; }
        
        public string TestTitle { get; set; } = null!;
        
        public int RemainingTimeSeconds { get; set; }

        public List<TestSectionViewModel> Sections { get; set; } = new List<TestSectionViewModel>();
    }

    public class TestSectionViewModel
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = null!;
        public string? Instruction { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }
}
