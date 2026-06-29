using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class ReferenceReviewController : Controller
    {
        private readonly IReferenceSourceService _referenceService;

        public ReferenceReviewController(IReferenceSourceService referenceService)
        {
            _referenceService = referenceService;
        }

        [HttpGet("/Admin/ReferenceReview/Pending")]
        [HttpGet("/review/pending")]
        public async Task<IActionResult> Pending()
        {
            var result = await _referenceService.GetPagedListAsync(null, null, ReferenceReviewStatus.PENDING, 1, 100);
            return View(result.Items);
        }

        [HttpGet("/Admin/ReferenceReview/Details/{id}")]
        [HttpGet("/review/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _referenceService.GetDetailsViewModelAsync(id, GetCurrentUserId(), "ADMIN");
            if (model == null) return NotFound();

            return View("Review", model);
        }

        [HttpPost("/Admin/ReferenceReview/Approve")]
        [HttpPost("/review/approve")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string? note)
        {
            try
            {
                await _referenceService.ApproveAsync(id, GetCurrentUserId(), "ADMIN", note);
                TempData["SuccessMessage"] = "Phê duyệt nguồn tham khảo thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Pending));
        }

        [HttpPost("/Admin/ReferenceReview/Reject")]
        [HttpPost("/review/reject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Lý do từ chối không được để trống!";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                await _referenceService.RejectAsync(id, reason, GetCurrentUserId(), "ADMIN");
                TempData["SuccessMessage"] = "Từ chối nguồn tham khảo thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Pending));
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out var id) ? id : 0;
        }
    }
}
