using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class ProgressTrackingService : IProgressTrackingService
    {
        private readonly IProgressRepository _progressRepo;
        private readonly IActivityLogRepository _activityLogRepo;
        private readonly ApplicationDbContext _context;

        public ProgressTrackingService(
            IProgressRepository progressRepo,
            IActivityLogRepository activityLogRepo,
            ApplicationDbContext context)
        {
            _progressRepo = progressRepo;
            _activityLogRepo = activityLogRepo;
            _context = context;
        }

        public async Task<bool> RecordLessonCompleted(int studentId, int lessonId, int? durationMinutes = null)
        {
            try
            {
                // Kiểm tra sự tồn tại của bài học
                var lesson = await _context.OriginalLessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    // Log lỗi và trả về false
                    System.Diagnostics.Debug.WriteLine($"Lesson with ID {lessonId} does not exist.");
                    return false;
                }

                // Tìm node lộ trình tương ứng
                var node = await _context.LearningPathNodes
                    .Include(n => n.LearningPath)
                    .FirstOrDefaultAsync(n => n.LearningPath.StudentId == studentId && n.LessonId == lessonId && n.LearningPath.Status == "ACTIVE");

                if (node == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No active path node found for StudentId {studentId} and LessonId {lessonId}.");
                    return false;
                }

                // Tránh ghi trùng lặp: Nếu node đã hoàn thành thì bỏ qua
                if (node.Status == ProgressStatus.Completed)
                {
                    return true;
                }

                using var transaction = IsInMemoryDatabase() ? null : await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Thêm log hoạt động
                    var log = new StudyActivityLog
                    {
                        StudentId = studentId,
                        ActivityType = ActivityType.Learn,
                        TopicId = node.TopicId,
                        LearningPathNodeId = node.Id,
                        DurationMinutes = durationMinutes ?? node.EstimatedMinutes ?? 15,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _activityLogRepo.AddActivityLog(log);
                    await _activityLogRepo.SaveChangesAsync();

                    // 2. Cập nhật node
                    node.Status = ProgressStatus.Completed;
                    node.CompletedAt = DateTime.UtcNow;
                    _context.LearningPathNodes.Update(node);

                    // 3. Mở khóa node kế tiếp
                    var nextNode = await _context.LearningPathNodes
                        .Where(n => n.LearningPathId == node.LearningPathId && n.OrderIndex > node.OrderIndex)
                        .OrderBy(n => n.OrderIndex)
                        .FirstOrDefaultAsync();

                    if (nextNode != null && nextNode.Status == ProgressStatus.Locked)
                    {
                        nextNode.Status = ProgressStatus.Available;
                        _context.LearningPathNodes.Update(nextNode);
                    }

                    await _context.SaveChangesAsync();

                    // 4. Tính toán lại Snapshot tiến độ
                    await RecalculateStudentProgress(studentId);

                    if (transaction != null) await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"Error during RecordLessonCompleted transaction: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RecordLessonCompleted: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RecordQuizSubmitted(int studentId, int quizId, decimal score, int? durationMinutes = null)
        {
            try
            {
                // Kiểm tra sự tồn tại của quiz
                var quiz = await _context.Quizzes.FindAsync(quizId);
                if (quiz == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Quiz with ID {quizId} does not exist.");
                    return false;
                }

                var node = await _context.LearningPathNodes
                    .Include(n => n.LearningPath)
                    .FirstOrDefaultAsync(n => n.LearningPath.StudentId == studentId && n.QuizId == quizId && n.LearningPath.Status == "ACTIVE");

                if (node == null)
                {
                    System.Diagnostics.Debug.WriteLine($"No active path node found for StudentId {studentId} and QuizId {quizId}.");
                    return false;
                }

                // Chuẩn hóa điểm về thang 10 nếu điểm được nộp theo thang 100
                decimal normalizedScore = score > 10.0m ? score / 10.0m : score;
                
                using var transaction = IsInMemoryDatabase() ? null : await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Thêm log hoạt động
                    var log = new StudyActivityLog
                    {
                        StudentId = studentId,
                        ActivityType = ActivityType.Quiz,
                        TopicId = node.TopicId,
                        LearningPathNodeId = node.Id,
                        DurationMinutes = durationMinutes ?? node.EstimatedMinutes ?? 10,
                        Score = Math.Round(normalizedScore, 1),
                        CreatedAt = DateTime.UtcNow
                    };
                    await _activityLogRepo.AddActivityLog(log);
                    await _activityLogRepo.SaveChangesAsync();

                    // 2. Cập nhật node: đạt nếu điểm >= 5.0 (tương đương 50%)
                    if (normalizedScore >= 5.0m)
                    {
                        node.Status = ProgressStatus.Completed;
                        node.CompletedAt = DateTime.UtcNow;

                        // Mở khóa node tiếp theo
                        var nextNode = await _context.LearningPathNodes
                            .Where(n => n.LearningPathId == node.LearningPathId && n.OrderIndex > node.OrderIndex)
                            .OrderBy(n => n.OrderIndex)
                            .FirstOrDefaultAsync();

                        if (nextNode != null && nextNode.Status == ProgressStatus.Locked)
                        {
                            nextNode.Status = ProgressStatus.Available;
                            _context.LearningPathNodes.Update(nextNode);
                        }
                    }
                    else
                    {
                        node.Status = ProgressStatus.NeedReview;
                    }

                    _context.LearningPathNodes.Update(node);
                    await _context.SaveChangesAsync();

                    // 3. Tính lại tiến độ
                    await RecalculateStudentProgress(studentId);

                    if (transaction != null) await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"Error during RecordQuizSubmitted transaction: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RecordQuizSubmitted: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RecordFeedbackViewed(int studentId, int feedbackId, bool triggerReview = false)
        {
            try
            {
                // Lấy thông tin phản hồi và xác định topicId tương ứng
                var feedback = await _context.AiFeedbacks
                    .Include(f => f.QuizAttempt)
                        .ThenInclude(qa => qa!.Quiz)
                    .Include(f => f.PracticeSubmission)
                        .ThenInclude(ps => ps!.PracticeTask)
                    .FirstOrDefaultAsync(f => f.Id == feedbackId);

                if (feedback == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Feedback with ID {feedbackId} does not exist.");
                    return false;
                }

                int? topicId = feedback.QuizAttempt?.Quiz?.TopicId ?? feedback.PracticeSubmission?.PracticeTask?.TopicId;

                using var transaction = IsInMemoryDatabase() ? null : await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Ghi log xem feedback
                    var logView = new StudyActivityLog
                    {
                        StudentId = studentId,
                        ActivityType = ActivityType.FeedbackView,
                        TopicId = topicId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _activityLogRepo.AddActivityLog(logView);

                    // 2. Nếu chọn ôn tập, ghi nhận log REVIEW và cập nhật trạng thái node lộ trình học tập
                    if (triggerReview && topicId.HasValue)
                    {
                        var logReview = new StudyActivityLog
                        {
                            StudentId = studentId,
                            ActivityType = ActivityType.Review,
                            TopicId = topicId,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _activityLogRepo.AddActivityLog(logReview);

                        // Cập nhật trạng thái node thành NEED_REVIEW để đánh dấu cần ôn tập
                        var node = await _context.LearningPathNodes
                            .Include(n => n.LearningPath)
                            .FirstOrDefaultAsync(n => n.LearningPath.StudentId == studentId && n.TopicId == topicId.Value && n.LearningPath.Status == "ACTIVE");
                        
                        if (node != null)
                        {
                            node.Status = ProgressStatus.NeedReview;
                            _context.LearningPathNodes.Update(node);
                        }
                    }

                    await _activityLogRepo.SaveChangesAsync();
                    await _context.SaveChangesAsync();

                    // 3. Tính lại tiến độ
                    await RecalculateStudentProgress(studentId);

                    if (transaction != null) await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"Error during RecordFeedbackViewed transaction: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RecordFeedbackViewed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RecordTutorMessage(int studentId, int tutorSessionId, int? durationMinutes = null)
        {
            try
            {
                // Kiểm tra chống spam: giới hạn mỗi session chỉ có một log chat
                string sessionMetadata = $"session_{tutorSessionId}";
                bool alreadyLogged = await _context.StudyActivityLogs.AnyAsync(l => 
                    l.StudentId == studentId && 
                    l.ActivityType == ActivityType.Chat && 
                    l.Metadata == sessionMetadata);

                if (alreadyLogged)
                {
                    return true;
                }

                // Truy vấn cuộc hội thoại để lấy topic hoặc node
                var conv = await _context.AiTutorConversations.FindAsync(tutorSessionId);
                int? topicId = conv?.TopicId;
                int? nodeId = conv?.LearningPathNodeId;

                using var transaction = IsInMemoryDatabase() ? null : await _context.Database.BeginTransactionAsync();
                try
                {
                    var log = new StudyActivityLog
                    {
                        StudentId = studentId,
                        ActivityType = ActivityType.Chat,
                        TopicId = topicId,
                        LearningPathNodeId = nodeId,
                        DurationMinutes = durationMinutes ?? 5,
                        Metadata = sessionMetadata,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _activityLogRepo.AddActivityLog(log);
                    await _activityLogRepo.SaveChangesAsync();

                    // Cập nhật node AI_TUTOR hoàn thành nếu gắn với lộ trình
                    if (nodeId.HasValue)
                    {
                        var node = await _context.LearningPathNodes.FindAsync(nodeId.Value);
                        if (node != null && node.Status != ProgressStatus.Completed)
                        {
                            node.Status = ProgressStatus.Completed;
                            node.CompletedAt = DateTime.UtcNow;
                            _context.LearningPathNodes.Update(node);

                            // Mở khóa node tiếp theo
                            var nextNode = await _context.LearningPathNodes
                                .Where(n => n.LearningPathId == node.LearningPathId && n.OrderIndex > node.OrderIndex)
                                .OrderBy(n => n.OrderIndex)
                                .FirstOrDefaultAsync();

                            if (nextNode != null && nextNode.Status == ProgressStatus.Locked)
                            {
                                nextNode.Status = ProgressStatus.Available;
                                _context.LearningPathNodes.Update(nextNode);
                            }

                            await _context.SaveChangesAsync();
                        }
                    }

                    // Tính lại tiến độ
                    await RecalculateStudentProgress(studentId);

                    if (transaction != null) await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    if (transaction != null) await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"Error during RecordTutorMessage transaction: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RecordTutorMessage: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateNodeProgress(int studentId, int learningPathNodeId)
        {
            try
            {
                var node = await _context.LearningPathNodes.FindAsync(learningPathNodeId);
                if (node == null) return false;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    node.Status = ProgressStatus.Completed;
                    node.CompletedAt = DateTime.UtcNow;
                    _context.LearningPathNodes.Update(node);

                    // Mở khóa node tiếp theo
                    var nextNode = await _context.LearningPathNodes
                        .Where(n => n.LearningPathId == node.LearningPathId && n.OrderIndex > node.OrderIndex)
                        .OrderBy(n => n.OrderIndex)
                        .FirstOrDefaultAsync();

                    if (nextNode != null && nextNode.Status == ProgressStatus.Locked)
                    {
                        nextNode.Status = ProgressStatus.Available;
                        _context.LearningPathNodes.Update(nextNode);
                    }

                    await _context.SaveChangesAsync();

                    // Tính lại tiến độ
                    await RecalculateStudentProgress(studentId);

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    System.Diagnostics.Debug.WriteLine($"Error during UpdateNodeProgress transaction: {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UpdateNodeProgress: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RecalculateStudentProgress(int studentId)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var activities = await _activityLogRepo.GetActivitiesForStreak(studentId);
                var pathNodes = await _context.LearningPathNodes
                    .Include(n => n.Topic)
                    .Where(n => n.LearningPath.StudentId == studentId && n.LearningPath.Status == "ACTIVE")
                    .ToListAsync();

                int totalStudyMinutes = await _activityLogRepo.GetTotalStudyMinutes(studentId);

                // 1. Snapshot tổng quát (overall)
                int totalNodes = pathNodes.Count;
                int completedNodes = pathNodes.Count(n => n.Status == ProgressStatus.Completed);
                decimal progressPercent = totalNodes > 0 ? (decimal)completedNodes * 100m / totalNodes : 0m;

                var overallScores = activities.Where(a => a.Score.HasValue).Select(a => a.Score!.Value).ToList();
                decimal? overallAvgScore = overallScores.Any() ? overallScores.Average() : null;

                var overallSnapshot = new StudentProgressSnapshot
                {
                    StudentId = studentId,
                    SnapshotDate = today,
                    ProgressPercent = Math.Round(progressPercent, 2),
                    AverageScore = overallAvgScore.HasValue ? Math.Round(overallAvgScore.Value, 2) : null,
                    TotalStudyMinutes = totalStudyMinutes,
                    CompletedNodes = completedNodes
                };

                var weakTopicsList = pathNodes
                    .Where(n => n.Status == ProgressStatus.NeedReview || 
                               (activities.Where(a => a.TopicId == n.TopicId && a.Score.HasValue)
                                          .Select(a => a.Score!.Value).DefaultIfEmpty(10m).Average() < 6.0m))
                    .Select(n => n.Topic?.Title)
                    .Where(t => t != null)
                    .Distinct()
                    .ToList();
                overallSnapshot.WeakPoints = string.Join(", ", weakTopicsList);

                await _progressRepo.UpsertSnapshot(overallSnapshot);

                // 2. Snapshot theo từng kỹ năng
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
                    int skillMinutes = activities.Where(a => a.TopicId.HasValue && skillTopicIds.Contains(a.TopicId.Value) && a.DurationMinutes.HasValue).Sum(a => a.DurationMinutes!.Value);

                    var skillSnapshot = new StudentProgressSnapshot
                    {
                        StudentId = studentId,
                        SkillId = skill.Id,
                        SnapshotDate = today,
                        ProgressPercent = Math.Round(skillProgress, 2),
                        AverageScore = skillAvgScore.HasValue ? Math.Round(skillAvgScore.Value, 2) : null,
                        TotalStudyMinutes = skillMinutes,
                        CompletedNodes = skillCompleted
                    };
                    await _progressRepo.UpsertSnapshot(skillSnapshot);
                }

                // 3. Snapshot theo từng chủ đề trong lộ trình
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

                    var topicSnapshot = new StudentProgressSnapshot
                    {
                        StudentId = studentId,
                        TopicId = topic.Id,
                        SnapshotDate = today,
                        ProgressPercent = Math.Round(topicProgress, 2),
                        AverageScore = topicAvgScore.HasValue ? Math.Round(topicAvgScore.Value, 2) : null,
                        TotalStudyMinutes = topicMinutes,
                        CompletedNodes = topicCompleted
                    };
                    await _progressRepo.UpsertSnapshot(topicSnapshot);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in RecalculateStudentProgress: {ex.Message}");
                return false;
            }
        }

        private bool IsInMemoryDatabase()
        {
            return _context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        }
    }
}
