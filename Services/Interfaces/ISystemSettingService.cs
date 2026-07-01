using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ISystemSettingService
    {
        Task<List<SystemSetting>> GetAllSettingsAsync();
        Task<SystemSetting?> GetSettingAsync(string key);
        Task UpdateSettingAsync(string key, string value, int userId);
    }
}
