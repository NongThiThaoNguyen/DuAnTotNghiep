using System;

namespace DuAnTotNghiep.ViewModels
{
    public class AiContentListItemViewModel
    {
        public int Id { get; set; }
        public string? ContentType { get; set; }
        public string? QuestionPreview { get; set; }
        public int? RequestedBy { get; set; }
        public string? RequestedByName { get; set; }
        public string? ReviewStatus { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? RelatedTopicId { get; set; }
        public string? TopicName { get; set; }
    }
}
