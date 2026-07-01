using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "SystemSettingsCache";

        public SystemSettingService(ApplicationDbContext context, IAuditService auditService, IMemoryCache cache)
        {
            _context = context;
            _auditService = auditService;
            _cache = cache;
        }

        public async Task<List<SystemSetting>> GetAllSettingsAsync()
        {
            if (!_cache.TryGetValue(CacheKey, out List<SystemSetting>? settings) || settings == null)
            {
                settings = await _context.SystemSettings.AsNoTracking().ToListAsync();
                _cache.Set(CacheKey, settings, TimeSpan.FromHours(1));
            }
            return settings;
        }

        public async Task<SystemSetting?> GetSettingAsync(string key)
        {
            var settings = await GetAllSettingsAsync();
            return settings.Find(s => s.SettingKey == key);
        }

        public async Task UpdateSettingAsync(string key, string value, int userId)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting != null)
            {
                var oldValue = setting.SettingValue;

                if (oldValue != value)
                {
                    setting.SettingValue = value;
                    setting.UpdatedBy = userId;
                    setting.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    // Log this action
                    await _auditService.LogAsync(
                        userId: userId,
                        action: "UPDATE_SETTING",
                        entityName: "SystemSetting",
                        entityId: setting.Id,
                        oldValue: oldValue,
                        newValue: value
                    );
                }
            }
            else
            {
                // If setting doesn't exist, we create it (optional, depending on requirements)
                setting = new SystemSetting
                {
                    SettingKey = key,
                    SettingValue = value,
                    UpdatedBy = userId,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.SystemSettings.Add(setting);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    userId: userId,
                    action: "CREATE_SETTING",
                    entityName: "SystemSetting",
                    entityId: setting.Id,
                    oldValue: null,
                    newValue: value
                );
            }

            // Invalidate cache
            _cache.Remove(CacheKey);
        }
    }
}
