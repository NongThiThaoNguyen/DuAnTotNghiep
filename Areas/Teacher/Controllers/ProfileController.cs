using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class ProfileController : Controller
    {
        private readonly ITeacherProfileService _profileService;

        public ProfileController(ITeacherProfileService profileService)
        {
            _profileService = profileService;
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
            var teacher = await _profileService.GetTeacherProfileAsync(teacherId);
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

            try
            {
                await _profileService.UpdateTeacherProfileAsync(teacherId, model, avatarFile, bio, gender, country, dateOfBirth);
                TempData["Success"] = "Hồ sơ của bạn đã được cập nhật thành công!";
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật hồ sơ: " + ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
