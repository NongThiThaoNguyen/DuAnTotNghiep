namespace DuAnTotNghiep.DTOs.LearningPath;

/// <summary>
/// Represents one phase in an AI-generated learning path.
/// </summary>
public class LearningPathOutputPhaseDto
{
    public string PhaseName { get; set; } = string.Empty;

    public int Weeks { get; set; }

    public List<LearningPathOutputNodeDto> Nodes { get; set; } = new();
}
