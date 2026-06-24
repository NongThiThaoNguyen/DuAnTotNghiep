using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class PlacementTestsController : Controller
    {
        private readonly IPlacementTestManagementService _managementService;
        private readonly IMasterDataService _masterDataService;

        public PlacementTestsController(IPlacementTestManagementService managementService, IMasterDataService masterDataService)
        {
            _managementService = managementService;
            _masterDataService = masterDataService;
        }

        public async Task<IActionResult> Index([FromQuery] PlacementTestFilterDto filter)
        {
            var pagedResult = await _managementService.GetListAsync(filter);
            
            // ViewBag for dropdowns
            var levels = await _masterDataService.GetActiveLevelsAsync();
            ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");

            ViewBag.Filter = filter;
            return View(pagedResult);
        }

        public async Task<IActionResult> Details(int id)
        {
            var test = await _managementService.GetDetailAsync(id);
            if (test == null) return NotFound();
            return View(test);
        }

        public async Task<IActionResult> Create()
        {
            var levels = await _masterDataService.GetActiveLevelsAsync();
            ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
            return View(new CreatePlacementTestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlacementTestDto dto)
        {
            if (!ModelState.IsValid)
            {
                var levels = await _masterDataService.GetActiveLevelsAsync();
                ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
                return View(dto);
            }

            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _managementService.CreateAsync(dto, userId);
                TempData["SuccessMessage"] = "Tạo Placement Test thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var levels = await _masterDataService.GetActiveLevelsAsync();
                ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var test = await _managementService.GetDetailAsync(id);
            if (test == null) return NotFound();

            var dto = new UpdatePlacementTestDto
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                TargetLevelId = test.TargetLevelId ?? 0,
                TimeLimitMinutes = test.TimeLimitMinutes,
                TotalScore = test.TotalScore
            };

            var levels = await _masterDataService.GetActiveLevelsAsync();
            ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
            ViewBag.HasAttempt = test.AttemptCount > 0;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePlacementTestDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                var testInfo = await _managementService.GetDetailAsync(id);
                var levels = await _masterDataService.GetActiveLevelsAsync();
                ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
                ViewBag.HasAttempt = testInfo?.AttemptCount > 0;
                return View(dto);
            }

            try
            {
                await _managementService.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var testInfo = await _managementService.GetDetailAsync(id);
                var levels = await _masterDataService.GetActiveLevelsAsync();
                ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
                ViewBag.HasAttempt = testInfo?.AttemptCount > 0;
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                await _managementService.PublishAsync(id);
                TempData["SuccessMessage"] = "Đã xuất bản (Publish) thành công bài thi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _managementService.ArchiveAsync(id);
                TempData["SuccessMessage"] = "Đã lưu trữ (Archive) thành công bài thi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
