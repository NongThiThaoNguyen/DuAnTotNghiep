using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        private readonly IStudentLessonService _lessonService;
        private readonly ApplicationDbContext _context;

        public LessonController(IStudentLessonService lessonService, ApplicationDbContext context)
        {
            _lessonService = lessonService;
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

            var vm = await _lessonService.GetLessonDetailAsync(id, userId);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var success = await _lessonService.MarkLessonCompletedAsync(id, userId);
            if (!success) return NotFound();

            var lesson = await _context.OriginalLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            // Find next sibling lesson in the topic
            var nextLesson = await _context.OriginalLessons
                .Where(l => l.TopicId == lesson.TopicId && l.Id > id)
                .OrderBy(l => l.Id)
                .FirstOrDefaultAsync();

            if (nextLesson != null)
            {
                return RedirectToAction("Detail", new { id = nextLesson.Id });
            }

            TempData["SuccessMessage"] = "Chúc mừng! Bạn đã hoàn thành toàn bộ bài học trong khóa học này!";
            return RedirectToAction("Index", "Courses");
        }
    }
}
