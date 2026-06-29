namespace DuAnTotNghiep.Models.DTOs.PlacementTestSection
{
    public class PlacementTestSectionDto
    {
        public int Id { get; set; }
        public int PlacementTestId { get; set; }
        public int SkillId { get; set; }
        public string SkillName { get; set; } = null!;
        public string SectionName { get; set; } = null!;
        public string? Instruction { get; set; }
        public int OrderIndex { get; set; }
        public decimal MaxScore { get; set; }
        public int QuestionCount { get; set; }
    }
}
