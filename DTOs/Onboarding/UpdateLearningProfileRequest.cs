using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.DTOs.Onboarding;

public class UpdateLearningProfileRequest
{
    public int? SelectedGoalId { get; set; }
    
    public int? SelectedLevelId { get; set; }
    
    public List<SkillPriorityDto>? SkillPreferences { get; set; }
    
    [Range(1, 168)]
    public int? WeeklyHours { get; set; }
    
    [StringLength(100)]
    public string? PreferredStudyTime { get; set; }
    
    public List<StudySlotDto>? AvailableStudySlots { get; set; }
}
