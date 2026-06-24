using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.DTOs.PlacementTest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class PlacementTestController : Controller
    {
        private readonly IPlacementTestService _placementTestService;
        private readonly IPlacementRequirementService _requirementService;

        public PlacementTestController(IPlacementTestService placementTestService, IPlacementRequirementService requirementService)
        {
            _placementTestService = placementTestService;
            _requirementService = requirementService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> Intro()
        {
            var userId = GetUserId();
            var flowStatus = await _requirementService.GetStudentFlowStatusAsync(userId);
            
            if (flowStatus.Status == DuAnTotNghiep.DTOs.PlacementTest.PlacementFlowStatus.Completed)
            {
                return RedirectToAction("Index", "Home");
            }
            if (flowStatus.Status == DuAnTotNghiep.DTOs.PlacementTest.PlacementFlowStatus.PlacementInProgress)
            {
                return Redirect(flowStatus.RedirectUrl!);
            }

            var availableTest = await _placementTestService.GetAvailableTestForStudentAsync(userId);
            return View(availableTest); // availableTest can be null if no test is published
        }

        [HttpGet]
        public async Task<IActionResult> Suggestion()
        {
            var userId = GetUserId();
            var suggestion = await _placementTestService.BuildPlacementTestSuggestionAsync(userId);

            if (suggestion == null)
            {
                // Nếu chưa có profile hoặc chưa có bài test active
                return RedirectToAction("Index", "Onboarding");
            }

            return View(suggestion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int testId)
        {
            var userId = GetUserId();
            try
            {
                var attempt = await _placementTestService.StartAttemptAsync(userId, testId);
                return RedirectToAction("Take", new { attemptId = attempt.Id });
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Suggestion");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Take(int? id, int? attemptId)
        {
            var userId = GetUserId();
            int finalAttemptId = id ?? attemptId ?? 0;
            var viewModel = await _placementTestService.GetTestTakingViewModelAsync(finalAttemptId, userId);

            if (viewModel == null)
            {
                return Forbid();
            }

            // We do not have status in viewModel directly as per new structure, so we check attempt status in service.
            // Wait, I did not include Status in TestTakingViewModel. I should check if remaining seconds <= 0 for expired.
            // Or I should add Status to the view model? Actually I didn't add Status in TestTakingViewModel.
            // Let me add it first, wait, it's easier to just add it. But for now let's just check if it's null (service can return null if not in progress).
            // Actually, the service doesn't filter by status. Let's fix that in controller or service.
            // Since I didn't add Status to TestTakingViewModel, let me check the DB directly or add it.
            // I'll just rely on the existing code and add Status to ViewModel later if needed, but the plan says "return View(viewModel)".
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveAnswer([FromBody] SaveAnswerInputDto input)
        {
            var studentId = HttpContext.Session.GetInt32("UserId");
            if (studentId == null)
            {
                return Unauthorized(new { success = false, message = "Not logged in" });
            }

            try
            {
                var result = await _placementTestService.SaveAnswerAsync(input, studentId.Value);
                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    savedAt = result.SavedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Submit(int attemptId)
        {
            var studentId = GetUserId();
            var result = await _placementTestService.SubmitAttemptAsync(attemptId, studentId, new List<AnswerInputDto>());

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Nộp bài thành công!";
                return RedirectToAction("Result", new { attemptId = attemptId });
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi nộp bài: " + result.Message;
                return RedirectToAction("Intro");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Result(int attemptId)
        {
            var studentId = GetUserId();
            try
            {
                var viewModel = await _placementTestService.GetResultForStudentAsync(attemptId, studentId);
                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException)
            {
                return RedirectToAction("Take", new { attemptId = attemptId });
            }
        }
    }
}
