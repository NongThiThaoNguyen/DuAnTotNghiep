namespace DuAnTotNghiep.Models.DTOs.Topic
{
    public class CreateTopicDto
    {
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SkillId { get; set; }
        public int? LevelId { get; set; }
        public int? ParentTopicId { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
    }
}
