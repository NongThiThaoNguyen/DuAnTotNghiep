using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetSystemOverviewAsync();
    Task<int> GetNewUsersThisMonthAsync();
    Task<LearningPathCompletionStatsViewModel> GetLearningPathCompletionStatsAsync();
}
