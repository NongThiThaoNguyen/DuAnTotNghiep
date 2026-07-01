using DuAnTotNghiep.Models.DTOs.Level;
using DuAnTotNghiep.Models.DTOs.Skill;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DuAnTotNghiep.Models.ViewModels.Teacher;

public class CourseIndexViewModel
{
    public string? Search { get; set; }
    public string? Skill { get; set; }
    public List<CourseListItemViewModel> Courses { get; set; } = new();
    public List<SkillOptionDto> SkillOptions { get; set; } = new();
}

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? LevelName { get; set; }
    public int LessonCount { get; set; }
    public int StudentCount { get; set; }
    public bool IsActive { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CourseDetailViewModel
{
    public int Id { get; set; }
    public int SkillId { get; set; }
    public int? ProficiencyLevelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TopicCode { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string? LevelName { get; set; }
    public string? LevelCode { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public int? EstimatedMinutes { get; set; }
    public int OrderIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<LessonSummaryViewModel> Lessons { get; set; } = new();
    public List<QuizSummaryViewModel> Quizzes { get; set; } = new();
}

public class CreateCourseViewModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SkillId { get; set; }
    public int? ProficiencyLevelId { get; set; }
    public string DifficultyLevel { get; set; } = "BASIC";
    public int? EstimatedMinutes { get; set; }
    [ValidateNever]
    public List<SkillOptionDto> SkillOptions { get; set; } = new();
    [ValidateNever]
    public List<LevelOptionDto> LevelOptions { get; set; } = new();
}

public class EditCourseViewModel : CreateCourseViewModel
{
    public int Id { get; set; }
    public string? TopicCode { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
}

public class LessonSummaryViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class QuizSummaryViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public string Status { get; set; } = string.Empty;
}
