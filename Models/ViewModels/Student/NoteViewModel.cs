using System;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    public class NoteViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public string? LessonName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
