namespace DuAnTotNghiep.Models.DTOs.Onboarding
{
    public class SkillDto
    {
        public int Id { get; set; }
        public string SkillCode { get; set; } = string.Empty;
        public string SkillName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
