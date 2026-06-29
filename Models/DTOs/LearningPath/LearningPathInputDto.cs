namespace DuAnTotNghiep.Models.DTOs.LearningPath;

/// <summary>
/// Contains normalized student profile and catalog data used to generate a learning path.
/// </summary>
public class LearningPathInputDto
{
    public int StudentId { get; set; }

    public string GoalName { get; set; } = string.Empty;

    public string TargetLevelName { get; set; } = string.Empty;

    public string CurrentLevelName { get; set; } = string.Empty;

    public int AvailableMinutesPerDay { get; set; }

    public List<string> SkillPriorities { get; set; } = new();

    public string Strengths { get; set; } = string.Empty;

    public string Weaknesses { get; set; } = string.Empty;

    public List<string> PriorityTopics { get; set; } = new();

    public List<LearningPathResourceDto> AvailableTopics { get; set; } = new();

    public List<LearningPathResourceDto> AvailableLessons { get; set; } = new();

    public List<LearningPathResourceDto> AvailableQuizzes { get; set; } = new();
}
