using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.Progress;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Progress;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class StudentProgressService : IStudentProgressService
    {
        private readonly IActivityLogRepository _activityLogRepo;
        private readonly IProgressRepository _progressRepo;
        private readonly ApplicationDbContext _context;

        public StudentProgressService(
            IActivityLogRepository activityLogRepo,
            IProgressRepository progressRepo,
            ApplicationDbContext context)
        {
            _activityLogRepo = activityLogRepo;
            _progressRepo = progressRepo;
            _context = context;
        }

        public async Task RecordActivityAsync(ActivityLogCreateDto dto, int studentId)
        {
            // 1. Ghi nhận hoạt động mới
            var activity = new StudyActivityLog
            {
                StudentId = studentId,
                ActivityType = dto.ActivityType.ToUpper(),
                TopicId = dto.TopicId,
                LearningPathNodeId = dto.LearningPathNodeId,
                DurationMinutes = dto.DurationMinutes,
                Score = dto.Score,
                Metadata = dto.Metadata,
                CreatedAt = DateTime.UtcNow
            };

            await _activityLogRepo.AddAsync(activity);
            await _activityLogRepo.SaveChangesAsync();

            // 2. Cập nhật trạng thái của LearningPathNode tương ứng (nếu có)
            if (dto.LearningPathNodeId.HasValue)
            {
                var node = await _context.LearningPathNodes
                    .Include(n => n.Topic)
                    .FirstOrDefaultAsync(n => n.Id == dto.LearningPathNodeId.Value);

                if (node != null)
                {
                    bool isCompleted = false;

                    if (activity.ActivityType == ActivityType.Learn)
                    {
                        // Hoàn thành bài học
                        isCompleted = true;
                    }
                    else if (activity.ActivityType == ActivityType.Quiz)
                    {
                        // Đạt điểm quiz từ 5.0 trở lên được coi là hoàn thành
                        if (!dto.Score.HasValue || dto.Score.Value >= 5.0m)
                        {
                            isCompleted = true;
                        }
                        else
                        {
                            node.Status = ProgressStatus.NeedReview;
                        }
                    }
                    else if (activity.ActivityType == ActivityType.Practice)
                    {
                        // Hoàn thành phần luyện tập khi nộp bài
                        isCompleted = true;
                    }

                    if (isCompleted)
                    {
                        node.Status = ProgressStatus.Completed;
                        node.CompletedAt = DateTime.UtcNow;

                        // Tự động mở khóa Node tiếp theo trong lộ trình
                        var nextNode = await _context.LearningPathNodes
                            .Where(n => n.LearningPathId == node.LearningPathId && n.OrderIndex > node.OrderIndex)
                            .OrderBy(n => n.OrderIndex)
                            .FirstOrDefaultAsync();

                        if (nextNode != null && nextNode.Status == ProgressStatus.Locked)
                        {
                            nextNode.Status = ProgressStatus.Available;
                            _context.Update(nextNode);
                        }
                    }

                    _context.Update(node);
                    await _context.SaveChangesAsync();
                }
            }

            // 3. Cập nhật Snapshot tiến độ của ngày hôm nay
            await UpdateProgressSnapshotsAsync(studentId);
        }

        public async Task<ProgressDashboardViewModel> GetDashboardAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // 1. Tính toán Streak học tập
            var activities = await _activityLogRepo.GetActivitiesForStreak(studentId);
            int streak = CalculateStreak(activities);

            // 2. Tính tổng thời gian học (phút)
            int totalStudyMinutes = await _activityLogRepo.GetTotalStudyMinutes(studentId);

            // 3. Đếm tổng số bài học đã hoàn thành (LEARN node)
            int completedLessons = await _context.LearningPathNodes
                .CountAsync(n => n.LearningPath.StudentId == studentId && n.Status == ProgressStatus.Completed && n.NodeType == "LEARN");

            // 4. Lấy tiến độ của từng kỹ năng
            var skills = await _context.EnglishSkills.Where(s => s.IsActive).OrderBy(s => s.OrderIndex).ToListAsync();
            var skillProgressList = new List<SkillProgressViewModel>();

            var pathNodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                .Where(n => n.LearningPath.StudentId == studentId && n.LearningPath.Status == "ACTIVE")
                .ToListAsync();

            foreach (var skill in skills)
            {
                var skillNodes = pathNodes.Where(n => n.Topic != null && n.Topic.SkillId == skill.Id).ToList();
                int totalNodes = skillNodes.Count;
                int completedNodes = skillNodes.Count(n => n.Status == ProgressStatus.Completed);
                decimal progressPercent = totalNodes > 0 ? (decimal)completedNodes * 100m / totalNodes : 0m;

                // Lấy điểm trung bình của kỹ năng này từ logs
                var skillTopicIds = skillNodes.Where(n => n.TopicId.HasValue).Select(n => n.TopicId!.Value).Distinct().ToList();
                var skillScores = activities.Where(a => a.TopicId.HasValue && skillTopicIds.Contains(a.TopicId.Value) && a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                decimal? averageScore = skillScores.Any() ? skillScores.Average() : null;

                skillProgressList.Add(new SkillProgressViewModel
                {
                    SkillId = skill.Id,
                    SkillCode = skill.SkillCode,
                    SkillName = skill.SkillName,
                    ProgressPercent = Math.Round(progressPercent, 1),
                    CompletedNodes = completedNodes,
                    TotalNodes = totalNodes,
                    AverageScore = averageScore.HasValue ? Math.Round(averageScore.Value, 1) : null
                });
            }

            // 5. Xác định chủ đề yếu (Weak Points) và chủ đề đã cải thiện (Improved Topics)
            var weakTopics = new List<TopicProgressViewModel>();
            var improvedTopics = new List<TopicProgressViewModel>();
            var topicsWithNodes = pathNodes.Where(n => n.TopicId.HasValue).Select(n => n.Topic).Distinct().ToList();

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            foreach (var topic in topicsWithNodes)
            {
                if (topic == null) continue;

                var topicNodes = pathNodes.Where(n => n.TopicId == topic.Id).ToList();
                bool hasNeedReviewNode = topicNodes.Any(n => n.Status == ProgressStatus.NeedReview);

                // Lọc hoạt động trong quá khứ và gần đây
                var topicScoresPast = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue && a.CreatedAt < sevenDaysAgo).Select(a => a.Score!.Value).ToList();
                var topicScoresRecent = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue && a.CreatedAt >= sevenDaysAgo).Select(a => a.Score!.Value).ToList();
                var allTopicScores = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue).Select(a => a.Score!.Value).ToList();

                decimal? pastAvg = topicScoresPast.Any() ? topicScoresPast.Average() : null;
                decimal? recentAvg = topicScoresRecent.Any() ? topicScoresRecent.Average() : null;
                decimal? overallAvg = allTopicScores.Any() ? allTopicScores.Average() : null;

                // 5.1 Xác định chủ đề yếu (GPA < 5.0 hoặc có node cần ôn tập)
                bool isWeak = hasNeedReviewNode || (overallAvg.HasValue && overallAvg.Value < 5.0m);

                int totalNodes = topicNodes.Count;
                int completedNodes = topicNodes.Count(n => n.Status == ProgressStatus.Completed);
                decimal progressPercent = totalNodes > 0 ? (decimal)completedNodes * 100m / totalNodes : 0m;
                var skill = skills.FirstOrDefault(s => s.Id == topic.SkillId);

                if (isWeak)
                {
                    weakTopics.Add(new TopicProgressViewModel
                    {
                        TopicId = topic.Id,
                        TopicName = topic.Title,
                        SkillName = skill?.SkillName ?? "Khác",
                        ProgressPercent = Math.Round(progressPercent, 1),
                        AverageScore = overallAvg.HasValue ? Math.Round(overallAvg.Value, 1) : null,
                        IsWeakArea = true
                    });
                }

                // 5.2 Xác định chủ đề cải thiện (trước < 5.0 nay >= 7.0, hoặc có ôn tập và điểm gần đây >= 7.0)
                bool hasBeenReviewed = activities.Any(a => a.TopicId == topic.Id && a.ActivityType == ActivityType.Review);
                bool isImproved = false;

                if (pastAvg.HasValue && pastAvg.Value < 5.0m && recentAvg.HasValue && recentAvg.Value >= 7.0m)
                {
                    isImproved = true;
                }
                else if (hasBeenReviewed && recentAvg.HasValue && recentAvg.Value >= 7.0m)
                {
                    isImproved = true;
                }

                if (isImproved)
                {
                    improvedTopics.Add(new TopicProgressViewModel
                    {
                        TopicId = topic.Id,
                        TopicName = topic.Title,
                        SkillName = skill?.SkillName ?? "Khác",
                        ProgressPercent = Math.Round(progressPercent, 1),
                        AverageScore = recentAvg.HasValue ? Math.Round(recentAvg.Value, 1) : null,
                        IsWeakArea = false
                    });
                }
            }

            // Đếm tổng số bài Quiz đã hoàn thành
            int completedQuizzes = await _context.LearningPathNodes
                .Where(n => n.LearningPath.StudentId == studentId && n.LearningPath.Status == "ACTIVE" && n.QuizId != null && n.Status == ProgressStatus.Completed)
                .CountAsync();

            // 6. Lấy 5 hoạt động học tập gần đây nhất
            var recentActivities = await _activityLogRepo.GetRecentActivities(studentId, 5);
            var recentHistory = recentActivities.Select(a => new LearningHistoryItemViewModel
            {
                ActivityType = a.ActivityType,
                ActivityDate = a.CreatedAt.AddHours(7),
                DurationMinutes = a.DurationMinutes,
                Score = a.Score,
                Title = a.Topic?.Title ?? a.LearningPathNode?.NodeTitle ?? "Hoạt động chung",
                MetadataJson = a.Metadata
            }).ToList();

            return new ProgressDashboardViewModel
            {
                StreakDays = streak,
                TotalStudyMinutes = totalStudyMinutes,
                CompletedLessonsCount = completedLessons,
                CompletedQuizzesCount = completedQuizzes,
                SkillProgresses = skillProgressList,
                WeakTopics = weakTopics,
                ImprovedTopics = improvedTopics,
                RecentActivities = recentHistory
            };
        }

        public async Task<List<LearningHistoryItemViewModel>> GetLearningHistoryAsync(int studentId, int page, int pageSize)
        {
            var query = _context.StudyActivityLogs
                .Include(a => a.Topic)
                .Include(a => a.LearningPathNode)
                .Where(a => a.StudentId == studentId);

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return logs.Select(a => new LearningHistoryItemViewModel
            {
                ActivityType = a.ActivityType,
                ActivityDate = a.CreatedAt.AddHours(7), // Múi giờ VN
                DurationMinutes = a.DurationMinutes,
                Score = a.Score,
                Title = a.Topic?.Title ?? a.LearningPathNode?.NodeTitle ?? "Hoạt động chung",
                MetadataJson = a.Metadata
            }).ToList();
        }

        public async Task UpdateProgressSnapshotsAsync(int studentId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var activities = await _activityLogRepo.GetActivitiesForStreak(studentId);
            var pathNodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                .Where(n => n.LearningPath.StudentId == studentId && n.LearningPath.Status == "ACTIVE")
                .ToListAsync();

            int totalStudyMinutes = await _activityLogRepo.GetTotalStudyMinutes(studentId);

            // 1. Tạo/Cập nhật snapshot tổng quát (overall)
            int totalNodes = pathNodes.Count;
            int completedNodes = pathNodes.Count(n => n.Status == ProgressStatus.Completed);
            decimal progressPercent = totalNodes > 0 ? (decimal)completedNodes * 100m / totalNodes : 0m;

            var overallScores = activities.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
            decimal? overallAvgScore = overallScores.Any() ? overallScores.Average() : null;

            var overallSnapshot = await _context.StudentProgressSnapshots
                .FirstOrDefaultAsync(s => s.StudentId == studentId && s.SkillId == null && s.TopicId == null && s.SnapshotDate == today);

            if (overallSnapshot == null)
            {
                overallSnapshot = new StudentProgressSnapshot
                {
                    StudentId = studentId,
                    SnapshotDate = today
                };
                await _context.StudentProgressSnapshots.AddAsync(overallSnapshot);
            }

            overallSnapshot.ProgressPercent = Math.Round(progressPercent, 2);
            overallSnapshot.AverageScore = overallAvgScore.HasValue ? Math.Round(overallAvgScore.Value, 2) : null;
            overallSnapshot.TotalStudyMinutes = totalStudyMinutes;
            overallSnapshot.CompletedNodes = completedNodes;

            var weakTopicsList = pathNodes
                .Where(n => n.Status == ProgressStatus.NeedReview || 
                           (activities.Where(a => a.TopicId == n.TopicId && a.Score.HasValue)
                                      .Select(a => a.Score!.Value).DefaultIfEmpty(10m).Average() < 6.0m))
                .Select(n => n.Topic?.Title)
                .Where(t => t != null)
                .Distinct()
                .ToList();
            overallSnapshot.WeakPoints = string.Join(", ", weakTopicsList);

            // 2. Tạo/Cập nhật snapshot cho từng kỹ năng
            var skills = await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync();
            foreach (var skill in skills)
            {
                var skillNodes = pathNodes.Where(n => n.Topic != null && n.Topic.SkillId == skill.Id).ToList();
                int skillTotal = skillNodes.Count;
                if (skillTotal == 0) continue;

                int skillCompleted = skillNodes.Count(n => n.Status == ProgressStatus.Completed);
                decimal skillProgress = (decimal)skillCompleted * 100m / skillTotal;

                var skillTopicIds = skillNodes.Where(n => n.TopicId.HasValue).Select(n => n.TopicId!.Value).Distinct().ToList();
                var skillScores = activities.Where(a => a.TopicId.HasValue && skillTopicIds.Contains(a.TopicId.Value) && a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                decimal? skillAvgScore = skillScores.Any() ? skillScores.Average() : null;

                // Thời gian học cho kỹ năng này
                int skillMinutes = activities.Where(a => a.TopicId.HasValue && skillTopicIds.Contains(a.TopicId.Value) && a.DurationMinutes.HasValue).Sum(a => a.DurationMinutes!.Value);

                var skillSnapshot = await _context.StudentProgressSnapshots
                    .FirstOrDefaultAsync(s => s.StudentId == studentId && s.SkillId == skill.Id && s.TopicId == null && s.SnapshotDate == today);

                if (skillSnapshot == null)
                {
                    skillSnapshot = new StudentProgressSnapshot
                    {
                        StudentId = studentId,
                        SkillId = skill.Id,
                        SnapshotDate = today
                    };
                    await _context.StudentProgressSnapshots.AddAsync(skillSnapshot);
                }

                skillSnapshot.ProgressPercent = Math.Round(skillProgress, 2);
                skillSnapshot.AverageScore = skillAvgScore.HasValue ? Math.Round(skillAvgScore.Value, 2) : null;
                skillSnapshot.TotalStudyMinutes = skillMinutes;
                skillSnapshot.CompletedNodes = skillCompleted;
            }

            // 3. Tạo/Cập nhật snapshot cho các chủ đề trong lộ trình hoạt động
            var topics = pathNodes.Where(n => n.TopicId.HasValue).Select(n => n.Topic).Distinct().ToList();
            foreach (var topic in topics)
            {
                if (topic == null) continue;

                var topicNodes = pathNodes.Where(n => n.TopicId == topic.Id).ToList();
                int topicTotal = topicNodes.Count;
                if (topicTotal == 0) continue;

                int topicCompleted = topicNodes.Count(n => n.Status == ProgressStatus.Completed);
                decimal topicProgress = (decimal)topicCompleted * 100m / topicTotal;

                var topicScores = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                decimal? topicAvgScore = topicScores.Any() ? topicScores.Average() : null;

                int topicMinutes = activities.Where(a => a.TopicId == topic.Id && a.DurationMinutes.HasValue).Sum(a => a.DurationMinutes!.Value);

                var topicSnapshot = await _context.StudentProgressSnapshots
                    .FirstOrDefaultAsync(s => s.StudentId == studentId && s.SkillId == null && s.TopicId == topic.Id && s.SnapshotDate == today);

                if (topicSnapshot == null)
                {
                    topicSnapshot = new StudentProgressSnapshot
                    {
                        StudentId = studentId,
                        TopicId = topic.Id,
                        SnapshotDate = today
                    };
                    await _context.StudentProgressSnapshots.AddAsync(topicSnapshot);
                }

                topicSnapshot.ProgressPercent = Math.Round(topicProgress, 2);
                topicSnapshot.AverageScore = topicAvgScore.HasValue ? Math.Round(topicAvgScore.Value, 2) : null;
                topicSnapshot.TotalStudyMinutes = topicMinutes;
                topicSnapshot.CompletedNodes = topicCompleted;
            }

            await _context.SaveChangesAsync();
        }

        private int CalculateStreak(List<StudyActivityLog> logs)
        {
            // Bỏ các hoạt động LOGIN và chỉ lấy các hoạt động học tập thực tế để tính streak
            var studyDates = logs
                .Where(a => a.ActivityType != ActivityType.Login)
                .Select(a => DateOnly.FromDateTime(a.CreatedAt.Date))
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (!studyDates.Any()) return 0;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var yesterday = today.AddDays(-1);

            // Nếu ngày học cuối cùng không phải hôm nay và cũng không phải hôm qua, streak = 0
            if (studyDates[0] != today && studyDates[0] != yesterday)
            {
                return 0;
            }

            int streak = 1;
            for (int i = 0; i < studyDates.Count - 1; i++)
            {
                // So sánh khoảng cách ngày học tiếp theo
                if (studyDates[i].AddDays(-1) == studyDates[i + 1])
                {
                    streak++;
                }
                else if (studyDates[i] == studyDates[i + 1])
                {
                    continue; // Cùng ngày
                }
                else
                {
                    break; // Có khoảng trống
                }
            }

            return streak;
        }

        public async Task<ReplanningInputDto> GetReplanningInputAsync(int studentId, int pathId)
        {
            var pathExists = await _context.StudentLearningPaths.AnyAsync(p => p.Id == pathId && p.StudentId == studentId);
            if (!pathExists)
            {
                throw new KeyNotFoundException($"Learning path with ID {pathId} for student ID {studentId} not found.");
            }

            // 1. Các learningPathNode trong path chưa hoàn thành
            var remainingNodes = await _context.LearningPathNodes
                .Where(n => n.LearningPathId == pathId && n.Status != ProgressStatus.Completed)
                .Select(n => n.Id)
                .ToListAsync();

            // 2. Lấy dữ liệu hoạt động & lộ trình để tính weakTopics và fastImprovementTopics
            var pathNodes = await _context.LearningPathNodes
                .Include(n => n.Topic)
                .Where(n => n.LearningPathId == pathId)
                .ToListAsync();

            var activities = await _context.StudyActivityLogs
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

            var weakTopics = new List<string>();
            var fastImprovementTopics = new List<string>();
            var topicsWithNodes = pathNodes.Where(n => n.TopicId.HasValue).Select(n => n.Topic).Distinct().ToList();

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            foreach (var topic in topicsWithNodes)
            {
                if (topic == null) continue;

                var topicNodes = pathNodes.Where(n => n.TopicId == topic.Id).ToList();
                bool hasNeedReviewNode = topicNodes.Any(n => n.Status == ProgressStatus.NeedReview);

                var topicScoresPast = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue && a.CreatedAt < sevenDaysAgo).Select(a => a.Score!.Value).ToList();
                var topicScoresRecent = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue && a.CreatedAt >= sevenDaysAgo).Select(a => a.Score!.Value).ToList();
                var allTopicScores = activities.Where(a => a.TopicId == topic.Id && a.Score.HasValue).Select(a => a.Score!.Value).ToList();

                decimal? pastAvg = topicScoresPast.Any() ? topicScoresPast.Average() : null;
                decimal? recentAvg = topicScoresRecent.Any() ? topicScoresRecent.Average() : null;
                decimal? overallAvg = allTopicScores.Any() ? allTopicScores.Average() : null;

                // weakTopics: điểm trung bình < 5.0 hoặc có node cần ôn tập
                if (hasNeedReviewNode || (overallAvg.HasValue && overallAvg.Value < 5.0m))
                {
                    weakTopics.Add(topic.Title);
                }

                // fastImprovementTopics: trước < 5.0 nay >= 7.0, hoặc có ôn tập và điểm gần đây >= 7.0
                bool hasBeenReviewed = activities.Any(a => a.TopicId == topic.Id && a.ActivityType == ActivityType.Review);
                bool isImproved = false;

                if (pastAvg.HasValue && pastAvg.Value < 5.0m && recentAvg.HasValue && recentAvg.Value >= 7.0m)
                {
                    isImproved = true;
                }
                else if (hasBeenReviewed && recentAvg.HasValue && recentAvg.Value >= 7.0m)
                {
                    isImproved = true;
                }

                if (isImproved)
                {
                    fastImprovementTopics.Add(topic.Title);
                }
            }

            // 3. inactiveDays: ngày không có hoạt động học trong 30 ngày qua
            var inactiveDays = new List<string>();
            var studyDates = activities
                .Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-30) && a.ActivityType != ActivityType.Login)
                .Select(a => a.CreatedAt.Date)
                .Distinct()
                .ToList();

            for (int offset = 0; offset < 30; offset++)
            {
                var checkDate = DateTime.UtcNow.Date.AddDays(-offset);
                if (!studyDates.Contains(checkDate))
                {
                    inactiveDays.Add(checkDate.ToString("yyyy-MM-dd"));
                }
            }

            inactiveDays.Reverse(); // Sắp xếp từ cũ nhất đến gần đây nhất

            return new ReplanningInputDto
            {
                StudentId = studentId,
                PathId = pathId,
                RemainingNodes = remainingNodes,
                WeakTopics = weakTopics,
                InactiveDays = inactiveDays,
                FastImprovementTopics = fastImprovementTopics
            };
        }
    }
}
