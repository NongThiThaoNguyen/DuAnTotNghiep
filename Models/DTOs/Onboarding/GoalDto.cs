namespace DuAnTotNghiep.Models.DTOs.Onboarding
{
    public class GoalDto
    {
        public int Id { get; set; }
        public string GoalCode { get; set; } = string.Empty;
        public string GoalName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
