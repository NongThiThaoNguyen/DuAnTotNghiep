using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

[Table("schedules")]
public class Schedule
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Teacher")]
    public int TeacherId { get; set; }

    [ForeignKey("Topic")]
    public int? TopicId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [StringLength(255)]
    public string? Classroom { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Teacher { get; set; } = null!;
    public virtual LearningTopic? Topic { get; set; }
}
