using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Student;

public class StudentScheduleViewModel
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd => WeekStart.AddDays(6);
    public List<StudentScheduleDayViewModel> Days { get; set; } = new();
    public bool HasEnrollment { get; set; }
}

public class StudentScheduleDayViewModel
{
    public DateTime Date { get; set; }
    public List<StudentScheduleItemViewModel> Items { get; set; } = new();
}

public class StudentScheduleItemViewModel
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string EnrollmentStatus { get; set; } = string.Empty;
}