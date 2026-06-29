namespace DuAnTotNghiep.Models.DTOs.LearningPath;

/// <summary>
/// Represents the full learning path structure returned by the AI path engine.
/// </summary>
public class LearningPathOutputDto
{
    public string PathTitle { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public int TotalWeeks { get; set; }

    public List<LearningPathOutputPhaseDto> Phases { get; set; } = new();
}
