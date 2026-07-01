using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherSettingsService
    {
        Task<List<SystemSetting>> GetSystemSettingsAsync();
        Task UpdateSystemSettingsAsync(Dictionary<int, string> settingsData);
    }
}
