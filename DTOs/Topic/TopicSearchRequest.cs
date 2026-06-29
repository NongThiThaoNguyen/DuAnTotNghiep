namespace DuAnTotNghiep.DTOs.Topic
{
    public class TopicSearchRequest
    {
        public string? Keyword { get; set; }
        public int? SkillId { get; set; }
        public int? LevelId { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? Status { get; set; }
        public int? ParentTopicId { get; set; }
        
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;
    }
}
