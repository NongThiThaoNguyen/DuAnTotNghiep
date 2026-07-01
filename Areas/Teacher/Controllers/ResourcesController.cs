using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class ResourcesController : Controller
    {
        private readonly ITeacherResourceService _resourceService;

        public ResourcesController(ITeacherResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // GET: Teacher/Resources
        public async Task<IActionResult> Index(string? keyword, ReferenceSourceType? sourceType, int page = 1, int pageSize = 10)
        {
            var result = await _resourceService.GetResourcesAsync(keyword, sourceType, page, pageSize);

            ViewBag.Keyword = keyword;
            ViewBag.SourceType = sourceType;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(result.TotalItems / (double)pageSize);

            return View(result.Items);
        }

        // GET: Teacher/Resources/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            return View(resource);
        }

        // GET: Teacher/Resources/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teacher/Resources/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReferenceSource resource)
        {
            if (ModelState.IsValid)
            {
                int teacherId = GetCurrentUserId();
                await _resourceService.CreateResourceAsync(resource, teacherId);
                TempData["SuccessMessage"] = "Học liệu đã được tải lên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(resource);
        }

        // GET: Teacher/Resources/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null)
            {
                return NotFound();
            }
            return View(resource);
        }

        // POST: Teacher/Resources/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReferenceSource resource)
        {
            if (id != resource.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_resourceService.ResourceExists(id))
                    {
                        return NotFound();
                    }

                    await _resourceService.UpdateResourceAsync(resource);
                    TempData["SuccessMessage"] = "Cập nhật học liệu thành công!";
                }
                catch
                {
                    if (!_resourceService.ResourceExists(resource.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(resource);
        }

        // GET: Teacher/Resources/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _resourceService.GetResourceByIdAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            return View(resource);
        }

        // POST: Teacher/Resources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _resourceService.DeleteResourceAsync(id);
            TempData["SuccessMessage"] = "Xóa học liệu thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
