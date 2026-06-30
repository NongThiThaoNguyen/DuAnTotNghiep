using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
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

        // GET: Teacher/Profile
        public async Task<IActionResult> Index()
        {
            int teacherId = GetCurrentUserId();
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // POST: Teacher/Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(User model)
        {
            int teacherId = GetCurrentUserId();
            if (model.Id != teacherId) return BadRequest();

            var existing = await _context.Users.FindAsync(teacherId);
            if (existing == null) return NotFound();

            existing.FullName = model.FullName;
            existing.Email = model.Email;
            existing.Phone = model.Phone;
            if (!string.IsNullOrEmpty(model.AvatarUrl))
            {
                existing.AvatarUrl = model.AvatarUrl;
            }
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Update(existing);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hồ sơ của bạn đã được cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
