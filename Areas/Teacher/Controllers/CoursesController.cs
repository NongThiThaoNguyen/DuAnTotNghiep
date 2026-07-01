using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class CoursesController : Controller
    {
        private readonly ITeacherCourseService _courseService;
        private readonly IEnglishSkillService _skillService;
        private readonly IEnglishProficiencyLevelService _levelService;

        public CoursesController(
            ITeacherCourseService courseService,
            IEnglishSkillService skillService,
            IEnglishProficiencyLevelService levelService)
        {
            _courseService = courseService;
            _skillService = skillService;
            _levelService = levelService;
        }

        public async Task<IActionResult> Index(string? search, string? skill)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var model = new CourseIndexViewModel
            {
                Search = search,
                Skill = skill,
                Courses = await _courseService.GetCoursesAsync(teacherId, search, skill),
                SkillOptions = await _skillService.GetOptionsAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseDetailAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel
            {
                SkillOptions = await _skillService.GetOptionsAsync(),
                LevelOptions = await _levelService.GetOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                await PopulateCourseOptionsAsync(model);
                return View(model);
            }

            await _courseService.CreateCourseAsync(model, teacherId);
            TempData["SuccessMessage"] = "Khóa học đã được tạo thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseDetailAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var model = new EditCourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                TopicCode = course.TopicCode,
                SkillId = course.SkillId,
                ProficiencyLevelId = course.ProficiencyLevelId,
                DifficultyLevel = course.DifficultyLevel,
                EstimatedMinutes = course.EstimatedMinutes,
                OrderIndex = course.OrderIndex,
                IsActive = course.Status == "ACTIVE",
                SkillOptions = await _skillService.GetOptionsAsync(),
                LevelOptions = await _levelService.GetOptionsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCourseViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateCourseOptionsAsync(model);
                return View(model);
            }

            await _courseService.UpdateCourseAsync(model);
            TempData["SuccessMessage"] = "Cập nhật khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            TempData["SuccessMessage"] = "Xóa khóa học thành công!";
            return RedirectToAction(nameof(Index));
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }

        private async Task PopulateCourseOptionsAsync(CreateCourseViewModel model)
        {
            model.SkillOptions = await _skillService.GetOptionsAsync();
            model.LevelOptions = await _levelService.GetOptionsAsync();
        }
    }
}
