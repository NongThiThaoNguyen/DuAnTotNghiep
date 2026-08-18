using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

[Table("enrollments")]
public class Enrollment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey("Student")]
    public int StudentId { get; set; }

    [Required]
    [ForeignKey("Classroom")]
    public int ClassroomId { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, DROPPED, COMPLETED

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public DateTime? DroppedAt { get; set; }

    // Navigation properties
    public virtual User Student { get; set; } = null!;
    public virtual Classroom Classroom { get; set; } = null!;
}
