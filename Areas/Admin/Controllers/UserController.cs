using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserProfileService _profileService;

        public UserController(IUserService userService, IUserProfileService profileService)
        {
            _userService = userService;
            _profileService = profileService;
        }

        public async Task<IActionResult> Index([FromQuery] UserFilterViewModel filter)
        {
            var model = await _userService.GetPagedUsersAsync(filter);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userDetails = await _profileService.GetAdminUserProfileAsync(id);
                return View(userDetails);
            }
            catch (DuAnTotNghiep.Exceptions.NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.LockUserAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Khóa tài khoản thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể khóa tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.UnlockUserAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Mở khóa tài khoản thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể mở khóa tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.ResetPasswordAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Mật khẩu mới đã được tạo và gửi qua email cho người dùng.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể reset mật khẩu cho tài khoản này.";
            }

            return RedirectToAction("Index");
        }
    }
}
