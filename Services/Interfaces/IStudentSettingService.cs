using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentSettingService
    {
        Task<UserSetting> GetSettingAsync(int userId);
        Task UpdateSettingAsync(int userId, UserSetting setting);
    }
}
