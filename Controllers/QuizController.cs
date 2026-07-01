using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly IStudentQuizService _quizService;

        public QuizController(IStudentQuizService quizService)
        {
            _quizService = quizService;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var quizzes = await _quizService.GetAllQuizzesAsync(userId);
            return View(quizzes);
        }

        [HttpGet]
        public async Task<IActionResult> Take(int id) // id is course/topic ID
        {
            var vm = await _quizService.GetQuizForTakingAsync(id);

            if (vm == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài trắc nghiệm cho khóa học này.";
                return RedirectToAction("Index", "Courses");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int quizId, Dictionary<int, string> answers)
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            try
            {
                var result = await _quizService.SubmitQuizAsync(quizId, userId, answers);

                ViewBag.Score = result.Score;
                ViewBag.CorrectCount = result.CorrectCount;
                ViewBag.TotalCount = result.TotalCount;
                ViewBag.Answers = answers;

                var vm = await _quizService.GetQuizByIdAsync(quizId);
                if (vm == null) return NotFound();

                return View("Result", vm);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
