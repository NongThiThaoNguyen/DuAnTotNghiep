using System.Collections.Generic;

namespace DuAnTotNghiep.DTOs.TopicImport
{
    public class TopicImportRowDto
    {
        public string SkillCode { get; set; } = string.Empty;
        public string LevelCode { get; set; } = string.Empty;
        public string? ParentCode { get; set; }
        public string TopicCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public int EstimatedMinutes { get; set; }
        public int OrderIndex { get; set; }
        public string Status { get; set; } = "Active";
        public string? Objectives { get; set; }
    }
}
