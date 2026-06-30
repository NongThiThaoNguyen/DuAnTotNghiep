using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private readonly IStudentCourseService _courseService;

        public CoursesController(IStudentCourseService courseService)
        {
            _courseService = courseService;
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
            if (userId == 0) return RedirectToAction("Login", "Account");

            var vm = await _courseService.GetCoursesViewModelAsync(userId, activeCategory, searchQuery);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> StartCourse(int id)
        {
            var firstLessonId = await _courseService.GetFirstLessonIdAsync(id);

            if (firstLessonId == null)
            {
                TempData["ErrorMessage"] = "Khóa học này hiện chưa có bài học nào.";
                return RedirectToAction("Index");
            }

            return RedirectToAction("Detail", "Lesson", new { id = firstLessonId.Value });
        }
    }
}
