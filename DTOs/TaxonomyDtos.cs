using System;

namespace DuAnTotNghiep.DTOs
{
    public class SkillDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class TopicDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int SkillId { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
    }

    public class ProficiencyLevelDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
    }

    public class TaxonomyValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
