using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teacher/Courses
        public async Task<IActionResult> Index(string? keyword, int? skillId, int? levelId, string? difficulty, string? status, int page = 1, int pageSize = 6)
        {
            var query = _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .AsNoTracking();

            // Search
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.Title.Contains(keyword) || (t.Description != null && t.Description.Contains(keyword)) || (t.TopicCode != null && t.TopicCode.Contains(keyword)));
            }

            // Filters
            if (skillId.HasValue)
            {
                query = query.Where(t => t.SkillId == skillId);
            }
            if (levelId.HasValue)
            {
                query = query.Where(t => t.LevelId == levelId);
            }
            if (!string.IsNullOrEmpty(difficulty))
            {
                query = query.Where(t => t.DifficultyLevel == difficulty);
            }
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.OrderIndex)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.SkillId = skillId;
            ViewBag.LevelId = levelId;
            ViewBag.Difficulty = difficulty;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Dropdowns
            ViewBag.Skills = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName");
            ViewBag.Levels = new SelectList(await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).ToListAsync(), "Id", "Name");

            return View(items);
        }

        // GET: Teacher/Courses/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.OriginalLessons)
                .Include(t => t.Quizzes)
                .Include(t => t.PracticeTasks)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Teacher/Courses/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName");
            ViewBag.LevelId = new SelectList(await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Teacher/Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LearningTopic topic)
        {
            if (ModelState.IsValid)
            {
                topic.CreatedAt = DateTime.UtcNow;
                topic.UpdatedAt = DateTime.UtcNow;
                topic.Status = "ACTIVE";
                _context.Add(topic);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Khóa học đã được tạo thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName", topic.SkillId);
            ViewBag.LevelId = new SelectList(await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).ToListAsync(), "Id", "Name", topic.LevelId);
            return View(topic);
        }

        // GET: Teacher/Courses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.LearningTopics.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName", course.SkillId);
            ViewBag.LevelId = new SelectList(await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).ToListAsync(), "Id", "Name", course.LevelId);
            return View(course);
        }

        // POST: Teacher/Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LearningTopic topic)
        {
            if (id != topic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.LearningTopics.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Title = topic.Title;
                    existing.Description = topic.Description;
                    existing.SkillId = topic.SkillId;
                    existing.LevelId = topic.LevelId;
                    existing.TopicCode = topic.TopicCode;
                    existing.DifficultyLevel = topic.DifficultyLevel;
                    existing.OrderIndex = topic.OrderIndex;
                    existing.EstimatedMinutes = topic.EstimatedMinutes;
                    existing.Status = topic.Status;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(topic.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName", topic.SkillId);
            ViewBag.LevelId = new SelectList(await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).ToListAsync(), "Id", "Name", topic.LevelId);
            return View(topic);
        }

        // GET: Teacher/Courses/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.LearningTopics
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Teacher/Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.LearningTopics.FindAsync(id);
            if (course != null)
            {
                _context.LearningTopics.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa khóa học thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.LearningTopics.Any(e => e.Id == id);
        }
    }
}
