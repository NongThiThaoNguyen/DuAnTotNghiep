namespace DuAnTotNghiep.Models.ViewModels.LearningPath.M8;

/// <summary>
/// Represents the student's readiness to generate an AI learning path.
/// </summary>
public class LearningPathGenerateViewModel
{
    public int StudentId { get; set; }

    public bool HasOnboarding { get; set; }

    public bool HasPlacementTest { get; set; }

    public bool HasCompetencyAnalysis { get; set; }

    public bool HasActivePath { get; set; }

    public string MissingStep { get; set; } = string.Empty;
}
