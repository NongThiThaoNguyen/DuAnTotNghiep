using System.Collections.Generic;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class LessonViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Content { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public string? ContentType { get; set; }
    public string? VideoUrl { get; set; }

    public bool IsCompleted { get; set; }

    public List<LessonNavigationItemViewModel> LessonsInCourse { get; set; } = new();

    public int? PreviousLessonId { get; set; }
    public int? NextLessonId { get; set; }
}

public class LessonNavigationItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCurrent { get; set; }
}
