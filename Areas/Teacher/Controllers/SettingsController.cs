using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class SettingsController : Controller
    {
        private readonly ITeacherSettingsService _settingsService;

        public SettingsController(ITeacherSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // GET: Teacher/Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetSystemSettingsAsync();
            return View(settings);
        }

        // POST: Teacher/Settings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Dictionary<int, string> settingsData)
        {
            await _settingsService.UpdateSystemSettingsAsync(settingsData);
            TempData["SuccessMessage"] = "Đã cập nhật cấu hình hệ thống thành công!";

            return RedirectToAction(nameof(Index));
        }
    }
}
