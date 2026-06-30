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
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
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

        // GET: Teacher/Schedule
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 10)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var query = _context.Schedules
                .Include(s => s.Topic)
                .Where(s => s.TeacherId == teacherId)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.Title.Contains(keyword) || (s.Description != null && s.Description.Contains(keyword)) || (s.Classroom != null && s.Classroom.Contains(keyword)));
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Teacher/Schedule/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title");
            return View();
        }

        // POST: Teacher/Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (ModelState.IsValid)
            {
                schedule.TeacherId = teacherId;
                schedule.CreatedAt = DateTime.UtcNow;
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Lịch giảng dạy đã được thêm thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", schedule.TopicId);
            return View(schedule);
        }

        // GET: Teacher/Schedule/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int teacherId = GetCurrentUserId();
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == id && s.TeacherId == teacherId);
            if (schedule == null)
            {
                return NotFound();
            }

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", schedule.TopicId);
            return View(schedule);
        }

        // POST: Teacher/Schedule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            int teacherId = GetCurrentUserId();
            if (id != schedule.Id || schedule.TeacherId != teacherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Schedules.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.Title = schedule.Title;
                    existing.Description = schedule.Description;
                    existing.TopicId = schedule.TopicId;
                    existing.StartTime = schedule.StartTime;
                    existing.EndTime = schedule.EndTime;
                    existing.Classroom = schedule.Classroom;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật lịch giảng dạy thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
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

            ViewBag.TopicId = new SelectList(await _context.LearningTopics.Where(t => t.Status == "ACTIVE").ToListAsync(), "Id", "Title", schedule.TopicId);
            return View(schedule);
        }

        // GET: Teacher/Schedule/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            int teacherId = GetCurrentUserId();
            var schedule = await _context.Schedules
                .Include(s => s.Topic)
                .FirstOrDefaultAsync(m => m.Id == id && m.TeacherId == teacherId);
            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // POST: Teacher/Schedule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int teacherId = GetCurrentUserId();
            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.Id == id && s.TeacherId == teacherId);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa lịch giảng dạy thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}
