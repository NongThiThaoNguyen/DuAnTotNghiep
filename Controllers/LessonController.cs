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
public class LessonController : Controller
{
    private readonly ApplicationDbContext _context;

    public LessonController(ApplicationDbContext context)
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

    public async Task<IActionResult> Detail(int id)
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

        var lesson = await _context.OriginalLessons
            .Include(l => l.Topic)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null) return NotFound();

        // Get sibling lessons in course
        var siblings = await _context.OriginalLessons
            .Where(l => l.TopicId == lesson.TopicId)
            .OrderBy(l => l.Id)
            .ToListAsync();

        var completedLessonIds = await _context.StudyActivityLogs
            .Where(log => log.StudentId == userId && log.TopicId == lesson.TopicId && (log.ActivityType == "LESSON" || log.ActivityType == "ARTICLE"))
            .Select(log => log.LearningPathNodeId ?? 0) // reusing node ID or map it to lesson ID
            .Distinct()
            .ToListAsync();

        var lessonNav = siblings.Select((sib, index) => new LessonNavigationItemViewModel
        {
            Id = sib.Id,
            Title = sib.Title,
            OrderIndex = index + 1,
            IsCurrent = sib.Id == id,
            IsCompleted = completedLessonIds.Contains(sib.Id)
        }).ToList();

        // Sibling navigation
        int currentIndex = siblings.FindIndex(s => s.Id == id);
        int? prevId = currentIndex > 0 ? siblings[currentIndex - 1].Id : null;
        int? nextId = currentIndex < siblings.Count - 1 ? siblings[currentIndex + 1].Id : null;

        var isCompleted = completedLessonIds.Contains(id);

        var vm = new LessonViewModel
        {
            CourseId = lesson.TopicId,
            CourseTitle = lesson.Topic?.Title ?? "Khóa học",
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Summary = lesson.Summary ?? "Tóm tắt bài học tiếng Anh hữu ích dành cho bạn.",
            Content = lesson.Content ?? "<p>Nội dung đang được cập nhật...</p>",
            EstimatedMinutes = lesson.EstimatedMinutes ?? 15,
            IsCompleted = isCompleted,
            LessonsInCourse = lessonNav,
            PreviousLessonId = prevId,
            NextLessonId = nextId
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkCompleted(int id)
    {
        int userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Account");

        var lesson = await _context.OriginalLessons.FindAsync(id);
        if (lesson == null) return NotFound();

        // Check if log already exists
        var existingLog = await _context.StudyActivityLogs
            .FirstOrDefaultAsync(l => l.StudentId == userId && l.TopicId == lesson.TopicId && l.LearningPathNodeId == id && l.ActivityType == "LESSON");

        if (existingLog == null)
        {
            var log = new StudyActivityLog
            {
                StudentId = userId,
                ActivityType = "LESSON",
                TopicId = lesson.TopicId,
                LearningPathNodeId = id, // map directly to lesson ID for this view
                DurationMinutes = lesson.EstimatedMinutes ?? 15,
                CreatedAt = DateTime.UtcNow
            };
            _context.StudyActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // Get sibling lessons to find the next one
        var siblings = await _context.OriginalLessons
            .Where(l => l.TopicId == lesson.TopicId)
            .OrderBy(l => l.Id)
            .ToListAsync();

        int currentIndex = siblings.FindIndex(s => s.Id == id);
        if (currentIndex < siblings.Count - 1)
        {
            return RedirectToAction("Detail", new { id = siblings[currentIndex + 1].Id });
        }

        TempData["SuccessMessage"] = "Chúc mừng! Bạn đã hoàn thành toàn bộ bài học trong khóa học này!";
        return RedirectToAction("Index", "Courses");
    }
}
