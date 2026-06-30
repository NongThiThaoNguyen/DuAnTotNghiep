using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            var teacher = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == teacherId);
            if (teacher == null) return NotFound();

            return View(teacher);
        }

        // POST: Teacher/Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(User model, IFormFile? avatarFile, string? bio, string? gender, string? country, DateOnly? dateOfBirth)
        {
            int teacherId = GetCurrentUserId();
            if (model.Id != teacherId) return BadRequest();

            var existing = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Id == teacherId);
            if (existing == null) return NotFound();

            existing.FullName = model.FullName;
            existing.Email = model.Email;
            existing.Phone = model.Phone;

            // Handle file upload for Avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                try
                {
                    var extension = Path.GetExtension(avatarFile.FileName);
                    var filename = Guid.NewGuid().ToString() + extension;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                    
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, filename);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    existing.AvatarUrl = "/uploads/avatars/" + filename;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi tải ảnh đại diện lên: " + ex.Message);
                }
            }

            // Handle UserProfile updates
            var profile = existing.UserProfile;
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = teacherId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserProfiles.Add(profile);
            }

            profile.Bio = bio;
            profile.Gender = gender;
            profile.Country = country;
            profile.DateOfBirth = dateOfBirth;
            profile.UpdatedAt = DateTime.UtcNow;

            existing.UpdatedAt = DateTime.UtcNow;

            _context.Update(existing);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hồ sơ của bạn đã được cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
