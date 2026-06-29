using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Admin.PlacementTest
{
    public class CreatePlacementTestViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int? TargetLevelId { get; set; }

        [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes")]
        public int? TimeLimitMinutes { get; set; }

        public List<int> SelectedSkills { get; set; } = new List<int>();

        public List<SectionViewModel> SelectedSections { get; set; } = new List<SectionViewModel>();
    }
}
