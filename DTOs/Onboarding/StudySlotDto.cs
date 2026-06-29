using System;

namespace DuAnTotNghiep.DTOs.Onboarding;

public class StudySlotDto
{
    public int DayOfWeek { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public TimeSpan EndTime { get; set; }
}
