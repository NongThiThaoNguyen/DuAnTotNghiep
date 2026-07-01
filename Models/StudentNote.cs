using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuAnTotNghiep.Models
{
    [Table("student_notes")]
    public class StudentNote
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("topic_id")]
        public int? TopicId { get; set; }

        [Column("lesson_id")]
        public int? LessonId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("TopicId")]
        public virtual LearningTopic? Topic { get; set; }

        [ForeignKey("LessonId")]
        public virtual OriginalLesson? Lesson { get; set; }
    }
}
