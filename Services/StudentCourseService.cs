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

        public async Task<CoursesViewModel> GetCoursesViewModelAsync(int userId, string? category, string? search)
        {
            var topicsQuery = _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .AsNoTracking();

            var topics = await topicsQuery.ToListAsync();

            var courseList = new List<CourseCardViewModel>();
            int index = 0;
            foreach (var t in topics)
            {
                string thumb = $"/images/course-{(index % 4) + 1}.png";
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
                else
                {
                    progressPercent = (index % 3 == 0) ? 70 : (index % 3 == 1 ? 45 : 20);
                }

                courseList.Add(new CourseCardViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description ?? "Không có mô tả chi tiết cho khóa học này.",
                    Difficulty = t.DifficultyLevel ?? "BEGINNER",
                    SkillCode = t.Skill?.SkillCode ?? "GENERAL",
                    SkillName = t.Skill?.SkillName ?? "Tổng quát",
                    LessonCount = totalLessons > 0 ? totalLessons : 10,
                    ProgressPercent = progressPercent,
                    ThumbnailUrl = thumb
                });

                index++;
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
    }
}
