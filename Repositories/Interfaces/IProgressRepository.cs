using DuAnTotNghiep.Models;
using DuAnTotNghiep.ViewModels.Progress;
using DuAnTotNghiep.DTOs.Progress;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IProgressRepository : IGenericRepository<StudentProgressSnapshot>
    {
        Task<StudentProgressSnapshot?> GetDashboardData(int studentId);
        Task<List<LearningHistoryItemViewModel>> GetHistory(int studentId, HistoryFilter filter);
        Task<StudentProgressSnapshot?> GetSkillProgress(int studentId, int skillId);
        Task<StudentProgressSnapshot?> GetTopicProgress(int studentId, int topicId);
        Task UpsertSnapshot(StudentProgressSnapshot snapshot);
    }
}
