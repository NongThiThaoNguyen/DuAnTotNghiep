using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class SystemSettingsController : Controller
    {
        private readonly ISystemSettingService _systemSettingService;

        public SystemSettingsController(ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _systemSettingService.GetAllSettingsAsync();
            return View("~/Areas/Admin/Views/SystemSettings/Index.cshtml", settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                TempData["Error"] = "Không tìm thấy khóa cài đặt cần cập nhật.";
                return RedirectToAction(nameof(Index));
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                TempData["Error"] = "Không thể lấy thông tin người dùng.";
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            try
            {
                await _systemSettingService.UpdateSettingAsync(key.Trim(), value?.Trim() ?? string.Empty, userId);

                TempData["SuccessMessage"] = $"Đã cập nhật cài đặt \"{key}\".";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã xảy ra lỗi khi lưu cấu hình: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
