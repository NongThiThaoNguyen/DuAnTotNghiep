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
    public class QuizzesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizzesController(ApplicationDbContext context)
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

        // GET: Teacher/Quizzes
        public async Task<IActionResult> Index(string? keyword, int? topicId, int page = 1, int pageSize = 10)
        {
            var query = _context.Quizzes
                .Include(q => q.Topic)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(q => q.Title.Contains(keyword) || (q.Description != null && q.Description.Contains(keyword)));
            }

            if (topicId.HasValue)
            {
                query = query.Where(q => q.TopicId == topicId);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(q => q.CreatedAt)
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

        // GET: Teacher/Quizzes/Create
        public async Task<IActionResult> Create(int? topicId)
        {
            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", topicId);
            return View();
        }

        // POST: Teacher/Quizzes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Quiz quiz)
        {
            int teacherId = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                var topic = await _context.LearningTopics.FindAsync(quiz.TopicId);
                quiz.SkillId = topic?.SkillId ?? 1;
                quiz.CreatedAt = DateTime.UtcNow;
                quiz.CreatedBy = teacherId;
                quiz.Status = "PUBLISHED"; // Teacher created is automatically published

                _context.Add(quiz);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Quiz đã được tạo thành công!";
                return RedirectToAction(nameof(Index), new { topicId = quiz.TopicId });
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", quiz.TopicId);
            return View(quiz);
        }

        // GET: Teacher/Quizzes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", quiz.TopicId);
            return View(quiz);
        }

        // POST: Teacher/Quizzes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Quizzes.FindAsync(id);
                    if (existing == null) return NotFound();

                    var topic = await _context.LearningTopics.FindAsync(quiz.TopicId);
                    existing.SkillId = topic?.SkillId ?? 1;

                    existing.Title = quiz.Title;
                    existing.Description = quiz.Description;
                    existing.TopicId = quiz.TopicId;
                    existing.QuizType = quiz.QuizType;
                    existing.TimeLimitMinutes = quiz.TimeLimitMinutes;
                    existing.PassingScore = quiz.PassingScore;
                    existing.Status = quiz.Status;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật Quiz thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { topicId = quiz.TopicId });
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", quiz.TopicId);
            return View(quiz);
        }

        // GET: Teacher/Quizzes/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Topic)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Teacher/Quizzes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                int topicId = quiz.TopicId ?? 0;
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa Quiz thành công!";
                return RedirectToAction(nameof(Index), new { topicId = topicId });
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Teacher/Quizzes/QuestionBank
        public async Task<IActionResult> QuestionBank(string? keyword, int? quizId, int page = 1, int pageSize = 10)
        {
            var query = _context.QuizQuestions
                .Include(q => q.Quiz)
                .Include(q => q.Question)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(q => q.Question.QuestionText.Contains(keyword) || (q.Question.Explanation != null && q.Question.Explanation.Contains(keyword)));
            }

            if (quizId.HasValue)
            {
                query = query.Where(q => q.QuizId == quizId);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(q => q.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.QuizId = quizId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            ViewBag.QuizzesList = new SelectList(await _context.Quizzes.ToListAsync(), "Id", "Title");

            return View(items);
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }
    }
}
