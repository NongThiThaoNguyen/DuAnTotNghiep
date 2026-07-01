using DuAnTotNghiep.Models.ViewModels.Teacher;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class QuizzesController : Controller
    {
        private readonly ITeacherQuizService _quizService;
        private readonly ITeacherCourseService _courseService;

        public QuizzesController(ITeacherQuizService quizService, ITeacherCourseService courseService)
        {
            _quizService = quizService;
            _courseService = courseService;
        }

        public async Task<IActionResult> Index(int topicId)
        {
            if (topicId <= 0)
            {
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetCourseDetailAsync(topicId);
            if (course == null)
            {
                return NotFound();
            }

            return View(new QuizIndexViewModel
            {
                TopicId = topicId,
                TopicTitle = course.Title,
                Quizzes = await _quizService.GetQuizzesByTopicAsync(topicId)
            });
        }

        public async Task<IActionResult> Create(int topicId)
        {
            var course = await _courseService.GetCourseDetailAsync(topicId);
            if (course == null)
            {
                return NotFound();
            }

            return View(new CreateQuizViewModel
            {
                TopicId = topicId,
                TopicTitle = course.Title,
                Questions =
                {
                    new QuizQuestionFormViewModel { Options = { "", "", "", "" } }
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
            {
                await PopulateTopicTitleAsync(model);
                return View(model);
            }

            await _quizService.CreateQuizAsync(model, teacherId);
            TempData["SuccessMessage"] = "Quiz đã được tạo thành công!";
            return RedirectToAction(nameof(Index), new { topicId = model.TopicId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            var course = await _courseService.GetCourseDetailAsync(quiz.TopicId);
            return View(new EditQuizViewModel
            {
                Id = quiz.Id,
                TopicId = quiz.TopicId,
                TopicTitle = course?.Title ?? string.Empty,
                Title = quiz.Title,
                Description = quiz.Description,
                TimeLimitMinutes = quiz.TimeLimitMinutes,
                PassScore = quiz.PassScore,
                Status = quiz.Status,
                Questions = quiz.Questions
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditQuizViewModel model)
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

            await _quizService.UpdateQuizAsync(model);
            TempData["SuccessMessage"] = "Cập nhật Quiz thành công!";
            return RedirectToAction(nameof(Index), new { topicId = model.TopicId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int topicId)
        {
            await _quizService.DeleteQuizAsync(id);
            TempData["SuccessMessage"] = "Xóa Quiz thành công!";
            return RedirectToAction(nameof(Index), new { topicId });
        }

        public async Task<IActionResult> Results(int id)
        {
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            ViewBag.QuizTitle = quiz.Title;
            ViewBag.TopicId = quiz.TopicId;
            return View(await _quizService.GetQuizAttemptsAsync(id));
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }

        private async Task PopulateTopicTitleAsync(CreateQuizViewModel model)
        {
            var course = await _courseService.GetCourseDetailAsync(model.TopicId);
            model.TopicTitle = course?.Title ?? string.Empty;
        }
    }
}
