using DuAnTotNghiep.Models;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.ViewModels.Admin.ReferenceSources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class ReferenceSourcesController : Controller
    {
        private readonly IReferenceSourceService _referenceService;

        public ReferenceSourcesController(IReferenceSourceService referenceService)
        {
            _referenceService = referenceService;
        }

        public async Task<IActionResult> Index(string? sourceType, string? status, string? keyword, int page = 1, int pageSize = 10)
        {
            ReferenceSourceType? typeEnum = null;
            if (!string.IsNullOrEmpty(sourceType))
            {
                if (!Enum.TryParse<ReferenceSourceType>(sourceType, out var parsedType))
                {
                    ModelState.AddModelError(nameof(sourceType), "Loại nguồn tham khảo không hợp lệ.");
                }
                else
                {
                    typeEnum = parsedType;
                }
            }

            ReferenceReviewStatus? statusEnum = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<ReferenceReviewStatus>(status, out var parsedStatus))
                {
                    ModelState.AddModelError(nameof(status), "Trạng thái kiểm duyệt không hợp lệ.");
                }
                else
                {
                    statusEnum = parsedStatus;
                }
            }

            ViewBag.Keyword = keyword;
            ViewBag.SourceType = sourceType;
            ViewBag.Status = status;
            ViewBag.SourceTypes = ReferenceHelper.GetSourceTypeSelectList(typeEnum);
            ViewBag.Statuses = ReferenceHelper.GetReviewStatusSelectList(statusEnum);

            if (!ModelState.IsValid)
            {
                // Return empty result view with validation errors
                return View(new ReferenceSourcePagedViewModel
                {
                    CurrentPage = 1,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                });
            }

            var result = await _referenceService.GetPagedListAsync(keyword, typeEnum, statusEnum, page, pageSize);
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var role = GetCurrentUserRole();
                var source = await _referenceService.GetDetailsViewModelAsync(id, GetCurrentUserId(), role);
                if (source == null) return NotFound();

                return View(source);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403);
            }
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            var model = new CreateReferenceSourceViewModel
            {
                SourceType = ReferenceSourceType.REFERENCE_ONLY,
                UsagePolicy = ReferenceUsagePolicy.REFERENCE_ONLY
            };

            PopulateDropdowns();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateReferenceSourceViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var source = new ReferenceSource
                    {
                        SourceName = model.SourceName,
                        SourceUrl = model.SourceUrl,
                        SourceType = model.SourceType,
                        Author = model.Author,
                        Organization = model.Organization,
                        Description = model.Description,
                        LicenseNote = model.LicenseNote,
                        UsagePolicy = model.UsagePolicy
                    };

                    await _referenceService.CreateAsync(source, GetCurrentUserId());
                    TempData["SuccessMessage"] = "Tạo nguồn tham khảo mới thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            PopulateDropdowns(model.SourceType, model.UsagePolicy);
            return View(model);
        }

        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var role = GetCurrentUserRole();
                var model = await _referenceService.GetEditModelAsync(id, GetCurrentUserId(), role);
                if (model == null) return NotFound();

                PopulateDropdowns(model.SourceType, model.UsagePolicy);

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Edit(int id, EditReferenceSourceViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var role = GetCurrentUserRole();

            if (ModelState.IsValid)
            {
                try
                {
                    await _referenceService.UpdateAsync(model, GetCurrentUserId(), role);
                    TempData["SuccessMessage"] = "Cập nhật nguồn tham khảo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (UnauthorizedAccessException)
                {
                    return StatusCode(403);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            PopulateDropdowns(model.SourceType, model.UsagePolicy);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> SubmitReview(int id)
        {
            try
            {
                var role = GetCurrentUserRole();
                await _referenceService.SubmitForReviewAsync(id, GetCurrentUserId(), role);
                TempData["SuccessMessage"] = "Gửi duyệt nguồn tham khảo thành công!";
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Review(int id, string status, string? note)
        {
            if (!Enum.TryParse<ReferenceReviewStatus>(status, out var statusEnum))
            {
                TempData["ErrorMessage"] = "Trạng thái kiểm duyệt không hợp lệ!";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var role = GetCurrentUserRole();
                if (statusEnum == ReferenceReviewStatus.APPROVED)
                {
                    await _referenceService.ApproveAsync(id, GetCurrentUserId(), role);
                    TempData["SuccessMessage"] = "Phê duyệt nguồn tham khảo thành công!";
                }
                else if (statusEnum == ReferenceReviewStatus.REJECTED)
                {
                    await _referenceService.RejectAsync(id, note, GetCurrentUserId(), role);
                    TempData["SuccessMessage"] = "Từ chối nguồn tham khảo thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Trạng thái phê duyệt không được chấp nhận.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                var role = GetCurrentUserRole();
                await _referenceService.ArchiveAsync(id, GetCurrentUserId(), role);
                TempData["SuccessMessage"] = "Đã lưu trữ (Archive) nguồn tham khảo thành công!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private void PopulateDropdowns(ReferenceSourceType? type = null, ReferenceUsagePolicy? policy = null)
        {
            ViewBag.SourceTypes = ReferenceHelper.GetSourceTypeSelectList(type);
            ViewBag.UsagePolicies = ReferenceHelper.GetUsagePolicySelectList(policy);
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out int userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
