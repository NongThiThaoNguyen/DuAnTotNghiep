using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class LessonsController : Controller
    {
        private readonly ITeacherLessonService _lessonService;
        private readonly ITeacherCourseService _courseService;

        public LessonsController(
            ITeacherLessonService lessonService,
            ITeacherCourseService courseService)
        {
            _lessonService = lessonService;
            _courseService = courseService;
        }

        public async Task<IActionResult> Index(int courseId)
        {
            if (courseId <= 0)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetCourseDetailAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            var model = new LessonIndexViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Lessons = await _lessonService.GetLessonsByTopicAsync(courseId)
            };

            return View(model);
        }

        public async Task<IActionResult> Create(int courseId)
        {
            if (courseId <= 0)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetCourseDetailAsync(courseId);
            if (course == null)
            {
                return NotFound();
            }

            return View(new CreateLessonViewModel
            {
                TopicId = courseId,
                TopicTitle = course.Title,
                DifficultyLevel = course.DifficultyLevel,
                EstimatedMinutes = 15
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLessonViewModel model)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                await PopulateTopicTitleAsync(model);
                return View(model);
            }

            await _lessonService.CreateLessonAsync(model, teacherId);
            TempData["SuccessMessage"] = "Bài học đã được tạo thành công!";
            return RedirectToAction(nameof(Index), new { courseId = model.TopicId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lesson = await _lessonService.GetLessonDetailAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View(new EditLessonViewModel
            {
                Id = lesson.Id,
                TopicId = lesson.TopicId,
                TopicTitle = lesson.TopicTitle,
                Title = lesson.Title,
                Summary = lesson.Summary,
                Content = lesson.Content,
                EstimatedMinutes = lesson.EstimatedMinutes,
                DifficultyLevel = "BASIC"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditLessonViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateTopicTitleAsync(model);
                return View(model);
            }

            await _lessonService.UpdateLessonAsync(model);
            TempData["SuccessMessage"] = "Cập nhật bài học thành công!";
            return RedirectToAction(nameof(Index), new { courseId = model.TopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int courseId)
        {
            await _lessonService.DeleteLessonAsync(id);
            TempData["SuccessMessage"] = "Xóa bài học thành công!";
            return RedirectToAction(nameof(Index), new { courseId });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var lesson = await _lessonService.GetLessonDetailAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return View("Detail", lesson);
        }

        public Task<IActionResult> Details(int id)
        {
            return Detail(id);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }

        private async Task PopulateTopicTitleAsync(CreateLessonViewModel model)
        {
            var course = await _courseService.GetCourseDetailAsync(model.TopicId);
            model.TopicTitle = course?.Title ?? string.Empty;
        }
    }
}
