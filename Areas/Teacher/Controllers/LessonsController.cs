using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonsController(ApplicationDbContext context)
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

        // GET: Teacher/Lessons
        public async Task<IActionResult> Index(string? keyword, int? topicId, string? contentType, int page = 1, int pageSize = 10)
        {
            var query = _context.OriginalLessons
                .Include(l => l.Topic)
                .AsNoTracking();

            // Search
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(l => l.Title.Contains(keyword) || (l.Summary != null && l.Summary.Contains(keyword)));
            }

            // Filters
            if (topicId.HasValue)
            {
                query = query.Where(l => l.TopicId == topicId);
            }
            if (!string.IsNullOrEmpty(contentType))
            {
                query = query.Where(l => l.ContentType == contentType);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.TopicId = topicId;
            ViewBag.ContentType = contentType;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Dropdowns
            ViewBag.Topics = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title");

            return View(items);
        }

        // GET: Teacher/Lessons/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var lesson = await _context.OriginalLessons
                .Include(l => l.Topic)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // GET: Teacher/Lessons/Create
        public async Task<IActionResult> Create(int? topicId)
        {
            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", topicId);
            return View();
        }

        // POST: Teacher/Lessons/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OriginalLesson lesson)
        {
            int teacherId = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                lesson.CreatedAt = DateTime.UtcNow;
                lesson.UpdatedAt = DateTime.UtcNow;
                lesson.SourceType = "TEACHER_CREATED";
                lesson.ReviewStatus = "APPROVED"; // Teacher created is automatically approved
                lesson.CreatedBy = teacherId;
                lesson.IsAiGenerated = false;

                _context.Add(lesson);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bài học đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { topicId = lesson.TopicId });
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", lesson.TopicId);
            return View(lesson);
        }

        // GET: Teacher/Lessons/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _context.OriginalLessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", lesson.TopicId);
            return View(lesson);
        }

        // POST: Teacher/Lessons/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OriginalLesson lesson)
        {
            if (id != lesson.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.OriginalLessons.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Title = lesson.Title;
                    existing.Summary = lesson.Summary;
                    existing.Content = lesson.Content;
                    existing.TopicId = lesson.TopicId;
                    existing.ContentType = lesson.ContentType;
                    existing.EstimatedMinutes = lesson.EstimatedMinutes;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LessonExists(lesson.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { topicId = lesson.TopicId });
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", lesson.TopicId);
            return View(lesson);
        }

        // GET: Teacher/Lessons/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var lesson = await _context.OriginalLessons
                .Include(l => l.Topic)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(lesson);
        }

        // POST: Teacher/Lessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.OriginalLessons.FindAsync(id);
            if (lesson != null)
            {
                int topicId = lesson.TopicId;
                _context.OriginalLessons.Remove(lesson);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài học thành công!";
                return RedirectToAction(nameof(Index), new { topicId = topicId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool LessonExists(int id)
        {
            return _context.OriginalLessons.Any(e => e.Id == id);
        }
    }
}
