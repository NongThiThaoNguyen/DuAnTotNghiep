using System.ComponentModel.DataAnnotations;

namespace DuAnTotNghiep.Models.DTOs.Skill
{
    public class SkillOptionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class SkillListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SkillDetailDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSkillDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSkillDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
