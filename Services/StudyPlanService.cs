using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.StudyPlan;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Helpers;

namespace DuAnTotNghiep.Services
{
    public class StudyPlanService : IStudyPlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPathViewService _pathViewService;
        private readonly IProgressTrackingService _progressTrackingService;
        private readonly ILogger<StudyPlanService> _logger;

        public StudyPlanService(
            ApplicationDbContext context,
            IPathViewService pathViewService,
            IProgressTrackingService progressTrackingService,
            ILogger<StudyPlanService> logger)
        {
            _context = context;
            _pathViewService = pathViewService;
            _progressTrackingService = progressTrackingService;
            _logger = logger;
        }

        public async Task<TodayStudyPlanViewModel> GetTodayPlanAsync(int studentId)
        {
            var path = await _context.StudentLearningPaths
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.Topic)
                        .ThenInclude(t => t.Skill)
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.Lesson)
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.Quiz)
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.PracticeTask)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "ACTIVE");

            if (path == null)
            {
                return new TodayStudyPlanViewModel { HasActivePath = false };
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            var orderedNodes = path.LearningPathNodes
                .OrderBy(n => n.OrderIndex)
                .ThenBy(n => n.Id)
                .ToList();

            var todayTasks = new List<DailyStudyTaskViewModel>();
            var overdueTasks = new List<DailyStudyTaskViewModel>();

            foreach (var node in orderedNodes)
            {
                var isOverdue = StudyPlanHelper.IsOverdue(node.ScheduledDate, node.Status);
                var isToday = node.ScheduledDate.HasValue && node.ScheduledDate.Value == today;

                if (isToday)
                {
                    todayTasks.Add(await MapToViewModelAsync(node, today));
                }
                else if (isOverdue && node.Status != "LOCKED")
                {
                    overdueTasks.Add(await MapToViewModelAsync(node, today));
                }
            }

            // Keep only top 10 overdue tasks, sorted by scheduled date asc
            overdueTasks = overdueTasks.OrderBy(t => t.ScheduledDate).Take(10).ToList();

            var continueTaskNode = orderedNodes.FirstOrDefault(n => n.Status == "IN_PROGRESS");
            var continueTask = continueTaskNode != null ? await MapToViewModelAsync(continueTaskNode, today) : null;

            var nextTaskNode = orderedNodes.FirstOrDefault(n => n.Status == "AVAILABLE" && (!n.ScheduledDate.HasValue || n.ScheduledDate.Value >= today));
            var nextTask = nextTaskNode != null ? await MapToViewModelAsync(nextTaskNode, today) : null;

            var profile = await _context.StudentLearningProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == studentId);

            int dailyLimit = profile?.DailyStudyMinutes ?? 60;
            int totalMinutesToday = todayTasks.Sum(t => t.EstimatedMinutes);

            return new TodayStudyPlanViewModel
            {
                PathTitle = path.Title,
                TotalMinutesToday = totalMinutesToday,
                DailyLimitMinutes = dailyLimit,
                TodayTasks = todayTasks,
                OverdueTasks = overdueTasks,
                ContinueTask = continueTask,
                NextTask = nextTask,
                HasActivePath = true
            };
        }

        public async Task<WeeklyStudyPlanViewModel> GetWeeklyPlanAsync(int studentId, DateOnly? weekStart = null)
        {
            var path = await _context.StudentLearningPaths
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "ACTIVE");

            if (path == null)
            {
                return new WeeklyStudyPlanViewModel { HasActivePath = false };
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (!weekStart.HasValue)
            {
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                weekStart = today.AddDays(-1 * diff);
            }

            var weekEnd = weekStart.Value.AddDays(6);

            var nodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                    .ThenInclude(t => t.Skill)
                .Include(n => n.Lesson)
                .Include(n => n.Quiz)
                .Include(n => n.PracticeTask)
                .Where(n => n.LearningPathId == path.Id && n.ScheduledDate >= weekStart && n.ScheduledDate <= weekEnd)
                .AsNoTracking()
                .ToListAsync();

            var dayGroups = new List<DayGroupViewModel>();
            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.Value.AddDays(i);
                var label = StudyPlanHelper.GetViDayName(date.DayOfWeek);
                dayGroups.Add(new DayGroupViewModel
                {
                    Date = date,
                    DayLabel = $"{label} ({date:dd/MM})",
                    IsToday = (date == today),
                    Tasks = new List<DailyStudyTaskViewModel>()
                });
            }

            foreach (var node in nodes)
            {
                if (!node.ScheduledDate.HasValue) continue;
                var group = dayGroups.FirstOrDefault(g => g.Date == node.ScheduledDate.Value);
                if (group != null)
                {
                    group.Tasks.Add(await MapToViewModelAsync(node, today));
                }
            }

            int totalTasks = dayGroups.Sum(g => g.Tasks.Count);
            int completedTasks = dayGroups.Sum(g => g.Tasks.Count(t => t.Status == "COMPLETED"));
            int overdueTasksCount = dayGroups.Sum(g => g.Tasks.Count(t => t.IsOverdue));
            int totalMinutes = dayGroups.Sum(g => g.TotalMinutes);

            return new WeeklyStudyPlanViewModel
            {
                PathTitle = path.Title,
                WeekStart = weekStart.Value,
                WeekEnd = weekEnd,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                OverdueTasks = overdueTasksCount,
                TotalMinutes = totalMinutes,
                DayGroups = dayGroups,
                HasActivePath = true
            };
        }

        public async Task<MonthlyStudyPlanViewModel> GetMonthlyPlanAsync(int studentId, int? year = null, int? month = null)
        {
            var path = await _context.StudentLearningPaths
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "ACTIVE");

            if (path == null)
            {
                return new MonthlyStudyPlanViewModel { HasActivePath = false };
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            int y = year ?? today.Year;
            int m = month ?? today.Month;

            var nodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                    .ThenInclude(t => t.Skill)
                .Include(n => n.Lesson)
                .Include(n => n.Quiz)
                .Include(n => n.PracticeTask)
                .Where(n => n.LearningPathId == path.Id && n.ScheduledDate.HasValue && n.ScheduledDate.Value.Year == y && n.ScheduledDate.Value.Month == m)
                .AsNoTracking()
                .ToListAsync();

            var firstDay = new DateOnly(y, m, 1);
            var lastDay = new DateOnly(y, m, DateTime.DaysInMonth(y, m));

            // Generate weekly buckets covering the month
            int firstDiff = (7 + (firstDay.DayOfWeek - DayOfWeek.Monday)) % 7;
            var currentMonday = firstDay.AddDays(-1 * firstDiff);

            var weekSummaries = new List<WeekSummaryViewModel>();
            int weekNum = 1;
            while (currentMonday <= lastDay)
            {
                var currentSunday = currentMonday.AddDays(6);
                weekSummaries.Add(new WeekSummaryViewModel
                {
                    WeekNumber = weekNum++,
                    WeekStart = currentMonday,
                    WeekEnd = currentSunday,
                    Tasks = new List<DailyStudyTaskViewModel>()
                });
                currentMonday = currentMonday.AddDays(7);
            }

            foreach (var node in nodes)
            {
                var date = node.ScheduledDate!.Value;
                var week = weekSummaries.FirstOrDefault(w => date >= w.WeekStart && date <= w.WeekEnd);
                if (week != null)
                {
                    week.Tasks.Add(await MapToViewModelAsync(node, today));
                    week.TaskCount++;
                    if (node.Status == "COMPLETED")
                    {
                        week.CompletedCount++;
                    }
                    week.TotalMinutes += node.EstimatedMinutes ?? StudyPlanHelper.GetDefaultMinutes(node.NodeType);
                }
            }

            int totalTasks = weekSummaries.Sum(w => w.TaskCount);
            int completedTasks = weekSummaries.Sum(w => w.CompletedCount);
            int totalMinutes = weekSummaries.Sum(w => w.TotalMinutes);

            string monthLabel = $"Tháng {m}/{y}";

            return new MonthlyStudyPlanViewModel
            {
                PathTitle = path.Title,
                Year = y,
                Month = m,
                MonthLabel = monthLabel,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                TotalMinutes = totalMinutes,
                WeekSummaries = weekSummaries,
                HasActivePath = true
            };
        }

        public async Task<bool> MarkTaskSkippedAsync(int nodeId, int studentId, string? reason = null)
        {
            var node = await _context.LearningPathNodes
                .Include(n => n.LearningPath)
                .FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null || node.LearningPath.StudentId != studentId)
            {
                return false;
            }

            if (node.Status == "LOCKED" || node.Status == "COMPLETED")
            {
                return false;
            }

            node.Status = "SKIPPED";
            node.SkippedReason = reason;

            var log = new StudyActivityLog
            {
                StudentId = studentId,
                ActivityType = "TASK_SKIPPED",
                TopicId = node.TopicId,
                LearningPathNodeId = node.Id,
                DurationMinutes = 0,
                Metadata = reason,
                CreatedAt = DateTime.UtcNow
            };

            await _context.StudyActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student {StudentId} skipped learning path node {NodeId} with reason {Reason}", studentId, nodeId, reason);

            // Trigger progress recalculation
            try
            {
                await _progressTrackingService.RecalculateStudentProgress(studentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to recalculate progress for student {StudentId} after skipping node {NodeId}", studentId, nodeId);
            }

            return true;
        }

        public async Task<List<DailyStudyTaskViewModel>> GetOverdueTasksAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var path = await _context.StudentLearningPaths
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "ACTIVE");

            if (path == null)
            {
                return new List<DailyStudyTaskViewModel>();
            }

            var nodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                    .ThenInclude(t => t.Skill)
                .Include(n => n.Lesson)
                .Include(n => n.Quiz)
                .Include(n => n.PracticeTask)
                .Where(n => n.LearningPathId == path.Id && n.ScheduledDate.HasValue && n.ScheduledDate.Value < today && n.Status != "COMPLETED" && n.Status != "SKIPPED" && n.Status != "LOCKED")
                .OrderBy(n => n.ScheduledDate)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();

            var list = new List<DailyStudyTaskViewModel>();
            foreach (var node in nodes)
            {
                list.Add(await MapToViewModelAsync(node, today));
            }
            return list;
        }

        public async Task<int> CalculateDailyLoadAsync(int studentId, DateOnly? date = null)
        {
            var d = date ?? DateOnly.FromDateTime(DateTime.Today);
            var path = await _context.StudentLearningPaths
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == "ACTIVE");

            if (path == null)
            {
                return 0;
            }

            var nodes = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == path.Id && n.ScheduledDate == d)
                .AsNoTracking()
                .ToListAsync();

            return nodes.Sum(n => n.EstimatedMinutes ?? StudyPlanHelper.GetDefaultMinutes(n.NodeType));
        }

        private async Task<DailyStudyTaskViewModel> MapToViewModelAsync(LearningPathNode node, DateOnly today)
        {
            var isOverdue = StudyPlanHelper.IsOverdue(node.ScheduledDate, node.Status);
            var isToday = StudyPlanHelper.IsTodayTask(node.ScheduledDate, node.Status);

            // Validate integrity
            bool hasConfigError = false;
            if (node.NodeType == "LESSON" && node.LessonId == null) hasConfigError = true;
            else if (node.NodeType == "QUIZ" && node.QuizId == null) hasConfigError = true;
            else if (node.NodeType == "PRACTICE" && node.PracticeTaskId == null) hasConfigError = true;

            var targetUrl = await _pathViewService.BuildNodeTargetUrlAsync(node);

            return new DailyStudyTaskViewModel
            {
                NodeId = node.Id,
                Title = node.NodeTitle ?? "",
                SkillName = node.Topic?.Skill?.SkillName ?? node.Topic?.Title,
                NodeType = node.NodeType,
                Status = node.Status,
                StatusLabel = StudyPlanHelper.GetStatusLabel(node.Status),
                StatusCssClass = StudyPlanHelper.GetStatusCssClass(node.Status, isOverdue),
                EstimatedMinutes = node.EstimatedMinutes ?? StudyPlanHelper.GetDefaultMinutes(node.NodeType),
                TargetUrl = targetUrl,
                AiReason = node.AiReason,
                ScheduledDate = node.ScheduledDate,
                IsOverdue = isOverdue,
                IsTodayTask = isToday,
                HasConfigError = hasConfigError,
                RescheduledFrom = node.RescheduledFrom
            };
        }
    }
}
