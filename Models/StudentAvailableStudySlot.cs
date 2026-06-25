using System;

namespace DuAnTotNghiep.Models;

public partial class StudentAvailableStudySlot
{
    public int Id { get; set; }

    public int StudentProfileId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual StudentLearningProfile StudentProfile { get; set; } = null!;
}
