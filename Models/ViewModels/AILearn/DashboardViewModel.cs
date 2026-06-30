using System.Collections.Generic;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Models.ViewModels.AILearn;

public class DashboardViewModel
{
    public string StudentName { get; set; } = "";
    public string AvatarUrl { get; set; } = "/default-images/avatar.png";
    public string RankTier { get; set; } = "Bronze";
    public string LevelCode { get; set; } = "BEGINNER";
    public string TargetLevelCode { get; set; } = "N/A";
    
    // Stats
    public int StreakDays { get; set; }
    public decimal AverageQuizScore { get; set; }
    public int CompletedLessonsCount { get; set; }
    public int CompletedQuizzesCount { get; set; }
    public int TotalXp { get; set; }
    public int Level { get; set; }
    public double ProgressPercent { get; set; }

    // Activities and Recommendations
    public OriginalLesson? TodayLesson { get; set; }
    public OriginalLesson? ContinueLesson { get; set; }
    public string AiRecommendation { get; set; } = "";
    public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
}

public class ActivityItemViewModel
{
    public string Type { get; set; } = ""; // LESSON, QUIZ, CHAT, COMPETENCY
    public string Title { get; set; } = "";
    public System.DateTime Time { get; set; }
    public string Detail { get; set; } = "";
}
