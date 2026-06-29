using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels.Admin.Levels
{
    public class LevelCreateViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Level Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Level Name")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
