using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminTeacherScheduleService
{
    Task<AdminTeacherScheduleListViewModel> GetSchedulesAsync(AdminTeacherScheduleFilterViewModel filter);
    Task<AdminTeacherScheduleFormViewModel> BuildCreateModelAsync(AdminTeacherScheduleFormViewModel? model = null);
    Task<AdminTeacherScheduleFormViewModel?> BuildEditModelAsync(int id);
    Task<int> CreateScheduleAsync(AdminTeacherScheduleFormViewModel model);
    Task<bool> UpdateScheduleAsync(AdminTeacherScheduleFormViewModel model);
    Task<bool> DeleteScheduleAsync(int id);
}
