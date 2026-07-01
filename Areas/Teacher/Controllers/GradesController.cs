using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class GradesController : Controller
    {
        private readonly ITeacherGradingService _gradingService;

        public GradesController(ITeacherGradingService gradingService)
        {
            _gradingService = gradingService;
        }

        public async Task<IActionResult> Index(int? topicId)
        {
            ViewBag.TopicId = topicId;
            return View(await _gradingService.GetGradesOverviewAsync(topicId));
        }

        public async Task<IActionResult> Pending()
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            return View(await _gradingService.GetPendingSubmissionsAsync(teacherId));
        }

        public async Task<IActionResult> Grade(int id)
        {
            var submission = await _gradingService.GetSubmissionDetailAsync(id);
            if (submission == null)
            {
                return NotFound();
            }

            return View(submission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(int id, decimal score, string? feedback)
        {
            var teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            await _gradingService.GradeSubmissionAsync(id, score, feedback, teacherId);
            TempData["SuccessMessage"] = "Chấm bài thành công!";
            return RedirectToAction(nameof(Pending));
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
        }
    }
}
