namespace DuAnTotNghiep.Models.DTOs.Topic
{
    public class TopicListItemDto
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string ParentTopicName { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public int ObjectiveCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
