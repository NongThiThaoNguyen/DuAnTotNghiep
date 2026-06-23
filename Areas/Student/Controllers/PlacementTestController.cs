using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class PlacementTestController : Controller
    {
        private readonly IPlacementTestService _placementTestService;

        public PlacementTestController(IPlacementTestService placementTestService)
        {
            _placementTestService = placementTestService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
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
        public IActionResult Start(int suggestedTestId)
        {
            // Bắt đầu bài thi thật sẽ được implement ở Task tiếp theo
            TempData["SuccessMessage"] = $"Đã xác nhận bài kiểm tra (ID: {suggestedTestId}). Tính năng làm bài đang được phát triển.";
            return RedirectToAction("Suggestion");
        }
    }
}
