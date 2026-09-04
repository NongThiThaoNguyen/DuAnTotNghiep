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
        private readonly IGoogleMeetService _meetService;

        public StudentDashboardService(
            ApplicationDbContext context,
            IPathViewService pathViewService,
            IGoogleMeetService meetService)
        {
            _context = context;
            _pathViewService = pathViewService;
            _meetService = meetService;
        }

        public async Task<StudentDashboardViewModel> GetDashboardAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            var studentName = user?.FullName ?? "Học viên";
            var avatarUrl = NormalizeAvatarUrl(user?.AvatarUrl);

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

            bool hasQuizAttempts = quizScores.Any();
            decimal averageQuizScore = hasQuizAttempts ? quizScores.Average() : 0;
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

            // 6. Fetch Enrolled Courses
            var enrolledCourses = new List<EnrolledCourseViewModel>();
            if (path != null && path.LearningPathNodes != null)
            {
                var topicIds = path.LearningPathNodes
                    .Where(n => n.TopicId.HasValue)
                    .Select(n => n.TopicId!.Value)
                    .Distinct()
                    .ToList();

                if (topicIds.Any())
                {
                    var topics = await _context.LearningTopics
                        .Include(t => t.Skill)
                        .Include(t => t.OriginalLessons)
                        .Where(t => topicIds.Contains(t.Id))
                        .AsNoTracking()
                        .ToListAsync();

                    foreach (var t in topics)
                    {
                        var topicNodes = path.LearningPathNodes.Where(n => n.TopicId == t.Id).ToList();
                        int totalNodes = topicNodes.Count > 0 ? topicNodes.Count : t.OriginalLessons.Count;
                        int completedNodesCount = topicNodes.Count(n => n.Status == ProgressStatus.Completed);
                        double topicProgress = totalNodes > 0 ? Math.Round((double)completedNodesCount / totalNodes * 100, 1) : 0;

                        enrolledCourses.Add(new EnrolledCourseViewModel
                        {
                            Id = t.Id,
                            Title = t.Title,
                            SkillName = t.Skill?.SkillName ?? "Tổng quát",
                            LessonCount = t.OriginalLessons.Count > 0 ? t.OriginalLessons.Count : totalNodes,
                            ProgressPercent = topicProgress,
                            TargetUrl = $"/Courses/Detail/{t.Id}"
                        });
                    }
                }
            }

            if (!enrolledCourses.Any())
            {
                var studiedTopicIds = logs
                    .Where(l => l.TopicId.HasValue)
                    .Select(l => l.TopicId!.Value)
                    .Distinct()
                    .Take(4)
                    .ToList();

                if (studiedTopicIds.Any())
                {
                    var topics = await _context.LearningTopics
                        .Include(t => t.Skill)
                        .Include(t => t.OriginalLessons)
                        .Where(t => studiedTopicIds.Contains(t.Id))
                        .AsNoTracking()
                        .ToListAsync();

                    foreach (var t in topics)
                    {
                        enrolledCourses.Add(new EnrolledCourseViewModel
                        {
                            Id = t.Id,
                            Title = t.Title,
                            SkillName = t.Skill?.SkillName ?? "Tổng quát",
                            LessonCount = t.OriginalLessons.Count,
                            ProgressPercent = 0,
                            TargetUrl = $"/Courses/Detail/{t.Id}"
                        });
                    }
                }
            }

            // 7. Fetch Todo Items
            var todoItems = new List<StudentTodoItemViewModel>();
            if (path != null && path.LearningPathNodes != null)
            {
                var pendingNodes = path.LearningPathNodes
                    .Where(n => n.Status != ProgressStatus.Completed)
                    .OrderBy(n => n.Status == ProgressStatus.InProgress ? 0 : 1)
                    .ThenBy(n => n.OrderIndex)
                    .Take(5)
                    .ToList();

                foreach (var node in pendingNodes)
                {
                    todoItems.Add(new StudentTodoItemViewModel
                    {
                        Id = node.Id,
                        Title = node.NodeTitle,
                        Type = node.NodeType,
                        DueDate = node.ScheduledDate.HasValue ? node.ScheduledDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                        IsOverdue = node.ScheduledDate.HasValue && node.ScheduledDate.Value < DateOnly.FromDateTime(DateTime.Now),
                        TargetUrl = await _pathViewService.BuildNodeTargetUrlAsync(node)
                    });
                }
            }

            // 8. Fetch Upcoming Schedule
            UpcomingScheduleViewModel? upcomingSchedule = null;
            var now = DateTime.Now;
            var activeClassroomId = await _context.Enrollments
                .Where(e => e.StudentId == userId && e.Status == "ACTIVE")
                .Select(e => (int?)e.ClassroomId)
                .FirstOrDefaultAsync();
            var nextSchedule = await _context.Schedules
                .Include(s => s.Teacher)
                .Include(s => s.Topic)
                .Where(s => activeClassroomId.HasValue
                    && s.ClassroomId == activeClassroomId.Value
                    && (s.StartTime >= now || s.EndTime >= now))
                .OrderBy(s => s.StartTime)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (nextSchedule != null)
            {
                var meetUrl = nextSchedule.MeetUrl;
                if (string.IsNullOrWhiteSpace(meetUrl))
                {
                    meetUrl = _meetService.GenerateMeetUrl(nextSchedule.Id, nextSchedule.Title);
                }

                upcomingSchedule = new UpcomingScheduleViewModel
                {
                    Id = nextSchedule.Id,
                    Title = nextSchedule.Title,
                    StartTime = nextSchedule.StartTime,
                    EndTime = nextSchedule.EndTime,
                    Classroom = nextSchedule.Classroom,
                    MeetUrl = meetUrl,
                    TeacherName = nextSchedule.Teacher?.FullName ?? "Giảng viên",
                    TopicTitle = nextSchedule.Topic?.Title
                };
            }

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
                HasQuizAttempts = hasQuizAttempts,
                AverageQuizScore = averageQuizScore,
                ProgressPercent = progressPercent,
                StudyMinutesThisWeek = studyMinutesThisWeek,
                CurrentLevel = currentLevel,
                TargetLevel = targetLevel,
                RecentActivities = recentActivities,
                NextTask = nextTask,
                AiRecommendation = aiRecommendation,
                EnrolledCourses = enrolledCourses,
                TodoItems = todoItems,
                UpcomingSchedule = upcomingSchedule
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

        private static string NormalizeAvatarUrl(string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl)
                || avatarUrl.Contains("/default-images/avatar.png", StringComparison.OrdinalIgnoreCase)
                || avatarUrl.Contains("/images/default-avatar.png", StringComparison.OrdinalIgnoreCase))
            {
                return "/images/default-avatar.svg";
            }

            return avatarUrl;
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
