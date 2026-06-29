using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.LearningPath;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class PathViewService : IPathViewService
    {
        private static readonly HashSet<string> OpenableStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            ProgressStatus.Available,
            ProgressStatus.InProgress,
            ProgressStatus.Completed,
            ProgressStatus.NeedReview,
            ProgressStatus.Skipped
        };

        private readonly ApplicationDbContext _context;
        private readonly ILogger<PathViewService> _logger;

        public PathViewService(ApplicationDbContext context, ILogger<PathViewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LearningPathPageViewModel> GetCurrentPathPageAsync(int userId)
        {
            var path = await _context.StudentLearningPaths
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.Topic)
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.Lesson)
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.Quiz)
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.PracticeTask)
                .AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.StudentId == userId && p.Status == "ACTIVE");

            if (path == null)
            {
                return new LearningPathPageViewModel
                {
                    HasPath = false,
                    PathTitle = "Chua co lo trinh hoc",
                    PathStatus = "NONE"
                };
            }

            var orderedNodes = path.LearningPathNodes
                .OrderBy(n => n.OrderIndex)
                .ThenBy(n => n.Id)
                .ToList();

            var nodeViewModels = new List<PathNodeViewModel>();
            foreach (var node in orderedNodes)
            {
                nodeViewModels.Add(await MapNodeAsync(node));
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayTasks = nodeViewModels
                .Where(n => n.ScheduledDate.HasValue &&
                            n.ScheduledDate.Value <= today &&
                            IsOpenableStatus(n.Status) &&
                            !n.Status.Equals(ProgressStatus.Completed, StringComparison.OrdinalIgnoreCase) &&
                            !n.Status.Equals(ProgressStatus.Skipped, StringComparison.OrdinalIgnoreCase))
                .Select(n => new TodayTaskViewModel
                {
                    NodeId = n.NodeId,
                    Title = n.Title,
                    NodeType = n.NodeType,
                    AiReason = n.AiReason,
                    EstimatedMinutes = n.EstimatedMinutes,
                    TargetUrl = n.TargetUrl,
                    IsOverdue = n.ScheduledDate < today
                })
                .ToList();

            return new LearningPathPageViewModel
            {
                PathId = path.Id,
                PathTitle = path.Title,
                PathDescription = path.Description,
                PathStatus = path.Status,
                StartDate = path.StartDate,
                TargetEndDate = path.TargetEndDate,
                GeneratedByAi = path.GeneratedByAi,
                AiPlanSummary = path.AiPlanSummary,
                HasPath = true,
                Nodes = nodeViewModels,
                TodayTasks = todayTasks,
                Progress = await BuildProgressSummaryAsync(userId, orderedNodes)
            };
        }

        public async Task<bool> EnsurePathOwnerAsync(int pathId, int userId)
        {
            return await _context.StudentLearningPaths
                .AnyAsync(p => p.Id == pathId && p.StudentId == userId);
        }

        public Task<string?> BuildNodeTargetUrlAsync(LearningPathNode node)
        {
            var nodeType = node.NodeType?.ToUpperInvariant();
            string? targetUrl = nodeType switch
            {
                NodeType.Topic when node.TopicId.HasValue => $"/Student/Topics/Details/{node.TopicId.Value}",
                NodeType.Lesson when node.LessonId.HasValue => $"/Student/Lesson/Details/{node.LessonId.Value}",
                NodeType.Quiz when node.QuizId.HasValue => $"/Student/Quiz/Details/{node.QuizId.Value}",
                NodeType.Practice when node.PracticeTaskId.HasValue => $"/Student/Practice/Details/{node.PracticeTaskId.Value}",
                NodeType.Review when node.TopicId.HasValue => $"/Student/Progress/TopicDetail/{node.TopicId.Value}",
                NodeType.AiTutor => $"/Student/AiTutor/Node/{node.Id}",
                _ => null
            };

            return Task.FromResult(targetUrl);
        }

        public async Task<bool> CanOpenNodeAsync(int nodeId, int userId)
        {
            var node = await _context.LearningPathNodes
                .Include(n => n.LearningPath)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null || node.LearningPath.StudentId != userId)
            {
                return false;
            }

            return IsOpenableStatus(node.Status);
        }

        public async Task<bool> TryUnlockNextNodesAsync(int completedNodeId, int userId)
        {
            var node = await _context.LearningPathNodes
                .Include(n => n.LearningPath)
                .FirstOrDefaultAsync(n => n.Id == completedNodeId);

            if (node == null || node.LearningPath.StudentId != userId)
            {
                return false;
            }

            var unlocked = await UnlockNextNodeAsync(node);
            await _context.SaveChangesAsync();
            return unlocked;
        }

        public async Task<bool> MarkNodeCompletedAsync(
            int nodeId,
            int userId,
            string? activityType = null,
            int? durationMinutes = null,
            decimal? score = null,
            string? metadata = null)
        {
            var node = await _context.LearningPathNodes
                .Include(n => n.LearningPath)
                .FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null || node.LearningPath.StudentId != userId)
            {
                return false;
            }

            if (!node.Status.Equals(ProgressStatus.Completed, StringComparison.OrdinalIgnoreCase))
            {
                node.Status = ProgressStatus.Completed;
                node.CompletedAt = DateTime.UtcNow;
                _context.LearningPathNodes.Update(node);
            }

            var log = new StudyActivityLog
            {
                StudentId = userId,
                ActivityType = string.IsNullOrWhiteSpace(activityType) ? InferActivityType(node.NodeType) : activityType.ToUpperInvariant(),
                TopicId = node.TopicId,
                LearningPathNodeId = node.Id,
                DurationMinutes = durationMinutes ?? node.EstimatedMinutes,
                Score = score,
                Metadata = metadata,
                CreatedAt = DateTime.UtcNow
            };

            await _context.StudyActivityLogs.AddAsync(log);
            await UnlockNextNodeAsync(node);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Student {StudentId} completed learning path node {NodeId}", userId, nodeId);
            return true;
        }

        private async Task<PathNodeViewModel> MapNodeAsync(LearningPathNode node)
        {
            var targetUrl = await BuildNodeTargetUrlAsync(node);

            return new PathNodeViewModel
            {
                NodeId = node.Id,
                Title = node.NodeTitle,
                Description = node.NodeDescription ?? node.Lesson?.Summary ?? node.Quiz?.Description ?? node.PracticeTask?.Instruction,
                NodeType = node.NodeType,
                Status = node.Status,
                OrderIndex = node.OrderIndex,
                EstimatedMinutes = node.EstimatedMinutes,
                AiReason = node.AiReason,
                TargetUrl = targetUrl,
                IsClickable = IsOpenableStatus(node.Status) && !string.IsNullOrWhiteSpace(targetUrl),
                CssClass = GetCssClass(node.Status),
                IconClass = GetIconClass(node.NodeType),
                StatusLabel = GetStatusLabel(node.Status),
                CompletedAt = node.CompletedAt,
                ScheduledDate = node.ScheduledDate,
                TopicName = node.Topic?.Title,
                PathPhase = node.PathPhase
            };
        }

        private async Task<PathProgressSummaryViewModel> BuildProgressSummaryAsync(int userId, List<LearningPathNode> nodes)
        {
            var totalNodes = nodes.Count;
            var completedNodes = nodes.Count(n => n.Status.Equals(ProgressStatus.Completed, StringComparison.OrdinalIgnoreCase));
            var inProgressNodes = nodes.Count(n => n.Status.Equals(ProgressStatus.InProgress, StringComparison.OrdinalIgnoreCase));
            var progressPercent = totalNodes > 0
                ? Math.Round(completedNodes * 100m / totalNodes, 2)
                : 0m;

            var activities = await _context.StudyActivityLogs
                .Where(a => a.StudentId == userId)
                .AsNoTracking()
                .ToListAsync();

            return new PathProgressSummaryViewModel
            {
                TotalNodes = totalNodes,
                CompletedNodes = completedNodes,
                InProgressNodes = inProgressNodes,
                ProgressPercent = progressPercent,
                TotalStudyMinutes = activities.Sum(a => a.DurationMinutes ?? 0),
                CurrentStreak = CalculateCurrentStreak(activities)
            };
        }

        private async Task<bool> UnlockNextNodeAsync(LearningPathNode completedNode)
        {
            var nextNode = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == completedNode.LearningPathId && n.OrderIndex > completedNode.OrderIndex)
                .OrderBy(n => n.OrderIndex)
                .ThenBy(n => n.Id)
                .FirstOrDefaultAsync();

            if (nextNode == null)
            {
                return true;
            }

            if (!nextNode.Status.Equals(ProgressStatus.Locked, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            nextNode.Status = ProgressStatus.Available;
            _context.LearningPathNodes.Update(nextNode);
            return true;
        }

        private static string InferActivityType(string nodeType)
        {
            return nodeType.ToUpperInvariant() switch
            {
                NodeType.Quiz => ActivityType.Quiz,
                NodeType.Practice => ActivityType.Practice,
                NodeType.Review => ActivityType.Review,
                NodeType.AiTutor => ActivityType.Chat,
                _ => ActivityType.Learn
            };
        }

        private static bool IsOpenableStatus(string? status)
        {
            return !string.IsNullOrWhiteSpace(status) && OpenableStatuses.Contains(status);
        }

        private static int CalculateCurrentStreak(List<StudyActivityLog> activities)
        {
            var studyDates = activities
                .Where(a => !a.ActivityType.Equals(ActivityType.Login, StringComparison.OrdinalIgnoreCase))
                .Select(a => DateOnly.FromDateTime(a.CreatedAt.Date))
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

            var streak = 1;
            for (var index = 0; index < studyDates.Count - 1; index++)
            {
                if (studyDates[index].AddDays(-1) == studyDates[index + 1])
                {
                    streak++;
                    continue;
                }

                break;
            }

            return streak;
        }

        private static string GetCssClass(string? status)
        {
            return (status ?? string.Empty).ToUpperInvariant() switch
            {
                ProgressStatus.Locked    => "node-locked",
                ProgressStatus.Available => "node-available",
                ProgressStatus.InProgress => "node-in-progress",
                ProgressStatus.Completed => "node-completed",
                ProgressStatus.NeedReview => "node-need-review",
                ProgressStatus.Skipped   => "node-skipped",
                _                        => ""
            };
        }

        private static string GetIconClass(string? nodeType)
        {
            return (nodeType ?? string.Empty).ToUpperInvariant() switch
            {
                NodeType.Topic => "fa-solid fa-layer-group",
                NodeType.Lesson => "fa-solid fa-book-open",
                NodeType.Quiz => "fa-solid fa-circle-question",
                NodeType.Practice => "fa-solid fa-pen-to-square",
                NodeType.Review => "fa-solid fa-rotate-right",
                NodeType.AiTutor => "fa-solid fa-robot",
                _ => "fa-solid fa-circle"
            };
        }

        private static string GetStatusLabel(string? status)
        {
            return (status ?? string.Empty).ToUpperInvariant() switch
            {
                ProgressStatus.Locked    => "Chưa mở",
                ProgressStatus.Available => "Có thể học",
                ProgressStatus.InProgress => "Đang học",
                ProgressStatus.Completed => "Hoàn thành",
                ProgressStatus.NeedReview => "Cần ôn lại",
                ProgressStatus.Skipped   => "Đã bỏ qua",
                _ => string.IsNullOrWhiteSpace(status) ? "Không rõ" : status
            };
        }
    }
}
