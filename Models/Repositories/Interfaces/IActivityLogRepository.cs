using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.DTOs.Progress;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IActivityLogRepository : IGenericRepository<StudyActivityLog>
    {
        Task AddActivityLog(StudyActivityLog log);
        Task AddActivityLog(ActivityLogCreateDto dto);
        Task<List<StudyActivityLog>> GetRecentActivities(int studentId, int count);
        Task<List<StudyActivityLog>> GetActivitiesForStreak(int studentId);
        Task<int> GetTotalStudyMinutes(int studentId);
    }
}
