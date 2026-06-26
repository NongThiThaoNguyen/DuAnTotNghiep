using DuAnTotNghiep.DTOs.Skill;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Admin.Skills;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Teacher")]
    public class EnglishSkillsController : Controller
    {
        private readonly IEnglishSkillService _service;

        public EnglishSkillsController(IEnglishSkillService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var skills = await _service.GetListAsync();
            return View(skills);
        }

        public async Task<IActionResult> Details(int id)
        {
            var detailDto = await _service.GetByIdAsync(id);
            if (detailDto == null) return NotFound();
            return View(detailDto);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new SkillCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(SkillCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new CreateSkillDto
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _service.CreateAsync(dto);
                TempData["SuccessMessage"] = "Skill created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var skill = await _service.GetByIdAsync(id);
            if (skill == null)
                return NotFound();

            var model = new SkillEditViewModel
            {
                Id = skill.Id,
                Code = skill.Code,
                Name = skill.Name,
                Description = skill.Description,
                IsActive = skill.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, SkillEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var dto = new UpdateSkillDto
                {
                    Id = model.Id,
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = model.IsActive
                };

                await _service.UpdateAsync(dto);
                TempData["SuccessMessage"] = "Skill updated successfully.";
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _service.DeactivateAsync(id);
                TempData["SuccessMessage"] = "Skill deactivated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                await _service.ArchiveSkillAsync(id);
                TempData["SuccessMessage"] = "Skill archived successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                await _service.RestoreSkillAsync(id);
                TempData["SuccessMessage"] = "Skill restored successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
