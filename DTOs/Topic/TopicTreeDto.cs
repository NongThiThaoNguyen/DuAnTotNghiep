using System.Collections.Generic;

namespace DuAnTotNghiep.DTOs.Topic
{
    public class TopicTreeDto
    {
        public int Id { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? ParentTopicId { get; set; }
        public int OrderIndex { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public List<int> PrerequisiteTopicIds { get; set; } = new List<int>();
        public List<TopicTreeDto> Children { get; set; } = new List<TopicTreeDto>();
    }
}
