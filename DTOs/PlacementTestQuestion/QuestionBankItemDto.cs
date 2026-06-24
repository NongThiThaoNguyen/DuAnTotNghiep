namespace DuAnTotNghiep.DTOs.PlacementTestQuestion
{
    public class QuestionBankItemDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = null!;
        public string QuestionType { get; set; } = null!;
        public string DifficultyLevel { get; set; } = null!;
        public string SkillName { get; set; } = null!;
        public string? TopicName { get; set; }
    }
}
