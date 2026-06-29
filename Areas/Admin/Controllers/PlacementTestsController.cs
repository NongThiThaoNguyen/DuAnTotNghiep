using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
<<<<<<< HEAD
using System.Security.Claims;
using System.Threading.Tasks;
=======
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
<<<<<<< HEAD
    [Authorize(Roles = "ADMIN")]
=======
    [Authorize(Roles = "ADMIN,TEACHER")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    public class PlacementTestsController : Controller
    {
        private readonly IPlacementTestManagementService _managementService;
        private readonly IMasterDataService _masterDataService;
        private readonly IPlacementTestValidationService _validationService;
<<<<<<< HEAD

        public PlacementTestsController(IPlacementTestManagementService managementService, IMasterDataService masterDataService, IPlacementTestValidationService validationService)
=======
        private readonly DuAnTotNghiep.Services.Background.IAiAnalysisQueue _aiQueue;
        private readonly DuAnTotNghiep.Data.ApplicationDbContext _dbContext;

        public PlacementTestsController(
            IPlacementTestManagementService managementService, 
            IMasterDataService masterDataService, 
            IPlacementTestValidationService validationService,
            DuAnTotNghiep.Services.Background.IAiAnalysisQueue aiQueue,
            DuAnTotNghiep.Data.ApplicationDbContext dbContext)
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        {
            _managementService = managementService;
            _masterDataService = masterDataService;
            _validationService = validationService;
<<<<<<< HEAD
=======
            _aiQueue = aiQueue;
            _dbContext = dbContext;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
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

            var validationResult = await _validationService.ValidatePlacementTestAsync(id);
            ViewBag.ValidationResult = validationResult;

            return View(test);
        }

        [HttpGet]
<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        public async Task<IActionResult> Validate(int id)
        {
            var result = await _validationService.ValidatePlacementTestAsync(id);
            return Json(result);
        }

<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        public async Task<IActionResult> Create()
        {
            var levels = await _masterDataService.GetActiveLevelsAsync();
            ViewBag.TargetLevels = new SelectList(levels, "Id", "Name");
            return View(new CreatePlacementTestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
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

<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
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
<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
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
<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        public async Task<IActionResult> Publish(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _managementService.PublishAsync(id, userId);
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
<<<<<<< HEAD
=======
        [Authorize(Roles = "ADMIN")]
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                await _managementService.ArchiveAsync(id, userId);
                TempData["SuccessMessage"] = "Đã lưu trữ (Archive) thành công bài thi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
<<<<<<< HEAD
=======

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetryAiAnalysis(int attemptId)
        {
            try
            {
                var attempt = await _dbContext.TestAttempts.FindAsync(attemptId);
                if (attempt == null) return NotFound("Attempt not found");

                // Reset AI log status to PENDING
                var aiLog = await _dbContext.AiUsageLogs
                    .FirstOrDefaultAsync(l => l.UserId == attempt.StudentId && l.ModuleCode == $"M6_ATTEMPT_{attemptId}");
                
                if (aiLog != null)
                {
                    aiLog.RequestStatus = "PENDING";
                    aiLog.ErrorMessage = null;
                    await _dbContext.SaveChangesAsync();
                }

                await _aiQueue.QueueAttemptForAnalysisAsync(attemptId, attempt.StudentId);
                TempData["SuccessMessage"] = "Đã đưa vào hàng đợi AI Analysis.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi Retry AI Analysis: " + ex.Message;
            }
            
            // Redirect back to wherever we came from, e.g. Attempt Details page
            // Assuming we don't have Attempt Details view, we just redirect to Index
            return RedirectToAction(nameof(Index));
        }
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    }
}
