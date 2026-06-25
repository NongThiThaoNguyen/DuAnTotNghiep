using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.ViewModels.Onboarding;

public class StudyTimeStepViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập số giờ học mỗi tuần.")]
    [Range(1, 168, ErrorMessage = "Số giờ học mỗi tuần phải từ 1 đến 168.")]
    [Display(Name = "Số giờ học hàng tuần")]
    public int WeeklyHours { get; set; }

    [StringLength(100, ErrorMessage = "Độ dài thời gian ưu tiên tối đa 100 ký tự.")]
    [Display(Name = "Khung giờ học ưu tiên (sáng, tối...)")]
    public string? PreferredStudyTime { get; set; }

    public List<StudySlotViewModel> AvailableStudySlots { get; set; } = new List<StudySlotViewModel>();
}

public class StudySlotViewModel
{
    public int DayOfWeek { get; set; }
    
    public string? DayName { get; set; }
    
    public bool IsSelected { get; set; }
    
    [DataType(DataType.Time)]
    public TimeSpan? StartTime { get; set; }
    
    [DataType(DataType.Time)]
    public TimeSpan? EndTime { get; set; }
}
