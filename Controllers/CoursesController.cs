using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int userId))
        {
            return userId;
        }
        return 0;
    }

    public async Task<IActionResult> Index(string activeCategory = "ALL", string searchQuery = "")
    {
        int userId = GetCurrentUserId();

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
            // Simple visual placeholder for image thumbnails
            string thumb = $"/images/course-{(index % 4) + 1}.png";
            
            // Calc progress
            int totalLessons = t.OriginalLessons.Count;
            double progressPercent = 0;
            if (totalLessons > 0)
            {
                // check completed logs
                var completedLogs = await _context.StudyActivityLogs
                    .Where(log => log.StudentId == userId && log.TopicId == t.Id && (log.ActivityType == "LESSON" || log.ActivityType == "ARTICLE"))
                    .Select(log => log.LearningPathNodeId)
                    .Distinct()
                    .CountAsync();
                
                progressPercent = Math.Round((double)completedLogs / totalLessons * 100, 1);
                if (progressPercent > 100) progressPercent = 100;
            }
            else
            {
                progressPercent = (index % 3 == 0) ? 70 : (index % 3 == 1 ? 45 : 20); // mock variations
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

        var vm = new CoursesViewModel
        {
            SearchQuery = searchQuery,
            ActiveCategory = activeCategory.ToUpper(),
            Courses = courseList
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> StartCourse(int id)
    {
        var firstLesson = await _context.OriginalLessons
            .Where(l => l.TopicId == id)
            .OrderBy(l => l.Id)
            .FirstOrDefaultAsync();

        if (firstLesson == null)
        {
            TempData["ErrorMessage"] = "Khóa học này hiện chưa có bài học nào.";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Detail", "Lesson", new { id = firstLesson.Id });
    }
}
