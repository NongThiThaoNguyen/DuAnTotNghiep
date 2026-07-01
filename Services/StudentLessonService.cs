using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class StudentLessonService : IStudentLessonService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGamificationService _gamificationService;

        public StudentLessonService(ApplicationDbContext context, IGamificationService gamificationService)
        {
            _context = context;
            _gamificationService = gamificationService;
        }

        public async Task<LessonViewModel?> GetLessonDetailAsync(int lessonId, int userId)
        {
            var lesson = await _context.OriginalLessons
                .Include(l => l.Topic)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return null;

            var siblings = await _context.OriginalLessons
                .Where(l => l.TopicId == lesson.TopicId)
                .OrderBy(l => l.Id)
                .ToListAsync();

            var completedLessonIds = await _context.StudyActivityLogs
                .Where(log => log.StudentId == userId && log.TopicId == lesson.TopicId && (log.ActivityType == "LESSON" || log.ActivityType == "ARTICLE"))
                .Select(log => log.LearningPathNodeId ?? 0)
                .Distinct()
                .ToListAsync();

            var lessonNav = siblings.Select((sib, index) => new LessonNavigationItemViewModel
            {
                Id = sib.Id,
                Title = sib.Title,
                OrderIndex = index + 1,
                IsCurrent = sib.Id == lessonId,
                IsCompleted = completedLessonIds.Contains(sib.Id)
            }).ToList();

            int currentIndex = siblings.FindIndex(s => s.Id == lessonId);
            int? prevId = currentIndex > 0 ? siblings[currentIndex - 1].Id : null;
            int? nextId = currentIndex < siblings.Count - 1 ? siblings[currentIndex + 1].Id : null;

            var isCompleted = completedLessonIds.Contains(lessonId);

            string? videoUrl = lesson.VideoUrl;
            if (lesson.ContentType == "VIDEO_LINK" && string.IsNullOrEmpty(videoUrl))
            {
                videoUrl = lesson.Content; // Fallback to old behavior
            }

            return new LessonViewModel
            {
                CourseId = lesson.TopicId,
                CourseTitle = lesson.Topic?.Title ?? "Khóa học",
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                Summary = lesson.Summary ?? "Tóm tắt bài học tiếng Anh hữu ích dành cho bạn.",
                Content = lesson.Content ?? "<p>Nội dung đang được cập nhật...</p>",
                ContentType = lesson.ContentType,
                VideoUrl = videoUrl,
                EstimatedMinutes = lesson.EstimatedMinutes ?? 15,
                IsCompleted = isCompleted,
                LessonsInCourse = lessonNav,
                PreviousLessonId = prevId,
                NextLessonId = nextId
            };
        }

        public async Task<bool> MarkLessonCompletedAsync(int lessonId, int userId)
        {
            var lesson = await _context.OriginalLessons.FindAsync(lessonId);
            if (lesson == null) return false;

            var existingLog = await _context.StudyActivityLogs
                .FirstOrDefaultAsync(l => l.StudentId == userId && l.TopicId == lesson.TopicId && l.LearningPathNodeId == lessonId && l.ActivityType == "LESSON");

            if (existingLog == null)
            {
                var log = new StudyActivityLog
                {
                    StudentId = userId,
                    ActivityType = "LESSON",
                    TopicId = lesson.TopicId,
                    LearningPathNodeId = lessonId,
                    DurationMinutes = lesson.EstimatedMinutes ?? 15,
                    CreatedAt = DateTime.UtcNow
                };
                _context.StudyActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            // Gamification hook
            await _gamificationService.CheckAndGrantAchievementsAsync(userId);

            return true;
        }
    }
}
