using DuAnTotNghiep.DTOs.PlacementTestSection;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class PlacementTestSectionsController : Controller
    {
        private readonly IPlacementTestSectionService _sectionService;
        private readonly IMasterDataService _masterDataService;
        private readonly IPlacementTestManagementService _testManagementService;

        public PlacementTestSectionsController(
            IPlacementTestSectionService sectionService,
            IMasterDataService masterDataService,
            IPlacementTestManagementService testManagementService)
        {
            _sectionService = sectionService;
            _masterDataService = masterDataService;
            _testManagementService = testManagementService;
        }

        public async Task<IActionResult> Index(int placementTestId)
        {
            var test = await _testManagementService.GetDetailAsync(placementTestId);
            if (test == null) return NotFound();

            ViewBag.Test = test;
            var sections = await _sectionService.GetSectionsAsync(placementTestId);
            return View(sections);
        }

        public async Task<IActionResult> Create(int placementTestId)
        {
            var test = await _testManagementService.GetDetailAsync(placementTestId);
            if (test == null) return NotFound();

            if (test.Status == "ARCHIVED")
            {
                TempData["ErrorMessage"] = "Không thể thêm Section vào bài thi đã lưu trữ.";
                return RedirectToAction("Details", "PlacementTests", new { id = placementTestId });
            }

            var skills = await _masterDataService.GetActiveSkillsAsync();
            ViewBag.Skills = new SelectList(skills.OrderBy(s => s.SkillName), "Id", "SkillName");

            var dto = new CreateSectionDto
            {
                PlacementTestId = placementTestId,
                OrderIndex = (test.SectionCount) + 1
            };

            ViewBag.TestTitle = test.Title;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSectionDto dto)
        {
            if (!ModelState.IsValid)
            {
                var test = await _testManagementService.GetDetailAsync(dto.PlacementTestId);
                var skills = await _masterDataService.GetActiveSkillsAsync();
                ViewBag.Skills = new SelectList(skills.OrderBy(s => s.SkillName), "Id", "SkillName");
                ViewBag.TestTitle = test?.Title;
                return View(dto);
            }

            try
            {
                await _sectionService.AddSectionAsync(dto);
                TempData["SuccessMessage"] = "Thêm phần thi (Section) thành công.";
                return RedirectToAction(nameof(Index), new { placementTestId = dto.PlacementTestId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var test = await _testManagementService.GetDetailAsync(dto.PlacementTestId);
                var skills = await _masterDataService.GetActiveSkillsAsync();
                ViewBag.Skills = new SelectList(skills.OrderBy(s => s.SkillName), "Id", "SkillName");
                ViewBag.TestTitle = test?.Title;
                return View(dto);
            }
        }

        public async Task<IActionResult> Edit(int placementTestId, int id)
        {
            var test = await _testManagementService.GetDetailAsync(placementTestId);
            if (test == null) return NotFound();

            var sections = await _sectionService.GetSectionsAsync(placementTestId);
            var section = sections.FirstOrDefault(s => s.Id == id);
            if (section == null) return NotFound();

            var skills = await _masterDataService.GetActiveSkillsAsync();
            ViewBag.Skills = new SelectList(skills.OrderBy(s => s.SkillName), "Id", "SkillName");
            ViewBag.TestTitle = test.Title;
            ViewBag.HasAttempt = test.AttemptCount > 0;

            var dto = new UpdateSectionDto
            {
                Id = section.Id,
                PlacementTestId = section.PlacementTestId,
                SkillId = section.SkillId,
                SectionName = section.SectionName,
                Instruction = section.Instruction,
                OrderIndex = section.OrderIndex,
                MaxScore = section.MaxScore
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int placementTestId, int id, UpdateSectionDto dto)
        {
            if (id != dto.Id || placementTestId != dto.PlacementTestId) return BadRequest();

            if (!ModelState.IsValid)
            {
                var testInfo = await _testManagementService.GetDetailAsync(placementTestId);
                var skills = await _masterDataService.GetActiveSkillsAsync();
                ViewBag.Skills = new SelectList(skills.OrderBy(s => s.SkillName), "Id", "SkillName");
                ViewBag.TestTitle = testInfo?.Title;
                ViewBag.HasAttempt = testInfo?.AttemptCount > 0;
                return View(dto);
            }

            try
            {
                await _sectionService.UpdateSectionAsync(dto);
                TempData["SuccessMessage"] = "Cập nhật Section thành công.";
                return RedirectToAction(nameof(Index), new { placementTestId = dto.PlacementTestId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var testInfo = await _testManagementService.GetDetailAsync(placementTestId);
                var skills = await _masterDataService.GetActiveSkillsAsync();
                ViewBag.Skills = new SelectList(skills.OrderBy(s => s.SkillName), "Id", "SkillName");
                ViewBag.TestTitle = testInfo?.Title;
                ViewBag.HasAttempt = testInfo?.AttemptCount > 0;
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int placementTestId, int id)
        {
            try
            {
                await _sectionService.DeleteSectionIfUnusedAsync(id);
                TempData["SuccessMessage"] = "Xóa Section thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index), new { placementTestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int placementTestId, [FromBody] List<SectionOrderDto> sectionOrders)
        {
            try
            {
                await _sectionService.ReorderSectionAsync(placementTestId, sectionOrders);
                return Ok(new { success = true, message = "Cập nhật thứ tự thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
