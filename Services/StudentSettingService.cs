using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class StudentSettingService : IStudentSettingService
    {
        private readonly ApplicationDbContext _context;

        public StudentSettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserSetting> GetSettingAsync(int userId)
        {
            var setting = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (setting == null)
            {
                setting = new UserSetting
                {
                    UserId = userId,
                    Language = "vi",
                    Timezone = "SE Asia Standard Time",
                    EmailNotifications = true,
                    StudyReminderEnabled = true,
                    Theme = "light"
                };
                await _context.UserSettings.AddAsync(setting);
                await _context.SaveChangesAsync();
            }

            return setting;
        }

        public async Task UpdateSettingAsync(int userId, UserSetting setting)
        {
            var existing = await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existing == null)
            {
                setting.UserId = userId;
                await _context.UserSettings.AddAsync(setting);
            }
            else
            {
                existing.Language = setting.Language;
                existing.Timezone = setting.Timezone;
                existing.EmailNotifications = setting.EmailNotifications;
                existing.StudyReminderEnabled = setting.StudyReminderEnabled;
                existing.Theme = setting.Theme;
            }

            await _context.SaveChangesAsync();
        }
    }
}
