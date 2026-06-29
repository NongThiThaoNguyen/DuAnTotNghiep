using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.PlacementTestQuestion
{
    public class AttachQuestionDto
    {
        [Required]
        public int SectionId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Range(0.01, 100, ErrorMessage = "Điểm phải lớn hơn 0 và nhỏ hơn hoặc bằng 100.")]
        public decimal Points { get; set; }

        public int OrderIndex { get; set; }
    }
}
