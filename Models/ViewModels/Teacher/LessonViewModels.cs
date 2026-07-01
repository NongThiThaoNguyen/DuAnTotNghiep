namespace DuAnTotNghiep.Models.ViewModels.Teacher;

public class LessonIndexViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<LessonListItemViewModel> Lessons { get; set; } = new();
}

public class LessonListItemViewModel
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int? EstimatedMinutes { get; set; }
    public int OrderIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LessonDetailViewModel
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? ContentType { get; set; } = string.Empty;
    public int? EstimatedMinutes { get; set; }
    public string? VideoUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateLessonViewModel
{
    public int TopicId { get; set; }
    public string TopicTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string? VideoUrl { get; set; }
    public string DifficultyLevel { get; set; } = "BASIC";
}

public class EditLessonViewModel : CreateLessonViewModel
{
    public int Id { get; set; }
}
