using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Onboarding;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class ProfileController : Controller
    {
        private readonly ILearningProfileService _profileService;
        private readonly IStudentProgressService _progressService;
        private readonly ApplicationDbContext _context;

        public ProfileController(
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

        [HttpGet]
        public async Task<IActionResult> EditLearningProfile()
        {
            int userId = GetUserId();
            var profile = await _profileService.GetProfileByUserIdAsync(userId);
            if (profile == null) return RedirectToAction("Index", "Onboarding");

            await PopulateLearningProfileDataAsync(userId);

            var model = new UpdateLearningProfileViewModel
            {
                MainGoalId = profile.MainGoal?.Id,
                CurrentLevelId = profile.CurrentLevel?.Id,
                TargetLevelId = profile.TargetLevel?.Id,
                TargetScore = profile.TargetScore,
                DailyStudyMinutes = profile.DailyStudyMinutes,
                WeeklyStudyDays = profile.WeeklyStudyDays,
                PreferredStudyTime = profile.PreferredStudyTime,
                LearningNote = profile.LearningNote,
                SelectedSkillCodes = profile.PrioritySkills?.Select(s => s.SkillCode).ToList() ?? new System.Collections.Generic.List<string>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLearningProfile(UpdateLearningProfileViewModel model)
        {
            int userId = GetUserId();
            if (!ModelState.IsValid)
            {
                await PopulateLearningProfileDataAsync(userId);
                return View(model);
            }

            var success = await _profileService.EditLearningProfileAsync(userId, model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật hồ sơ. Vui lòng kiểm tra lại thông tin và thử lại.");
                await PopulateLearningProfileDataAsync(userId);
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật hồ sơ học tập thành công!";
            return RedirectToAction("EditLearningProfile");
        }

        private async Task PopulateLearningProfileDataAsync(int userId)
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

            ViewBag.Goals = await _profileService.GetActiveGoalsAsync();
            ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
            ViewBag.Skills = await _profileService.GetActiveSkillsAsync();
        }
    }
}
