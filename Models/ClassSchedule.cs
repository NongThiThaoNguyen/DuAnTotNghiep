using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

[Table("class_schedules")]
public class ClassSchedule
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Classroom")]
    public int ClassroomId { get; set; }

    /// <summary>
    /// Day of week: 1 = Monday, 2 = Tuesday, ..., 7 = Sunday
    /// </summary>
    [Required]
    public int DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    // Navigation
    public virtual Classroom Classroom { get; set; } = null!;
}
