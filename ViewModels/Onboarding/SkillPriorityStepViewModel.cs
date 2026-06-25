using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding;

public class SkillPriorityStepViewModel
{
    public List<SkillSelectionItem> AvailableSkills { get; set; } = new List<SkillSelectionItem>();
    
    public List<SkillSelectionItem> SelectedSkills { get; set; } = new List<SkillSelectionItem>();
}

public class SkillSelectionItem
{
    [Required]
    public string SkillCode { get; set; } = null!;
    
    public string? SkillName { get; set; }
    
    [Range(1, 10, ErrorMessage = "Mức độ ưu tiên không hợp lệ.")]
    public int PriorityLevel { get; set; }
}
