using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
<<<<<<< HEAD
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class HomeController : Controller
    {
<<<<<<< HEAD
<<<<<<< HEAD
        public IActionResult Index()
        {
            return View();
        }
=======
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        private readonly ILearningProfileService _profileService;
        private readonly IStudentProgressService _progressService;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILearningProfileService profileService,
            IStudentProgressService progressService,
            ApplicationDbContext context)
        {
            _profileService = profileService;
            _progressService = progressService;
            _context = context;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        public async Task<IActionResult> Index()
        {
            int userId = GetUserId();
            var profile = await _profileService.GetProfileByUserIdAsync(userId);
            if (profile == null) return RedirectToAction("Index", "Onboarding");

            await PopulateDashboardDataAsync(userId);

            return View();
        }

        private async Task PopulateDashboardDataAsync(int userId)
        {
            var profile = await _profileService.GetProfileByUserIdAsync(userId);
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            var dbProfile = await _context.StudentLearningProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            ViewBag.DbProfile = dbProfile;

            ViewBag.User = user;
            ViewBag.UserLearningProfile = profile;

            var learningPath = await _context.StudentLearningPaths
                .FirstOrDefaultAsync(p => p.StudentId == userId && p.Status == "ACTIVE");
            ViewBag.LearningPath = learningPath;

            var latestAttempt = await _context.TestAttempts
                .Include(a => a.PlacementTest)
                .Include(a => a.EstimatedLevel)
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                .FirstOrDefaultAsync();
            ViewBag.LatestAttempt = latestAttempt;

            var latestAnalysis = await _context.CompetencyAnalyses
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();
            ViewBag.LatestAnalysis = latestAnalysis;

            int totalTopics = 0;
            int completedTopics = 0;
            int inProgressTopics = 0;
            int unstartedTopics = 0;

            LearningPathNode? nextNode = null;
            List<string> preferredTopics = new List<string>();

            if (learningPath != null)
            {
                var nodes = await _context.LearningPathNodes
                    .Include(n => n.Topic)
                    .Where(n => n.LearningPathId == learningPath.Id)
                    .ToListAsync();

                totalTopics = nodes.Select(n => n.TopicId).Distinct().Count();
                completedTopics = nodes.Where(n => n.Status == ProgressStatus.Completed).Select(n => n.TopicId).Distinct().Count();
                inProgressTopics = nodes.Where(n => n.Status == ProgressStatus.InProgress).Select(n => n.TopicId).Distinct().Count();
                unstartedTopics = totalTopics - completedTopics - inProgressTopics;

                nextNode = nodes
                    .Where(n => n.Status != ProgressStatus.Completed)
                    .OrderBy(n => n.OrderIndex)
                    .FirstOrDefault();

                preferredTopics = nodes
                    .Where(n => n.Topic != null)
                    .Select(n => n.Topic!.Title)
                    .Distinct()
                    .Take(3)
                    .ToList();
            }
            ViewBag.TotalTopics = totalTopics;
            ViewBag.CompletedTopics = completedTopics;
            ViewBag.InProgressTopics = inProgressTopics;
            ViewBag.UnstartedTopics = unstartedTopics;
            ViewBag.NextNode = nextNode;
            ViewBag.PreferredTopicsList = preferredTopics;

            var progressDashboard = await _progressService.GetDashboardAsync(userId);
            ViewBag.ProgressDashboard = progressDashboard;

            var quizScores = await _context.QuizAttempts
                .Where(a => a.StudentId == userId && a.SubmittedAt.HasValue && a.Score.HasValue)
                .Select(a => a.Score!.Value)
                .ToListAsync();
            decimal avgQuizScore = quizScores.Any() ? quizScores.Average() : 0m;
            ViewBag.AverageQuizScore = avgQuizScore;

            // Generate combined Timeline
            var recentActivities = await _context.StudyActivityLogs
                .Include(log => log.Topic)
                .Include(log => log.LearningPathNode)
                .Where(log => log.StudentId == userId)
                .OrderByDescending(log => log.CreatedAt)
                .Take(5)
                .ToListAsync();

            var placementAttempts = await _context.TestAttempts
                .Include(a => a.PlacementTest)
                .Include(a => a.EstimatedLevel)
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.SubmittedAt ?? a.StartedAt)
                .Take(3)
                .ToListAsync();

            var competencyAnalyses = await _context.CompetencyAnalyses
                .Where(a => a.StudentId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToListAsync();

            var timeline = new List<dynamic>();

            foreach (var log in recentActivities)
            {
                timeline.Add(new {
                    Type = log.ActivityType,
                    Title = log.LearningPathNode?.NodeTitle ?? log.Topic?.Title ?? "Luyện tập học tập",
                    Time = log.CreatedAt,
                    Detail = $"Thời lượng: {log.DurationMinutes ?? 0} phút{(log.Score.HasValue ? $", Điểm số: {log.Score.Value:0.0}" : "")}"
                });
            }

            foreach (var att in placementAttempts)
            {
                timeline.Add(new {
                    Type = "PLACEMENT_TEST",
                    Title = "Làm bài thi Placement Test",
                    Time = att.SubmittedAt ?? att.StartedAt,
                    Detail = $"Điểm số: {att.TotalScore}đ - Trình độ: {att.EstimatedLevel?.Name ?? "Chưa rõ"} - Trạng thái: {att.Status}"
                });
            }

            foreach (var c in competencyAnalyses)
            {
                timeline.Add(new {
                    Type = "AI_RECOMMENDATION",
                    Title = "AI Đánh giá năng lực & Đề xuất",
                    Time = c.CreatedAt,
                    Detail = $"Đề xuất: {c.GapAnalysis}"
                });
            }

            ViewBag.Timeline = timeline.OrderByDescending(t => t.Time).Take(6).ToList();

            // Load student notifications
            var notifications = await _context.Notifications
                .Where(n => n.TargetUserId == null || n.TargetUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();
            ViewBag.NotificationsList = notifications;
        }
<<<<<<< HEAD
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    }
}
