using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

[Table("attendances")]
public class Attendance
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Student")]
    public int StudentId { get; set; }

    [Required]
    [ForeignKey("Topic")]
    public int TopicId { get; set; }

    [Required]
    public DateOnly AttendanceDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "PRESENT"; // PRESENT, ABSENT, LATE, EXCUSED

    [StringLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Student { get; set; } = null!;
    public virtual LearningTopic Topic { get; set; } = null!;
}
