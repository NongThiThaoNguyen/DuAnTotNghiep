using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }

    public async Task<IActionResult> Index()
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

        var user = await _context.Users
            .Include(u => u.UserSetting)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return RedirectToAction("Login", "Account");

        var profile = await _context.StudentLearningProfiles
            .Include(p => p.CurrentLevel)
            .Include(p => p.MainGoal)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        // Fetch Stats
        var studyLogs = await _context.StudyActivityLogs
            .Where(log => log.StudentId == userId)
            .ToListAsync();

        int completedLessons = studyLogs.Count(l => l.ActivityType == "LESSON" || l.ActivityType == "ARTICLE");
        int completedQuizzes = await _context.QuizAttempts
            .CountAsync(a => a.StudentId == userId && a.SubmittedAt.HasValue);

        var quizAttempts = await _context.QuizAttempts
            .Where(a => a.StudentId == userId && a.Score.HasValue)
            .Select(a => a.Score!.Value)
            .ToListAsync();

        decimal avgScore = quizAttempts.Any() ? quizAttempts.Average() : 0m;

        // XP Calculation
        int studyMinutes = studyLogs.Sum(l => l.DurationMinutes ?? 0);
        int totalXp = studyMinutes * 10 + completedLessons * 50 + completedQuizzes * 100;
        int level = (totalXp / 1000) + 1;

        // Rank Tier
        string rankTier = "Bronze";
        if (totalXp >= 3000) rankTier = "Gold";
        else if (totalXp >= 1000) rankTier = "Silver";

        // Progress Percent
        var activePath = await _context.StudentLearningPaths
            .FirstOrDefaultAsync(p => p.StudentId == userId && p.Status == "ACTIVE");

        double progressPercent = 0;
        if (activePath != null)
        {
            var nodes = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == activePath.Id)
                .ToListAsync();
            if (nodes.Any())
            {
                int completed = nodes.Count(n => n.Status == "COMPLETED");
                progressPercent = Math.Round((double)completed / nodes.Count * 100, 1);
            }
        }
        else
        {
            progressPercent = 35.0; // default indicator
        }

        // Today's Lesson and Continue Lesson
        var lessons = await _context.OriginalLessons.Take(2).ToListAsync();
        OriginalLesson? todayLesson = lessons.FirstOrDefault();
        OriginalLesson? continueLesson = lessons.LastOrDefault() ?? todayLesson;

        // Recent Activity Items mapping
        var recentLogs = await _context.StudyActivityLogs
            .Include(log => log.Topic)
            .Where(log => log.StudentId == userId)
            .OrderByDescending(log => log.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentActivities = new List<ActivityItemViewModel>();
        foreach (var log in recentLogs)
        {
            recentActivities.Add(new ActivityItemViewModel
            {
                Type = log.ActivityType ?? "LESSON",
                Title = log.Topic?.Title ?? "Luyện tập bài học",
                Time = log.CreatedAt,
                Detail = $"Thời lượng: {log.DurationMinutes ?? 0} phút{(log.Score.HasValue ? $", Điểm: {log.Score.Value:0.0}" : "")}"
            });
        }

        if (!recentActivities.Any())
        {
            recentActivities.Add(new ActivityItemViewModel
            {
                Type = "SYSTEM",
                Title = "Bắt đầu hành trình học tập",
                Time = DateTime.UtcNow.AddDays(-1),
                Detail = "Chào mừng bạn gia nhập nền tảng học tập AI Learn."
            });
        }

        var vm = new DashboardViewModel
        {
            StudentName = user.FullName,
            AvatarUrl = user.AvatarUrl ?? "/default-images/avatar.png",
            RankTier = rankTier,
            LevelCode = profile?.CurrentLevel?.Code ?? "BEGINNER",
            TargetLevelCode = profile?.MainGoal?.GoalName ?? "IELTS 6.5",
            StreakDays = 4, // Seed placeholder
            AverageQuizScore = avgScore,
            CompletedLessonsCount = completedLessons,
            CompletedQuizzesCount = completedQuizzes,
            TotalXp = totalXp,
            Level = level,
            ProgressPercent = progressPercent,
            TodayLesson = todayLesson,
            ContinueLesson = continueLesson,
            AiRecommendation = "Dựa trên kết quả Quiz gần đây của bạn, chúng tôi đề xuất bạn nên tập trung ôn tập phần <b>Ngữ pháp: Mệnh đề quan hệ</b> để cải thiện độ chuẩn xác khi viết luận.",
            RecentActivities = recentActivities
        };

        return View(vm);
    }
}
