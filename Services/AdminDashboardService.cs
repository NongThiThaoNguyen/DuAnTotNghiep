using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardViewModel> GetSystemOverviewAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var thirtyDaysAgo = now.AddDays(-30);
        var completionStats = await GetLearningPathCompletionStatsAsync();

        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _context.Users.AsNoTracking().CountAsync(),
            TotalStudents = await CountUsersByRoleAsync("STUDENT"),
            TotalTeachers = await CountUsersByRoleAsync("TEACHER"),
            TotalAdmins = await CountUsersByRoleAsync("ADMIN"),
            ActiveUsersToday = await _context.Users.AsNoTracking()
                .CountAsync(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date == today),
            ActiveLearningStudents = await _context.StudentLearningPaths.AsNoTracking()
                .Where(p => p.Status == "ACTIVE" || p.Status == "IN_PROGRESS")
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync(),
            CompletedLearningPaths = completionStats.CompletedPaths,
            LearningPathCompletionRate = completionStats.CompletionRate,
            PlacementAttemptsLast30Days = await _context.TestAttempts.AsNoTracking()
                .CountAsync(a => a.StartedAt >= thirtyDaysAgo),
            UnreadNotifications = await _context.Notifications.AsNoTracking()
                .CountAsync(n => !n.NotificationReads.Any()),
            TotalTopics = await _context.LearningTopics.AsNoTracking()
                .CountAsync(t => t.Status == "PUBLISHED" || t.Status == "ACTIVE"),
            TotalCourses = await GetCourseCountAsync(),
            TotalLessons = await _context.OriginalLessons.AsNoTracking().CountAsync(),
            TotalQuizzes = await _context.Quizzes.AsNoTracking().CountAsync(),
            TotalPlacementTests = await _context.PlacementTests.AsNoTracking().CountAsync(),
            TotalPlacementAttempts = await _context.TestAttempts.AsNoTracking().CountAsync(),
            TotalLearningPaths = completionStats.TotalPaths,
            TotalAiRecommendations = await _context.AiFeedbacks.AsNoTracking().CountAsync(),
            TotalAiAnalyses = await _context.CompetencyAnalyses.AsNoTracking().CountAsync(),
            PendingAiContents = await _context.AiGeneratedContents.AsNoTracking().CountAsync(a => a.ReviewStatus == "PENDING"),
            TotalQuestionBank = await _context.QuestionBanks.AsNoTracking().CountAsync(),
            TotalReferenceSources = await _context.ReferenceSources.AsNoTracking().CountAsync(),
            NewUsersThisWeek = await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= now.AddDays(-7)),
            NewUsersThisMonth = await GetNewUsersThisMonthAsync()
        };

        model.AveragePlacementScore = await GetAveragePlacementScoreAsync();
        model.UsersOverTime = await GetUsersRegisteredByMonthAsync(now);
        model.PlacementTestsOverTime = await GetPlacementAttemptsByDayAsync(thirtyDaysAgo);
        model.AiAnalysisData = await GetAiAnalysisLevelDistributionAsync();
        model.LearningPathCompletionData = completionStats.Items;
        model.LearningProgressData = completionStats.Items;
        model.RoleDistribution = await GetRoleDistributionAsync();
        model.LevelDistribution = await GetLevelDistributionAsync();
        model.RecentUsers = await GetRecentStudentUsersAsync();
        model.RecentActivities = await GetRecentActivitiesAsync();
        model.RecentPlacementTests = await GetRecentPlacementAttemptsAsync();
        model.RecentAiRecommendations = await GetRecentAiRecommendationsAsync();
        model.NewQuestionBankItems = await GetNewQuestionBankItemsAsync();
        model.NewReferenceSources = await GetNewReferenceSourcesAsync();
        model.NewLearningPaths = await GetNewLearningPathsAsync();

        return model;
    }

    public Task<int> GetNewUsersThisMonthAsync()
    {
        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= firstDayOfMonth);
    }

    public async Task<LearningPathCompletionStatsViewModel> GetLearningPathCompletionStatsAsync()
    {
        var statusCounts = await _context.StudentLearningPaths.AsNoTracking()
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var completed = statusCounts
            .Where(s => s.Status == "COMPLETED" || s.Status == "DONE")
            .Sum(s => s.Count);
        var total = statusCounts.Sum(s => s.Count);
        var incomplete = total - completed;

        var items = new List<ChartItem>
        {
            new() { Label = "Hoàn thành", Value = completed },
            new() { Label = "Đang học", Value = incomplete },
        };

        return new LearningPathCompletionStatsViewModel
        {
            TotalPaths = total,
            CompletedPaths = completed,
            CompletionRate = total == 0 ? 0 : Math.Round(completed * 100m / total, 2),
            Items = items
        };
    }

    private Task<int> CountUsersByRoleAsync(string roleCode)
    {
        return _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == roleCode);
    }

    private async Task<int> GetCourseCountAsync()
    {
        var rootTopics = await _context.LearningTopics.AsNoTracking().CountAsync(t => t.ParentTopicId == null);
        return rootTopics > 0 ? rootTopics : await _context.LearningTopics.AsNoTracking().CountAsync();
    }

    private async Task<double> GetAveragePlacementScoreAsync()
    {
        var scores = await _context.TestAttempts.AsNoTracking()
            .Where(a => a.TotalScore != null)
            .Select(a => (double)a.TotalScore!)
            .ToListAsync();

        return scores.Count == 0 ? 0 : Math.Round(scores.Average(), 2);
    }

    private async Task<List<ChartItem>> GetUsersRegisteredByMonthAsync(DateTime now)
    {
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-11);

        var data = await _context.Users.AsNoTracking()
            .Where(u => u.CreatedAt >= start)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync();

        return data.Select(g => new ChartItem
        {
            Label = $"{g.Month:00}/{g.Year}",
            Value = g.Count
        }).ToList();
    }

    private async Task<List<ChartItem>> GetPlacementAttemptsByDayAsync(DateTime startDate)
    {
        var data = await _context.TestAttempts.AsNoTracking()
            .Where(a => a.StartedAt >= startDate)
            .GroupBy(a => a.StartedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        return data.Select(g => new ChartItem
        {
            Label = g.Date.ToString("dd/MM"),
            Value = g.Count
        }).ToList();
    }

    private Task<List<ChartItem>> GetAiAnalysisLevelDistributionAsync()
    {
        return _context.CompetencyAnalyses.AsNoTracking()
            .Include(c => c.RecommendedLevel)
            .Where(c => c.RecommendedLevel != null)
            .GroupBy(c => c.RecommendedLevel!.Name)
            .Select(g => new ChartItem
            {
                Label = g.Key,
                Value = g.Count()
            })
            .ToListAsync();
    }

    private Task<List<ChartItem>> GetRoleDistributionAsync()
    {
        return _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .GroupBy(u => u.Role.RoleName)
            .Select(g => new ChartItem
            {
                Label = g.Key,
                Value = g.Count()
            })
            .ToListAsync();
    }

    private Task<List<ChartItem>> GetLevelDistributionAsync()
    {
        return _context.StudentLearningProfiles.AsNoTracking()
            .Include(p => p.CurrentLevel)
            .Where(p => p.CurrentLevel != null)
            .GroupBy(p => p.CurrentLevel!.Name)
            .Select(g => new ChartItem
            {
                Label = g.Key,
                Value = g.Count()
            })
            .ToListAsync();
    }

    private Task<List<RecentUserViewModel>> GetRecentStudentUsersAsync()
    {
        return _context.Users.AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role.RoleCode == "STUDENT")
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new RecentUserViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                CreatedAt = u.CreatedAt,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync();
    }

    private Task<List<RecentActivityViewModel>> GetRecentActivitiesAsync()
    {
        return _context.StudyActivityLogs.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.Topic)
            .OrderByDescending(a => a.CreatedAt)
            .Take(8)
            .Select(a => new RecentActivityViewModel
            {
                Id = a.Id,
                UserName = a.Student != null ? a.Student.FullName : "System",
                ActivityType = a.ActivityType,
                Description = a.Topic != null ? a.Topic.Title : "N/A",
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    private Task<List<TestAttempt>> GetRecentPlacementAttemptsAsync()
    {
        return _context.TestAttempts.AsNoTracking()
            .Include(a => a.Student)
            .Include(a => a.PlacementTest)
            .OrderByDescending(a => a.StartedAt)
            .Take(5)
            .ToListAsync();
    }

    private Task<List<AiFeedback>> GetRecentAiRecommendationsAsync()
    {
        return _context.AiFeedbacks.AsNoTracking()
            .Include(f => f.Student)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .ToListAsync();
    }

    private Task<List<QuestionBank>> GetNewQuestionBankItemsAsync()
    {
        return _context.QuestionBanks.AsNoTracking()
            .Include(q => q.Skill)
            .Include(q => q.Level)
            .OrderByDescending(q => q.CreatedAt)
            .Take(5)
            .ToListAsync();
    }

    private Task<List<ReferenceSource>> GetNewReferenceSourcesAsync()
    {
        return _context.ReferenceSources.AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();
    }

    private Task<List<StudentLearningPath>> GetNewLearningPathsAsync()
    {
        return _context.StudentLearningPaths.AsNoTracking()
            .Include(p => p.Student)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();
    }
}
