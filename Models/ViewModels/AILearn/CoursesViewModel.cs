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
