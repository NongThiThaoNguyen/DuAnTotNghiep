using DuAnTotNghiep.Models.DTOs.Progress;
using DuAnTotNghiep.Models.ViewModels.Progress;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentProgressService
    {
        Task RecordActivityAsync(ActivityLogCreateDto dto, int studentId);
        Task<ProgressDashboardViewModel> GetDashboardAsync(int studentId);
        Task<List<LearningHistoryItemViewModel>> GetLearningHistoryAsync(int studentId, int page, int pageSize);
        Task UpdateProgressSnapshotsAsync(int studentId);
        Task<ReplanningInputDto> GetReplanningInputAsync(int studentId, int pathId);
    }
}
