using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
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
        private readonly ITeacherScheduleService _scheduleService;

        public ScheduleController(ITeacherScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
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
        public async Task<IActionResult> Index(string? keyword, int page = 1, int pageSize = 20, string? weekStart = null)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            // Parse week start for calendar view
            DateTime weekStartDate;
            if (!string.IsNullOrEmpty(weekStart) && DateTime.TryParse(weekStart, out var parsed))
            {
                weekStartDate = parsed.Date;
            }
            else
            {
                var today = DateTime.Today;
                var dow = (int)today.DayOfWeek;
                weekStartDate = today.AddDays(dow == 0 ? -6 : -(dow - 1));
            }
            var weekEndDate = weekStartDate.AddDays(6);

            // Fetch all schedules for this week + paginated list
            var weekItems = await _scheduleService.GetSchedulesByDateRangeAsync(teacherId, weekStartDate, weekEndDate);

            int totalItems = await _scheduleService.GetTotalSchedulesCountAsync(teacherId, keyword);
            var items = await _scheduleService.GetSchedulesAsync(teacherId, keyword, page, pageSize);

            // Merge week items into main model (union, deduplicate by Id)
            var allItems = items.Union(weekItems, new ScheduleIdComparer()).OrderBy(s => s.StartTime).ToList();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.WeekStart = weekStartDate;

            return View(allItems);
        }

        // GET: Teacher/Schedule/Create
        public async Task<IActionResult> Create()
        {
            var activeTopics = await _scheduleService.GetActiveTopicsAsync();
            ViewBag.TopicId = new SelectList(activeTopics, "Id", "Title");

            var now = DateTime.Now;
            var defaultStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
            var defaultSchedule = new Schedule
            {
                StartTime = defaultStart,
                EndTime = defaultStart.AddHours(1)
            };
            return View(defaultSchedule);
        }

        // POST: Teacher/Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            // Assign teacher before validation — navigation properties are not form-bound
            schedule.TeacherId = teacherId;
            ModelState.Remove(nameof(Schedule.Teacher));
            ModelState.Remove(nameof(Schedule.Topic));

            if (ModelState.IsValid)
            {
                schedule.CreatedAt = DateTime.UtcNow;
                await _scheduleService.CreateAsync(schedule);
                TempData["SuccessMessage"] = "Lịch giảng dạy đã được thêm thành công!";
                return RedirectToAction(nameof(Index));
            }

            var activeTopics = await _scheduleService.GetActiveTopicsAsync();
            ViewBag.TopicId = new SelectList(activeTopics, "Id", "Title", schedule.TopicId);
            return View(schedule);
        }

        // GET: Teacher/Schedule/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            int teacherId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(id, teacherId);
            if (schedule == null)
            {
                return NotFound();
            }

            var activeTopics = await _scheduleService.GetActiveTopicsAsync();
            ViewBag.TopicId = new SelectList(activeTopics, "Id", "Title", schedule.TopicId);
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

            // Remove navigation property errors — not form-bound
            ModelState.Remove(nameof(Schedule.Teacher));
            ModelState.Remove(nameof(Schedule.Topic));

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _scheduleService.GetByIdAsync(id, teacherId);
                    if (existing == null) return NotFound();

                    existing.Title = schedule.Title;
                    existing.Description = schedule.Description;
                    existing.TopicId = schedule.TopicId;
                    existing.StartTime = schedule.StartTime;
                    existing.EndTime = schedule.EndTime;
                    existing.Classroom = schedule.Classroom;

                    await _scheduleService.UpdateAsync(existing);
                    TempData["SuccessMessage"] = "Cập nhật lịch giảng dạy thành công!";
                }
                catch (Exception)
                {
                    var existingCheck = await _scheduleService.GetByIdAsync(id, teacherId);
                    if (existingCheck == null)
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

            var activeTopics = await _scheduleService.GetActiveTopicsAsync();
            ViewBag.TopicId = new SelectList(activeTopics, "Id", "Title", schedule.TopicId);
            return View(schedule);
        }

        // GET: Teacher/Schedule/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            int teacherId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(id, teacherId);
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
            var schedule = await _scheduleService.GetByIdAsync(id, teacherId);
            if (schedule != null)
            {
                await _scheduleService.DeleteAsync(schedule);
                TempData["SuccessMessage"] = "Xóa lịch giảng dạy thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>Deduplicates Schedule items by Id when merging week + list queries.</summary>
    internal class ScheduleIdComparer : IEqualityComparer<Schedule>
    {
        public bool Equals(Schedule? x, Schedule? y) => x?.Id == y?.Id;
        public int GetHashCode(Schedule obj) => obj.Id.GetHashCode();
    }
}
