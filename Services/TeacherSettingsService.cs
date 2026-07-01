using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherSettingsService : ITeacherSettingsService
    {
        private readonly ApplicationDbContext _context;

        public TeacherSettingsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SystemSetting>> GetSystemSettingsAsync()
        {
            return await _context.SystemSettings.AsNoTracking().ToListAsync();
        }

        public async Task UpdateSystemSettingsAsync(Dictionary<int, string> settingsData)
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
            }
        }
    }
}
