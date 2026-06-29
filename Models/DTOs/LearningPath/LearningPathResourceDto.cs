namespace DuAnTotNghiep.Models.DTOs.LearningPath;

/// <summary>
/// Represents an available learning resource that can be selected by the path engine.
/// </summary>
public class LearningPathResourceDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
