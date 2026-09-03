using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels.LearningPath;
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
                .Include(p => p.LearningPathNodes)
                    .ThenInclude(n => n.RequiredNode)
                .AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.StudentId == userId && p.Status == "ACTIVE");

            if (path == null)
            {
                return new LearningPathPageViewModel
                {
                    HasPath = false,
                    PathTitle = "Chưa có lộ trình học",
                    PathStatus = "NONE"
                };
            }

            var orderedNodes = path.LearningPathNodes
                .OrderBy(n => n.OrderIndex)
                .ThenBy(n => n.Id)
                .ToList();

            // Tính điểm năng lực tích lũy của Khóa A (Phase 1)
            var courseAQuizScores = await GetCourseAQuizAverageScoreAsync(userId, path.Id);

            var nodeViewModels = new List<PathNodeViewModel>();
            foreach (var node in orderedNodes)
            {
                nodeViewModels.Add(await MapNodeAsync(node, courseAQuizScores));
            }

            var totalNodes = orderedNodes.Count;
            var completedCount = orderedNodes.Count(n => n.Status.Equals(ProgressStatus.Completed, StringComparison.OrdinalIgnoreCase));
            bool isPathCompleted = totalNodes > 0 && completedCount == totalNodes;

            // Lấy thông tin trình độ hiện tại và kế tiếp
            var profile = await _context.StudentLearningProfiles
                .Include(p => p.CurrentLevel)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            string currentLevelName = profile?.CurrentLevel?.Name ?? "A1";
            int currentLevelOrder = profile?.CurrentLevel?.OrderIndex ?? 1;

            var nextLevel = await _context.EnglishProficiencyLevels
                .Where(l => l.OrderIndex > currentLevelOrder && l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .FirstOrDefaultAsync();

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
                PathStatus = isPathCompleted ? "COMPLETED" : path.Status,
                StartDate = path.StartDate,
                TargetEndDate = path.TargetEndDate,
                GeneratedByAi = path.GeneratedByAi,
                AiPlanSummary = path.AiPlanSummary,
                HasPath = true,
                Nodes = nodeViewModels,
                TodayTasks = todayTasks,
                Progress = await BuildProgressSummaryAsync(userId, orderedNodes),
                IsPathCompleted = isPathCompleted,
                CanTakeLevelUpAssessment = isPathCompleted,
                CurrentLevelName = currentLevelName,
                NextLevelName = nextLevel?.Name ?? "A2 (Sơ cấp nâng cao)",
                NextLevelId = nextLevel?.Id
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
                NodeType.Topic when node.TopicId.HasValue => $"/Student/LearningPath/Topic/{node.Id}",
                NodeType.Lesson when node.LessonId.HasValue => $"/Lesson/Detail/{node.LessonId.Value}",
                NodeType.Quiz when node.QuizId.HasValue => $"/Quiz/Take/{node.QuizId.Value}",
                NodeType.Practice when node.PracticeTaskId.HasValue => $"/Lesson/Detail/{node.PracticeTaskId.Value}",
                NodeType.Review when node.TopicId.HasValue => $"/Student/LearningPath/Topic/{node.Id}",
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

            if (!IsOpenableStatus(node.Status))
            {
                return false;
            }

            // Kiểm tra điều kiện tiên quyết (Prerequisite Tree)
            if (node.RequiredNodeId.HasValue)
            {
                var reqNode = await _context.LearningPathNodes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(n => n.Id == node.RequiredNodeId.Value);

                if (reqNode != null && !reqNode.Status.Equals(ProgressStatus.Completed, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            // Kiểm tra điều kiện năng lực đối với Khóa B (Phase 2)
            if (IsCourseBNode(node))
            {
                var courseAScore = await GetCourseAQuizAverageScoreAsync(userId, node.LearningPathId);
                if (courseAScore < 70m)
                {
                    return false;
                }
            }

            return true;
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

        private async Task<PathNodeViewModel> MapNodeAsync(LearningPathNode node, decimal courseAQuizScores)
        {
            var targetUrl = await BuildNodeTargetUrlAsync(node);
            bool isCourseB = IsCourseBNode(node);

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
                PathPhase = node.PathPhase,
                RequiredNodeId = node.RequiredNodeId,
                RequiredNodeTitle = node.RequiredNode?.NodeTitle,
                CourseName = node.Topic?.Title ?? (node.PathPhase != null ? $"Khóa {node.PathPhase}" : null),
                IsCompetencyGated = isCourseB,
                RequiredCompetencyScore = isCourseB ? 70m : null,
                CurrentCompetencyScore = isCourseB ? courseAQuizScores : null
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
            // Tìm node tiếp theo dựa trên RequiredNodeId hoặc OrderIndex
            var nextNode = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == completedNode.LearningPathId && (n.RequiredNodeId == completedNode.Id || (n.OrderIndex > completedNode.OrderIndex && !n.RequiredNodeId.HasValue)))
                .OrderBy(n => n.OrderIndex)
                .ThenBy(n => n.Id)
                .FirstOrDefaultAsync();

            if (nextNode == null)
            {
                // Thử tìm theo OrderIndex kế tiếp
                nextNode = await _context.LearningPathNodes
                    .Where(n => n.LearningPathId == completedNode.LearningPathId && n.OrderIndex > completedNode.OrderIndex)
                    .OrderBy(n => n.OrderIndex)
                    .ThenBy(n => n.Id)
                    .FirstOrDefaultAsync();
            }

            if (nextNode == null)
            {
                return true;
            }

            if (!nextNode.Status.Equals(ProgressStatus.Locked, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Nếu node kế tiếp thuộc Khóa B: Kiểm tra điều kiện năng lực (Competency Gate)
            if (IsCourseBNode(nextNode))
            {
                var studentId = (await _context.StudentLearningPaths.FindAsync(completedNode.LearningPathId))?.StudentId ?? 0;
                var courseAScore = await GetCourseAQuizAverageScoreAsync(studentId, completedNode.LearningPathId);
                if (courseAScore < 70m)
                {
                    nextNode.Status = ProgressStatus.Locked;
                    nextNode.AiReason = $"Cần đạt tối thiểu 70% ở Khóa A (Điểm hiện tại: {courseAScore:0.#}%). Hãy làm bài ôn tập để mở khóa.";
                    _context.LearningPathNodes.Update(nextNode);
                    return false;
                }
            }

            nextNode.Status = ProgressStatus.Available;
            _context.LearningPathNodes.Update(nextNode);
            return true;
        }

        private static bool IsCourseBNode(LearningPathNode node)
        {
            if (string.IsNullOrEmpty(node.PathPhase)) return false;
            return node.PathPhase.Contains("B", StringComparison.OrdinalIgnoreCase) ||
                   node.PathPhase.Contains("Khóa B", StringComparison.OrdinalIgnoreCase) ||
                   node.PathPhase.Contains("Phase 2", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<decimal> GetCourseAQuizAverageScoreAsync(int studentId, int pathId)
        {
            var courseANodeIds = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == pathId &&
                           (n.PathPhase == null || n.PathPhase.Contains("A") || n.PathPhase.Contains("Khóa A") || n.PathPhase.Contains("Phase 1")))
                .Select(n => n.Id)
                .ToListAsync();

            if (!courseANodeIds.Any()) return 100m;

            var scores = await _context.StudyActivityLogs
                .Where(a => a.StudentId == studentId &&
                            a.LearningPathNodeId.HasValue &&
                            courseANodeIds.Contains(a.LearningPathNodeId.Value) &&
                            a.Score.HasValue)
                .Select(a => a.Score!.Value)
                .ToListAsync();

            if (!scores.Any())
            {
                // Nếu chưa có log quiz nhưng hoàn thành các node bài học Khóa A thì mặc định 75%
                var completedCount = await _context.LearningPathNodes
                    .CountAsync(n => courseANodeIds.Contains(n.Id) && n.Status == ProgressStatus.Completed);
                return completedCount > 0 ? 80m : 0m;
            }

            // Chuẩn hóa score nếu tính trên thang 10 sang thang 100
            var avg = scores.Average();
            return avg <= 10m ? avg * 10m : avg;
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
