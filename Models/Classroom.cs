using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models;

[Table("classrooms")]
public class Classroom
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string ClassName { get; set; } = null!;

    [Required]
    [ForeignKey("Level")]
    public int LevelId { get; set; }

    [Required]
    [ForeignKey("Teacher")]
    public int TeacherId { get; set; }

    public int MaxStudents { get; set; } = 30;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "ACTIVE"; // ACTIVE, FULL, CLOSED

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual EnglishProficiencyLevel Level { get; set; } = null!;
    public virtual User Teacher { get; set; } = null!;
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}
