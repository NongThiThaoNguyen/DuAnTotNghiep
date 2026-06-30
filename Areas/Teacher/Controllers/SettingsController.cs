using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Teacher/Settings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SystemSettings.AsNoTracking().ToListAsync();
            return View(settings);
        }

        // POST: Teacher/Settings/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Dictionary<int, string> settingsData)
        {
            if (settingsData != null && settingsData.Any())
            {
                foreach (var kvp in settingsData)
                {
                    var setting = await _context.SystemSettings.FindAsync(kvp.Key);
                    if (setting != null)
                    {
                        setting.SettingValue = kvp.Value;
                        setting.UpdatedAt = DateTime.UtcNow;
                        _context.Update(setting);
                    }
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật cấu hình hệ thống thành công!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
