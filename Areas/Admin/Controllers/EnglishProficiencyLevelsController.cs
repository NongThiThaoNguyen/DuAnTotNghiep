using DuAnTotNghiep.Models.DTOs.Level;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels.Admin.Levels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class EnglishProficiencyLevelsController : Controller
    {
        private readonly IEnglishProficiencyLevelService _service;

        public EnglishProficiencyLevelsController(IEnglishProficiencyLevelService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var levels = await _service.GetListAsync();
            return View(levels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var detailDto = await _service.GetByIdAsync(id);
            if (detailDto == null) return NotFound();
            return View(detailDto);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            return View(new LevelCreateViewModel { OrderIndex = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(LevelCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.OrderIndex < 0)
            {
                ModelState.AddModelError("OrderIndex", "OrderIndex cannot be negative.");
                return View(model);
            }

            try
            {
                var dto = new CreateLevelDto
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    OrderIndex = model.OrderIndex,
                    IsActive = model.IsActive
                };

                await _service.CreateAsync(dto);
                TempData["SuccessMessage"] = "Level created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id)
        {
            var level = await _service.GetByIdAsync(id);
            if (level == null)
                return NotFound();

            var model = new LevelEditViewModel
            {
                Id = level.Id,
                Code = level.Code,
                Name = level.Name,
                Description = level.Description,
                OrderIndex = level.OrderIndex,
                IsActive = level.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id, LevelEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            if (model.OrderIndex < 0)
            {
                ModelState.AddModelError("OrderIndex", "OrderIndex cannot be negative.");
                return View(model);
            }

            try
            {
                var dto = new UpdateLevelDto
                {
                    Id = model.Id,
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    OrderIndex = model.OrderIndex,
                    IsActive = model.IsActive
                };

                await _service.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Level updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _service.DeactivateAsync(id);
                TempData["SuccessMessage"] = "Level deactivated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _service.ArchiveLevelAsync(id);
                TempData["SuccessMessage"] = "Level archived successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _service.RestoreLevelAsync(id);
                TempData["SuccessMessage"] = "Level restored successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
