using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.ViewModels.Student;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPathViewService _pathViewService;

        public StudentDashboardService(ApplicationDbContext context, IPathViewService pathViewService)
        {
            _context = context;
            _pathViewService = pathViewService;
        }

        public async Task<StudentDashboardViewModel> GetDashboardAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            var studentName = user?.FullName ?? "Học viên";
            var avatarUrl = user?.AvatarUrl ?? "/default-images/avatar.png";

            // 1. Fetch active learning path and nodes
            var path = await _context.StudentLearningPaths
                .Include(p => p.LearningPathNodes)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == userId && p.Status == "ACTIVE");

            double progressPercent = 0;
            int completedLessons = 0;
            int completedQuizzes = 0;
            NextTaskViewModel? nextTask = null;

            if (path != null)
            {
                var nodes = path.LearningPathNodes.ToList();
                int totalNodes = nodes.Count;
                int completedNodes = nodes.Count(n => n.Status == ProgressStatus.Completed);
                
                if (totalNodes > 0)
                {
                    progressPercent = Math.Round((double)completedNodes * 100.0 / totalNodes, 2);
                }

                completedLessons = nodes.Count(n => n.NodeType == "LESSON" && n.Status == ProgressStatus.Completed);
                completedQuizzes = nodes.Count(n => n.NodeType == "QUIZ" && n.Status == ProgressStatus.Completed);

                // Find next task (InProgress first, then Available)
                var nextNode = nodes
                    .Where(n => n.Status == ProgressStatus.InProgress || n.Status == ProgressStatus.Available)
                    .OrderBy(n => n.Status == ProgressStatus.InProgress ? 0 : 1)
                    .ThenBy(n => n.OrderIndex)
                    .FirstOrDefault();

                if (nextNode != null)
                {
                    nextTask = new NextTaskViewModel
                    {
                        NodeId = nextNode.Id,
                        Title = nextNode.NodeTitle,
                        NodeType = nextNode.NodeType,
                        EstimatedMinutes = nextNode.EstimatedMinutes ?? 15,
                        TargetUrl = await _pathViewService.BuildNodeTargetUrlAsync(nextNode)
                    };
                }
            }

            // 2. Fetch study activity logs
            var logs = await _context.StudyActivityLogs
                .Where(a => a.StudentId == userId)
                .AsNoTracking()
                .ToListAsync();

            // Calculate Streak
            int streakDays = CalculateStreak(logs);

            // Calculate XP
            int totalStudyMinutes = logs.Sum(l => l.DurationMinutes ?? 0);
            int totalXp = totalStudyMinutes * 10 + completedLessons * 50 + completedQuizzes * 100;
            int level = totalXp / 1000 + 1;

            string rankTier = "Bronze";
            if (totalXp >= 3000) rankTier = "Gold";
            else if (totalXp >= 1000) rankTier = "Silver";

            // Calculate Average Quiz Score
            var quizScores = logs
                .Where(l => l.ActivityType == "QUIZ" && l.Score.HasValue)
                .Select(l => l.Score!.Value)
                .ToList();

            decimal averageQuizScore = quizScores.Any() ? quizScores.Average() : 0;
            averageQuizScore = Math.Round(averageQuizScore, 2);

            // Calculate Study Minutes This Week (Monday to Sunday)
            var today = DateTime.UtcNow.Date;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var mondayUtc = today.AddDays(-1 * diff);
            int studyMinutesThisWeek = logs
                .Where(l => l.CreatedAt >= mondayUtc)
                .Sum(l => l.DurationMinutes ?? 0);

            // 3. Fetch profile information
            var profile = await _context.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .Include(p => p.TargetLevel)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var currentLevel = profile?.CurrentLevel?.Name ?? profile?.CurrentLevel?.Code ?? "Chưa xác định";
            var targetLevel = profile?.TargetLevel?.Name ?? profile?.TargetLevel?.Code ?? "Chưa xác định";

            // 4. Fetch AI Recommendation
            var latestAnalysis = await _context.CompetencyAnalyses
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var aiRecommendation = latestAnalysis?.Summary ?? latestAnalysis?.GapAnalysis ?? "Hoàn thành các bài test năng lực để nhận nhận xét từ AI Tutor.";

            // 5. Recent Activities (Top 5)
            var recentActivities = logs
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .Select(l => new ActivityItemViewModel
                {
                    ActivityType = l.ActivityType,
                    ActivityTypeLabel = GetActivityLabel(l.ActivityType),
                    Title = GetActivityTitle(l),
                    CreatedAt = l.CreatedAt,
                    DurationMinutes = l.DurationMinutes,
                    Score = l.Score,
                    IconClass = GetActivityIcon(l.ActivityType)
                })
                .ToList();

            return new StudentDashboardViewModel
            {
                StudentName = studentName,
                AvatarUrl = avatarUrl,
                StreakDays = streakDays,
                TotalXp = totalXp,
                Level = level,
                RankTier = rankTier,
                CompletedLessons = completedLessons,
                CompletedQuizzes = completedQuizzes,
                AverageQuizScore = averageQuizScore,
                ProgressPercent = progressPercent,
                StudyMinutesThisWeek = studyMinutesThisWeek,
                CurrentLevel = currentLevel,
                TargetLevel = targetLevel,
                RecentActivities = recentActivities,
                NextTask = nextTask,
                AiRecommendation = aiRecommendation
            };
        }

        private static int CalculateStreak(List<StudyActivityLog> logs)
        {
            var studyDates = logs
                .Where(l => l.ActivityType != "LOGIN")
                .Select(l => DateOnly.FromDateTime(l.CreatedAt.Date))
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (studyDates.Count == 0)
            {
                return 0;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var yesterday = today.AddDays(-1);
            if (studyDates[0] != today && studyDates[0] != yesterday)
            {
                return 0;
            }

            int streak = 1;
            for (int i = 0; i < studyDates.Count - 1; i++)
            {
                if (studyDates[i].AddDays(-1) == studyDates[i + 1])
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private static string GetActivityLabel(string activityType)
        {
            return activityType.ToUpperInvariant() switch
            {
                "LEARN" => "Bài học",
                "QUIZ" => "Quiz kiểm tra",
                "CHAT" => "AI Tutor",
                "PRACTICE" => "Luyện viết/nói",
                "REVIEW" => "Ôn tập",
                "LOGIN" => "Đăng nhập",
                "TASK_SKIPPED" => "Bỏ qua nhiệm vụ",
                _ => activityType
            };
        }

        private static string GetActivityTitle(StudyActivityLog log)
        {
            return log.ActivityType.ToUpperInvariant() switch
            {
                "LEARN" => "Đã hoàn thành bài học",
                "QUIZ" => $"Đã nộp bài Quiz" + (log.Score.HasValue ? $" (Điểm: {log.Score.Value})" : ""),
                "CHAT" => "Đã thảo luận với AI Tutor",
                "PRACTICE" => "Đã thực hiện bài thực hành",
                "REVIEW" => "Đã ôn tập kiến thức",
                "LOGIN" => "Đăng nhập hệ thống",
                "TASK_SKIPPED" => "Đã bỏ qua một nhiệm vụ",
                _ => "Hoạt động học tập"
            };
        }

        private static string GetActivityIcon(string activityType)
        {
            return activityType.ToUpperInvariant() switch
            {
                "LEARN" => "fa-solid fa-book text-blue-500",
                "QUIZ" => "fa-solid fa-circle-question text-emerald-500",
                "CHAT" => "fa-solid fa-robot text-violet-500",
                "PRACTICE" => "fa-solid fa-pen-to-square text-amber-500",
                "REVIEW" => "fa-solid fa-rotate-right text-indigo-500",
                "LOGIN" => "fa-solid fa-right-to-bracket text-slate-500",
                "TASK_SKIPPED" => "fa-solid fa-eye-slash text-rose-500",
                _ => "fa-solid fa-circle text-slate-400"
            };
        }
    }
}
