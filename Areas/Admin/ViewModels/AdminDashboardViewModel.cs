using System;
using System.Collections.Generic;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Areas.Admin.ViewModels
{
    public class AdminDashboardViewModel
    {
        // General stats counts
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int ActiveUsersToday { get; set; }
        public int TotalTopics { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalLessons { get; set; }
        public int PendingAiContents { get; set; }
        public int TotalPlacementAttempts { get; set; }
        public int TotalPlacementTests { get; set; }
        public double AveragePlacementScore { get; set; }
        public int NewUsersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; }

        public int TotalCourses { get; set; }
        public int TotalLearningPaths { get; set; }
        public int TotalAiRecommendations { get; set; }
        public int TotalAiAnalyses { get; set; }
        public int TotalQuestionBank { get; set; }
        public int TotalReferenceSources { get; set; }

        // Chart Data Lists
        public List<ChartItem> UsersOverTime { get; set; } = new();
        public List<ChartItem> PlacementTestsOverTime { get; set; } = new();
        public List<ChartItem> AiAnalysisData { get; set; } = new();
        public List<ChartItem> LearningProgressData { get; set; } = new();
        public List<ChartItem> RoleDistribution { get; set; } = new();
        public List<ChartItem> LevelDistribution { get; set; } = new();

        // Recent items Lists
        public List<RecentUserViewModel> RecentUsers { get; set; } = new();
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();

        public List<TestAttempt> RecentPlacementTests { get; set; } = new();
        public List<AiFeedback> RecentAiRecommendations { get; set; } = new();
        public List<QuestionBank> NewQuestionBankItems { get; set; } = new();
        public List<ReferenceSource> NewReferenceSources { get; set; } = new();
        public List<StudentLearningPath> NewLearningPaths { get; set; } = new();
    }

    public class RecentUserViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class RecentActivityViewModel
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ChartItem
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }
}
