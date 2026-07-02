using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminTeacherScheduleAiService
{
    Task<AdminTeacherScheduleAiResultViewModel> GenerateSuggestionsAsync(
        AdminTeacherScheduleAiRequestViewModel request,
        CancellationToken cancellationToken = default);
}
