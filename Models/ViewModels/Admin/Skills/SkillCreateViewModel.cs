using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels.Admin.Skills
{
    public class SkillCreateViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Skill Code")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Skill Name")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
