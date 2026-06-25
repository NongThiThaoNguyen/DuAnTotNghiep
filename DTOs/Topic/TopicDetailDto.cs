namespace DuAnTotNghiep.DTOs.Topic
{
    public class TopicDetailDto
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public string? ParentTopicName { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty; // added
        public int ObjectiveCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Additional identifiers
        public int? SkillId { get; set; }
        public int? LevelId { get; set; }
        public int? ParentTopicId { get; set; }
        public int OrderIndex { get; set; }
        public string? Difficulty { get; set; } // backward compatibility

        // Navigation collections
        public IEnumerable<DuAnTotNghiep.DTOs.Objective.ObjectiveDto>? Objectives { get; set; }
        public IEnumerable<DuAnTotNghiep.DTOs.Topic.TopicOptionDto>? Prerequisites { get; set; }
    }
}
