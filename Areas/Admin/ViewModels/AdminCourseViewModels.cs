using System;
using System.Collections.Generic;
using DuAnTotNghiep.Models.ViewModels.Teacher;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class AdminCourseListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TopicCode { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string? LevelName { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminCourseDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? TopicCode { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string? LevelName { get; set; }
        public string DifficultyLevel { get; set; } = string.Empty;
        public int? EstimatedMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public int StudentCount { get; set; }
        public List<LessonSummaryViewModel> Lessons { get; set; } = new();
        public List<QuizSummaryViewModel> Quizzes { get; set; } = new();
    }
}
