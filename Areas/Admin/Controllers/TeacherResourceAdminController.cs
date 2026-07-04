using System.Security.Claims;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class TeacherResourceAdminController : Controller
{
    private const int PageSize = 20;
    private readonly ApplicationDbContext _context;

    public TeacherResourceAdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? teacherId, int page = 1)
    {
        page = Math.Max(page, 1);
        var query = _context.ReferenceSources.AsNoTracking()
            .Include(r => r.CreatedByNavigation)
            .AsQueryable();

        if (teacherId.HasValue)
        {
            query = query.Where(r => r.CreatedBy == teacherId.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)PageSize));
        page = Math.Min(page, totalPages);

        var model = new TeacherResourceAdminIndexViewModel
        {
            TeacherId = teacherId,
            CurrentPage = page,
            PageSize = PageSize,
            TotalItems = totalItems,
            Teachers = await GetTeacherOptionsAsync(),
            Items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(r => new TeacherResourceRowViewModel
                {
                    ResourceId = r.Id,
                    SourceName = r.SourceName,
                    SourceUrl = r.SourceUrl,
                    SourceType = r.SourceType.ToString(),
                    Status = r.Status.ToString(),
                    CreatedByName = r.CreatedByNavigation != null ? r.CreatedByNavigation.FullName : "Không rõ",
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int resourceId, int? teacherId)
    {
        var resource = await _context.ReferenceSources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound();
        }

        resource.Status = ReferenceReviewStatus.APPROVED;
        resource.ApprovedBy = GetCurrentUserId();
        resource.ApprovedAt = DateTime.UtcNow;
        resource.RejectedBy = null;
        resource.RejectedAt = null;
        resource.RejectionReason = null;
        resource.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã duyệt tài liệu.";
        return RedirectToAction(nameof(Index), new { teacherId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int resourceId, int? teacherId)
    {
        var resource = await _context.ReferenceSources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound();
        }

        resource.Status = ReferenceReviewStatus.REJECTED;
        resource.RejectedBy = GetCurrentUserId();
        resource.RejectedAt = DateTime.UtcNow;
        resource.RejectionReason = "Admin từ chối trong màn hình quản lý tài liệu giáo viên.";
        resource.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đã từ chối tài liệu.";
        return RedirectToAction(nameof(Index), new { teacherId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int resourceId, int? teacherId)
    {
        var resource = await _context.ReferenceSources.FindAsync(resourceId);
        if (resource == null)
        {
            return NotFound();
        }

        resource.Status = ReferenceReviewStatus.ARCHIVED;
        resource.IsActive = false;
        resource.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Đã lưu trữ tài liệu.";
        return RedirectToAction(nameof(Index), new { teacherId });
    }

    private int? GetCurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }

    private Task<List<AdminOptionViewModel>> GetTeacherOptionsAsync()
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Role.RoleCode == "TEACHER")
            .OrderBy(u => u.FullName)
            .Select(u => new AdminOptionViewModel { Id = u.Id, Text = u.FullName })
            .ToListAsync();
    }
}
