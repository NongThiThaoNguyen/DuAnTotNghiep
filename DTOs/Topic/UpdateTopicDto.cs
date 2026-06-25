namespace DuAnTotNghiep.DTOs.Topic
{
    public class UpdateTopicDto
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? LevelId { get; set; }
        public int? ParentTopicId { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
