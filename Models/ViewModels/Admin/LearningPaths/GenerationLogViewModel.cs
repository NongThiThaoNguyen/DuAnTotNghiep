namespace DuAnTotNghiep.Models.ViewModels.Admin.LearningPaths;

/// <summary>
/// Represents admin-visible AI generation logs for learning path operations.
/// </summary>
public class GenerationLogViewModel
{
    public string ModuleCode { get; set; } = "LEARNING_PATH";

    public List<GenerationLogItemViewModel> Logs { get; set; } = new();
}

/// <summary>
/// Represents one AI usage log row for learning path generation.
/// </summary>
public class GenerationLogItemViewModel
{
    public long LogId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string AiModel { get; set; } = string.Empty;

    public string RequestStatus { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
