using System;
using System.Collections.Generic;

namespace DuAnTotNghiep.Models.ViewModels.Student
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "";
        public string AvatarUrl { get; set; } = "";
        public int StreakDays { get; set; }
        public int TotalXp { get; set; }
        public int Level { get; set; }
        public string RankTier { get; set; } = "Bronze";
        public int CompletedLessons { get; set; }
        public int CompletedQuizzes { get; set; }
        public decimal AverageQuizScore { get; set; }
        public double ProgressPercent { get; set; }
        public int StudyMinutesThisWeek { get; set; }
        public string? CurrentLevel { get; set; }
        public string? TargetLevel { get; set; }
        public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
        public NextTaskViewModel? NextTask { get; set; }
        public string? AiRecommendation { get; set; }
    }

    public class ActivityItemViewModel
    {
        public string ActivityType { get; set; } = "";
        public string ActivityTypeLabel { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Score { get; set; }
        public string? IconClass { get; set; }
    }

    public class NextTaskViewModel
    {
        public int NodeId { get; set; }
        public string Title { get; set; } = "";
        public string NodeType { get; set; } = "";
        public string? TargetUrl { get; set; }
        public int EstimatedMinutes { get; set; }
    }
}
