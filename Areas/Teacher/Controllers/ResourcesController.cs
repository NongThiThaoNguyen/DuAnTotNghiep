using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class ResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResourcesController(ApplicationDbContext context)
        {
            _context = context;
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
            var query = _context.ReferenceSources
                .AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.SourceName.Contains(keyword) || (r.Description != null && r.Description.Contains(keyword)) || (r.Author != null && r.Author.Contains(keyword)));
            }

            if (sourceType.HasValue)
            {
                query = query.Where(r => r.SourceType == sourceType.Value);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.SourceType = sourceType;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(items);
        }

        // GET: Teacher/Resources/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.ReferenceSources
                .Include(r => r.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

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
            int teacherId = GetCurrentUserId();
            if (ModelState.IsValid)
            {
                resource.CreatedAt = DateTime.UtcNow;
                resource.UpdatedAt = DateTime.UtcNow;
                resource.CreatedBy = teacherId;
                resource.IsActive = true;
                resource.Status = ReferenceReviewStatus.APPROVED; // Teacher created is auto approved

                _context.Add(resource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Học liệu đã được tải lên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(resource);
        }

        // GET: Teacher/Resources/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _context.ReferenceSources.FindAsync(id);
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
                    var existing = await _context.ReferenceSources.FindAsync(id);
                    if (existing == null) return NotFound();

                    existing.SourceName = resource.SourceName;
                    existing.SourceUrl = resource.SourceUrl;
                    existing.SourceType = resource.SourceType;
                    existing.LicenseNote = resource.LicenseNote;
                    existing.Author = resource.Author;
                    existing.Organization = resource.Organization;
                    existing.Description = resource.Description;
                    existing.UsagePolicy = resource.UsagePolicy;
                    existing.IsActive = resource.IsActive;
                    existing.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật học liệu thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResourceExists(resource.Id))
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
            var resource = await _context.ReferenceSources
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var resource = await _context.ReferenceSources.FindAsync(id);
            if (resource != null)
            {
                _context.ReferenceSources.Remove(resource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa học liệu thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ResourceExists(int id)
        {
            return _context.ReferenceSources.Any(e => e.Id == id);
        }
    }
}
