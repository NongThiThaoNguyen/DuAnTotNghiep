using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var viewModel = new AdminDashboardViewModel();

            // 1. General counts
            viewModel.TotalUsers = await _context.Users.CountAsync();
            viewModel.TotalStudents = await _context.Users.CountAsync(u => u.Role.RoleCode == "STUDENT");
            viewModel.TotalTeachers = await _context.Users.CountAsync(u => u.Role.RoleCode == "TEACHER");

            var courseCount = await _context.LearningTopics.CountAsync(t => t.ParentTopicId == null);
            viewModel.TotalCourses = courseCount > 0 ? courseCount : await _context.LearningTopics.CountAsync();

            viewModel.TotalLessons = await _context.OriginalLessons.CountAsync();
            viewModel.TotalQuizzes = await _context.Quizzes.CountAsync();
            viewModel.TotalPlacementTests = await _context.PlacementTests.CountAsync();
            viewModel.TotalLearningPaths = await _context.StudentLearningPaths.CountAsync();
            viewModel.TotalAiRecommendations = await _context.AiFeedbacks.CountAsync();
            viewModel.TotalAiAnalyses = await _context.CompetencyAnalyses.CountAsync();
            viewModel.TotalQuestionBank = await _context.QuestionBanks.CountAsync();
            viewModel.TotalReferenceSources = await _context.ReferenceSources.CountAsync();

            // 2. Users over time (last 30 days)
            var usersTimeData = await _context.Users
                .GroupBy(u => u.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .Take(30)
                .ToListAsync();

            viewModel.UsersOverTime = usersTimeData.Select(x => new ChartItem
            {
                Label = x.Date.ToString("dd/MM"),
                Value = x.Count
            }).ToList();

            // 3. Placement test attempts over time (last 30 days)
            var attemptsTimeData = await _context.TestAttempts
                .GroupBy(a => a.StartedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .Take(30)
                .ToListAsync();

            viewModel.PlacementTestsOverTime = attemptsTimeData.Select(x => new ChartItem
            {
                Label = x.Date.ToString("dd/MM"),
                Value = x.Count
            }).ToList();

            // 4. AI Analysis: recommended level distribution
            var aiLevels = await _context.CompetencyAnalyses
                .Include(c => c.RecommendedLevel)
                .Where(c => c.RecommendedLevel != null)
                .GroupBy(c => c.RecommendedLevel!.Name)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            viewModel.AiAnalysisData = aiLevels.Select(x => new ChartItem
            {
                Label = x.Level,
                Value = x.Count
            }).ToList();

            // 5. Learning Progress: average progress percent by skill
            var skillProgress = await _context.StudentProgressSnapshots
                .Include(s => s.Skill)
                .Where(s => s.Skill != null)
                .GroupBy(s => s.Skill!.SkillName)
                .Select(g => new { Skill = g.Key, AvgProgress = g.Average(s => (double)s.ProgressPercent) })
                .ToListAsync();

            viewModel.LearningProgressData = skillProgress.Select(x => new ChartItem
            {
                Label = x.Skill,
                Value = Math.Round(x.AvgProgress, 1)
            }).ToList();

            // 6. Role Distribution
            var rolesData = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null)
                .GroupBy(u => u.Role!.RoleName)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            viewModel.RoleDistribution = rolesData.Select(x => new ChartItem
            {
                Label = x.Role,
                Value = x.Count
            }).ToList();

            // 7. Level Distribution
            var levelsData = await _context.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .Where(p => p.CurrentLevel != null)
                .GroupBy(p => p.CurrentLevel!.Name)
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToListAsync();

            viewModel.LevelDistribution = levelsData.Select(x => new ChartItem
            {
                Label = x.Level,
                Value = x.Count
            }).ToList();

            // 8. Recent Items Lists
            viewModel.RecentActivities = await _context.StudyActivityLogs
                .Include(a => a.Student)
                .Include(a => a.Topic)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new RecentActivityViewModel
                {
                    Id = a.Id,
                    UserName = a.Student != null ? a.Student.FullName : "System",
                    ActivityType = a.ActivityType,
                    Description = a.Topic != null ? a.Topic.Title : "N/A",
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            viewModel.RecentPlacementTests = await _context.TestAttempts
                .Include(a => a.Student)
                .Include(a => a.PlacementTest)
                .OrderByDescending(a => a.StartedAt)
                .Take(5)
                .ToListAsync();

            viewModel.RecentAiRecommendations = await _context.AiFeedbacks
                .Include(f => f.Student)
                .OrderByDescending(f => f.CreatedAt)
                .Take(5)
                .ToListAsync();

            viewModel.NewQuestionBankItems = await _context.QuestionBanks
                .Include(q => q.Skill)
                .Include(q => q.Level)
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .ToListAsync();

            viewModel.NewReferenceSources = await _context.ReferenceSources
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            viewModel.NewLearningPaths = await _context.StudentLearningPaths
                .Include(p => p.Student)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            return viewModel;
        }

        public async Task<AdminDashboardViewModel> GetAdminOverviewAsync()
        {
            var viewModel = new AdminDashboardViewModel();
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            viewModel.TotalUsers = await _context.Users.AsNoTracking().CountAsync();
            viewModel.TotalStudents = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "STUDENT");
            viewModel.TotalTeachers = await _context.Users.AsNoTracking().CountAsync(u => u.Role.RoleCode == "TEACHER");

            viewModel.ActiveUsersToday = await _context.Users.AsNoTracking()
                .CountAsync(u => u.LastLoginAt != null && u.LastLoginAt.Value.Date == today);

            viewModel.TotalTopics = await _context.LearningTopics.AsNoTracking()
                .CountAsync(t => t.Status == "PUBLISHED" || t.Status == "ACTIVE");

            viewModel.TotalQuizzes = await _context.Quizzes.AsNoTracking().CountAsync();

            viewModel.TotalLessons = await _context.OriginalLessons.AsNoTracking().CountAsync();

            viewModel.PendingAiContents = await _context.AiGeneratedContents.AsNoTracking()
                .CountAsync(a => a.ReviewStatus == "PENDING");

            viewModel.TotalPlacementAttempts = await _context.TestAttempts.AsNoTracking().CountAsync();

            var attempts = await _context.TestAttempts.AsNoTracking()
                .Where(a => a.TotalScore != null)
                .Select(a => (double)a.TotalScore)
                .ToListAsync();
            viewModel.AveragePlacementScore = attempts.Any() ? Math.Round(attempts.Average(), 2) : 0;

            viewModel.NewUsersThisWeek = await _context.Users.AsNoTracking()
                .CountAsync(u => u.CreatedAt >= weekAgo);

            viewModel.NewUsersThisMonth = await _context.Users.AsNoTracking()
                .CountAsync(u => u.CreatedAt >= monthAgo);

            viewModel.RecentUsers = await _context.Users.AsNoTracking()
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentUserViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = u.Role != null ? u.Role.RoleName : "Unknown",
                    CreatedAt = u.CreatedAt,
                    AvatarUrl = u.AvatarUrl
                })
                .ToListAsync();

            viewModel.RecentActivities = await _context.AuditLogs.AsNoTracking()
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new RecentActivityViewModel
                {
                    Id = a.Id,
                    UserName = a.User != null ? a.User.FullName : "System",
                    ActivityType = a.Action,
                    Description = $"{a.Action} on {a.EntityName}",
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return viewModel;
        }
    }
}
