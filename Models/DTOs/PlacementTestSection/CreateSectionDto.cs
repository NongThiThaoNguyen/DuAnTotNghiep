using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.PlacementTestSection
{
    public class CreateSectionDto
    {
        [Required]
        public int PlacementTestId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn kỹ năng (Skill)")]
        public int SkillId { get; set; }

        [Required(ErrorMessage = "Tên phần thi không được để trống")]
        [MinLength(2, ErrorMessage = "Tên phần thi phải từ 2 ký tự")]
        public string SectionName { get; set; } = null!;

        public string? Instruction { get; set; }

        public int OrderIndex { get; set; }

        [Range(0, 1000, ErrorMessage = "Điểm tối đa không hợp lệ")]
        public decimal MaxScore { get; set; }
    }
}
