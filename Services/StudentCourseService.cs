using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class StudentCourseService : IStudentCourseService
    {
        private readonly ApplicationDbContext _context;

        public StudentCourseService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static string ResolveThumbnailUrl(DuAnTotNghiep.Models.LearningTopic t)
        {
            var title = t.Title?.ToUpperInvariant() ?? "";
            var code = t.TopicCode?.ToUpperInvariant() ?? "";
            var skillCode = t.Skill?.SkillCode?.ToUpperInvariant() ?? "";

            if (code.Contains("IELTS") || title.Contains("IELTS")) return "/images/courses/ielts.png";
            if (code.Contains("TOEIC") || title.Contains("TOEIC")) return "/images/courses/toeic.png";
            if (skillCode == "GRAMMAR" || title.Contains("NGỮ PHÁP") || title.Contains("THÌ") || title.Contains("TENSES") || title.Contains("PRESENT")) return "/images/courses/grammar.png";
            if (skillCode == "VOCABULARY" || title.Contains("TỪ VỰNG") || title.Contains("FAMILY") || title.Contains("SCHOOL") || title.Contains("TRAVEL")) return "/images/courses/vocabulary.png";
            if (skillCode == "COMMUNICATION" || skillCode == "SPEAKING" || title.Contains("GIAO TIẾP") || title.Contains("SPEAKING") || title.Contains("NÓI")) return "/images/courses/communication.png";
            if (skillCode == "LISTENING" || title.Contains("LISTENING") || title.Contains("NGHE")) return "/images/courses/listening.png";
            if (skillCode == "READING" || title.Contains("READING") || title.Contains("ĐỌC")) return "/images/courses/reading.svg";
            if (skillCode == "WRITING" || title.Contains("WRITING") || title.Contains("VIẾT")) return "/images/courses/writing.svg";

            return "/images/courses/default.svg";
        }

        public async Task<CoursesViewModel> GetCoursesViewModelAsync(int userId, string? category, string? search)
        {
            var topicsQuery = _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .AsNoTracking();

            var topics = await topicsQuery.ToListAsync();

            var courseList = new List<CourseCardViewModel>();
            foreach (var t in topics)
            {
                string thumb = ResolveThumbnailUrl(t);
                int totalLessons = t.OriginalLessons.Count;
                double progressPercent = 0;

                if (totalLessons > 0)
                {
                    var completedLogsCount = await _context.StudyActivityLogs
                        .Where(log => log.StudentId == userId && log.TopicId == t.Id && (log.ActivityType == "LESSON" || log.ActivityType == "ARTICLE"))
                        .Select(log => log.LearningPathNodeId)
                        .Distinct()
                        .CountAsync();

                    progressPercent = Math.Round((double)completedLogsCount / totalLessons * 100, 1);
                    if (progressPercent > 100) progressPercent = 100;
                }

                courseList.Add(new CourseCardViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description ?? "Không có mô tả chi tiết cho khóa học này.",
                    Difficulty = t.DifficultyLevel ?? "BEGINNER",
                    SkillCode = t.Skill?.SkillCode ?? "GENERAL",
                    SkillName = t.Skill?.SkillName ?? "Tổng quát",
                    LessonCount = totalLessons,
                    ProgressPercent = progressPercent,
                    ThumbnailUrl = thumb
                });
            }

            return new CoursesViewModel
            {
                SearchQuery = search ?? "",
                ActiveCategory = (category ?? "ALL").ToUpper(),
                Courses = courseList
            };
        }

        public async Task<int?> GetFirstLessonIdAsync(int topicId)
        {
            var firstLesson = await _context.OriginalLessons
                .Where(l => l.TopicId == topicId)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();

            return firstLesson?.Id;
        }

        public async Task<CourseDetailViewModel?> GetCourseDetailAsync(int topicId, int userId)
        {
            var topic = await _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == topicId);

            if (topic == null) return null;

            var lessons = topic.OriginalLessons.OrderBy(l => l.Id).ToList();

            var completedLessonIds = await _context.StudyActivityLogs
                .Where(log => log.StudentId == userId && log.TopicId == topicId && (log.ActivityType == "LESSON" || log.ActivityType == "ARTICLE"))
                .Select(log => log.LearningPathNodeId ?? 0)
                .Distinct()
                .ToListAsync();

            int totalLessons = lessons.Count;
            int completedCount = completedLessonIds.Count(id => lessons.Any(l => l.Id == id));
            double progressPercent = totalLessons > 0
                ? Math.Min(100, Math.Round((double)completedCount / totalLessons * 100, 1))
                : 0;

            int? firstLessonId = lessons.FirstOrDefault()?.Id;
            int? lastCompletedLessonId = completedLessonIds.Count > 0
                ? lessons.LastOrDefault(l => completedLessonIds.Contains(l.Id))?.Id
                : null;

            var lessonItems = lessons.Select((l, index) => new CourseDetailLessonItem
            {
                Id = l.Id,
                Title = l.Title,
                Summary = l.Summary,
                ContentType = l.ContentType,
                EstimatedMinutes = l.EstimatedMinutes,
                OrderIndex = index + 1,
                IsCompleted = completedLessonIds.Contains(l.Id)
            }).ToList();

            return new CourseDetailViewModel
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description ?? "Không có mô tả chi tiết cho khóa học này.",
                Difficulty = topic.DifficultyLevel ?? "BEGINNER",
                SkillCode = topic.Skill?.SkillCode ?? "GENERAL",
                SkillName = topic.Skill?.SkillName ?? "Tổng quát",
                LevelName = topic.Level?.Name,
                EstimatedMinutes = topic.EstimatedMinutes,
                LessonCount = totalLessons,
                CompletedLessonCount = completedCount,
                ProgressPercent = progressPercent,
                FirstLessonId = firstLessonId,
                LastCompletedLessonId = lastCompletedLessonId,
                ThumbnailUrl = ResolveThumbnailUrl(topic),
                Lessons = lessonItems
            };
        }
    }
}

