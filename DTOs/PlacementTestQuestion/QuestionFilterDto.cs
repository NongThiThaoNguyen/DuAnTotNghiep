namespace DuAnTotNghiep.DTOs.PlacementTestQuestion
{
    public class QuestionFilterDto
    {
        public int SectionId { get; set; }
        public string? Keyword { get; set; }
        public string? QuestionType { get; set; }
        public string? DifficultyLevel { get; set; }
    }
}
