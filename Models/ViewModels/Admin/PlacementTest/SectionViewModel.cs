using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels.Admin.PlacementTest
{
    public class SectionViewModel
    {
        [Required]
        public int SkillId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = null!;

        public string? Instruction { get; set; }

        [Range(0, 999.99)]
        public decimal MaxScore { get; set; }

        public int OrderIndex { get; set; }
    }
}
