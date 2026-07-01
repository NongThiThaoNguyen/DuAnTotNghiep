using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherScheduleService
    {
        Task<List<Schedule>> GetSchedulesAsync(int teacherId, string? keyword, int page, int pageSize);
        Task<int> GetTotalSchedulesCountAsync(int teacherId, string? keyword);
        Task<Schedule?> GetByIdAsync(int id, int teacherId);
        Task CreateAsync(Schedule schedule);
        Task UpdateAsync(Schedule schedule);
        Task DeleteAsync(Schedule schedule);
        Task<List<LearningTopic>> GetActiveTopicsAsync();
    }
}
