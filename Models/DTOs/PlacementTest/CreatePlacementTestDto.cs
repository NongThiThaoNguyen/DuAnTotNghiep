using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.PlacementTest
{
    public class CreatePlacementTestDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [MinLength(3, ErrorMessage = "Tiêu đề phải từ 3 ký tự")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Trình độ mục tiêu không được để trống")]
        public int TargetLevelId { get; set; }

        [Range(1, 300, ErrorMessage = "Thời gian làm bài phải lớn hơn 0 và nhỏ hơn 300 phút")]
        public int? TimeLimitMinutes { get; set; }

        [Range(1, 1000, ErrorMessage = "Tổng điểm phải lớn hơn 0")]
        public decimal TotalScore { get; set; }
    }
}
