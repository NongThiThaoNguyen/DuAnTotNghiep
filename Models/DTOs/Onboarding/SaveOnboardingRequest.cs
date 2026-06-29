using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.Onboarding;

public class SaveOnboardingRequest
{
    // Chú ý: UserId sẽ được lấy từ User.Identity ở phía Controller,
    // Không nhận từ Client để bảo mật.

    [Required(ErrorMessage = "SelectedGoalId is required")]
    public int SelectedGoalId { get; set; }
    
    [Required(ErrorMessage = "SelectedLevelId is required")]
    public int SelectedLevelId { get; set; }
    
    public List<SkillPriorityDto> SkillPreferences { get; set; } = new List<SkillPriorityDto>();
    
    [Range(1, 168, ErrorMessage = "WeeklyHours must be between 1 and 168")]
    public int WeeklyHours { get; set; }
    
    [StringLength(100, ErrorMessage = "PreferredStudyTime cannot exceed 100 characters")]
    public string? PreferredStudyTime { get; set; }
    
    public List<StudySlotDto> AvailableStudySlots { get; set; } = new List<StudySlotDto>();
}
