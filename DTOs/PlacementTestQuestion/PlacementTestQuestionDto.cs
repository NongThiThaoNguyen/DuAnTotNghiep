namespace DuAnTotNghiep.DTOs.PlacementTestQuestion
{
    public class PlacementTestQuestionDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public int QuestionId { get; set; }
        public decimal Points { get; set; }
        public int OrderIndex { get; set; }

        public string QuestionText { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public string DifficultyLevel { get; set; } = null!;
        public string SkillName { get; set; } = null!;
    }
}
