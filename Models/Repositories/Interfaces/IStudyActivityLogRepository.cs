using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface IStudyActivityLogRepository : IGenericRepository<StudyActivityLog>
    {
        Task<List<StudyActivityLog>> GetRecentActivitiesAsync(int studentId, int count);
        Task<List<StudyActivityLog>> GetActivitiesForStreakAsync(int studentId);
        Task<int> GetTotalStudyMinutesAsync(int studentId);
    }
}
