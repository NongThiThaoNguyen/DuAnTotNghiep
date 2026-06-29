using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class AnswerInputDto
    {
        [Required]
        public int AttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        // SelectedAnswer có thể là ID của Option (nếu là trắc nghiệm) hoặc Text (nếu là điền từ)
        // Tuyệt đối không có Score hay IsCorrect ở đây để tránh gian lận.
        public int? SelectedOptionId { get; set; }
        
        public string? AnswerText { get; set; }
    }
}
