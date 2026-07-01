using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    public class NoteCreateUpdateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MaxLength(255, ErrorMessage = "Tiêu đề không được quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; } = string.Empty;

        public int? TopicId { get; set; }
        public int? LessonId { get; set; }
    }
}
