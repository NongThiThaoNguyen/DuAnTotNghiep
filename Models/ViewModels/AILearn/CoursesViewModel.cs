using System.Collections.Generic;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class CoursesViewModel
{
    public string SearchQuery { get; set; } = "";
    public string ActiveCategory { get; set; } = "ALL";
    public List<CourseCardViewModel> Courses { get; set; } = new();
    public List<string> Categories { get; set; } = new() { "ALL", "IELTS", "TOEIC", "COMMUNICATION", "GRAMMAR", "VOCABULARY" };
}

public class CourseCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Difficulty { get; set; } = "BEGINNER";
    public string SkillCode { get; set; } = "GRAMMAR";
    public string SkillName { get; set; } = "Ngữ pháp";
    public int LessonCount { get; set; }
    public double ProgressPercent { get; set; }
    public string ThumbnailUrl { get; set; } = "/images/course-thumb.png";
}

public class CourseDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Difficulty { get; set; } = "BEGINNER";
    public string SkillCode { get; set; } = "GRAMMAR";
    public string SkillName { get; set; } = "Ngữ pháp";
    public string? LevelName { get; set; }
    public int? EstimatedMinutes { get; set; }
    public int LessonCount { get; set; }
    public double ProgressPercent { get; set; }
    public int CompletedLessonCount { get; set; }
    public int? FirstLessonId { get; set; }
    public int? LastCompletedLessonId { get; set; }
    public string ThumbnailUrl { get; set; } = "/images/courses/default.svg";
    public List<CourseDetailLessonItem> Lessons { get; set; } = new();
}

public class CourseDetailLessonItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Summary { get; set; }
    public string ContentType { get; set; } = "";
    public int? EstimatedMinutes { get; set; }
    public int OrderIndex { get; set; }
    public bool IsCompleted { get; set; }
}

