using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.PlacementTest
{
    public class PlacementTestSuggestionViewModel
    {
        public string GoalName { get; set; } = null!;
        public string CurrentLevelName { get; set; } = null!;
        public List<string> PrioritySkills { get; set; } = new List<string>();
        
        public int SuggestedTestId { get; set; }
        public string SuggestedTestTitle { get; set; } = null!;
        public string? SuggestedTestDescription { get; set; }
        public int? TimeLimitMinutes { get; set; }
    }
}
