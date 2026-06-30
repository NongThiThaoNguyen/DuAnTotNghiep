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
    public class AssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AssignmentsController(ApplicationDbContext context)
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

        // GET: Teacher/Assignments
        public async Task<IActionResult> Index(string? keyword, int? topicId, int page = 1, int pageSize = 10)
        {
            var query = _context.PracticeTasks
                .Include(t => t.Topic)
                .Include(t => t.Skill)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(t => t.Title.Contains(keyword) || t.Instruction.Contains(keyword));
            }

            if (topicId.HasValue)
            {
                query = query.Where(t => t.TopicId == topicId);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.TopicId = topicId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.Topics = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title");

            return View(items);
        }

        // GET: Teacher/Assignments/Create
        public async Task<IActionResult> Create(int? topicId)
        {
            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", topicId);
            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName");
            return View();
        }

        // POST: Teacher/Assignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PracticeTask task)
        {
            int teacherId = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                task.CreatedAt = DateTime.UtcNow;
                task.CreatedBy = teacherId;
                task.Status = "PUBLISHED";

                _context.Add(task);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Bài tập đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { topicId = task.TopicId });
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", task.TopicId);
            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName", task.SkillId);
            return View(task);
        }

        // GET: Teacher/Assignments/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.PracticeTasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", task.TopicId);
            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName", task.SkillId);
            return View(task);
        }

        // POST: Teacher/Assignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PracticeTask task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.PracticeTasks.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Title = task.Title;
                    existing.Instruction = task.Instruction;
                    existing.TopicId = task.TopicId;
                    existing.SkillId = task.SkillId;
                    existing.TaskType = task.TaskType;
                    existing.DifficultyLevel = task.DifficultyLevel;
                    existing.Status = task.Status;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật bài tập thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PracticeTaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { topicId = task.TopicId });
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", task.TopicId);
            ViewBag.SkillId = new SelectList(await _context.EnglishSkills.Where(s => s.IsActive).ToListAsync(), "Id", "SkillName", task.SkillId);
            return View(task);
        }

        // GET: Teacher/Assignments/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.PracticeTasks
                .Include(t => t.Topic)
                .Include(t => t.Skill)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Teacher/Assignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.PracticeTasks.FindAsync(id);
            if (task != null)
            {
                int topicId = task.TopicId ?? 0;
                _context.PracticeTasks.Remove(task);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa bài tập thành công!";
                return RedirectToAction(nameof(Index), new { topicId = topicId });
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Teacher/Assignments/Submissions
        public async Task<IActionResult> Submissions(int? taskId, string? status, int page = 1, int pageSize = 10)
        {
            var query = _context.PracticeSubmissions
                .Include(s => s.Student)
                .Include(s => s.PracticeTask)
                .AsNoTracking();

            if (taskId.HasValue)
            {
                query = query.Where(s => s.PracticeTaskId == taskId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(s => s.Status == status);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TaskId = taskId;
            ViewBag.Status = status;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.Tasks = new SelectList(await _context.PracticeTasks.ToListAsync(), "Id", "Title");

            return View(items);
        }

        // GET: Teacher/Assignments/Grade/5
        public async Task<IActionResult> Grade(int id)
        {
            var submission = await _context.PracticeSubmissions
                .Include(s => s.Student)
                .Include(s => s.PracticeTask)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        // POST: Teacher/Assignments/Grade/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int id, decimal score, string? teacherFeedback)
        {
            var submission = await _context.PracticeSubmissions.FindAsync(id);
            if (submission == null) return NotFound();

            if (score < 0 || score > 100)
            {
                ModelState.AddModelError("Score", "Điểm số phải nằm trong khoảng từ 0 đến 100.");
                return View(submission);
            }

            submission.Score = score;
            submission.TeacherFeedback = teacherFeedback;
            submission.Status = "TEACHER_REVIEWED";

            _context.Update(submission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Chấm điểm bài nộp của học viên thành công!";
            return RedirectToAction(nameof(Submissions));
        }

        private bool PracticeTaskExists(int id)
        {
            return _context.PracticeTasks.Any(e => e.Id == id);
        }
    }
}
