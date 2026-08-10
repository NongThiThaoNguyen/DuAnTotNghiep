using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminTeacherManagementService
{
    Task<TeacherManagementIndexViewModel> GetTeacherListAsync(string? keyword = null, string? status = null);
    Task<TeacherProfileAdminViewModel?> GetTeacherProfileAsync(int teacherId);
    Task<TeacherPerformanceAdminViewModel?> GetTeacherPerformanceAsync(int teacherId);
    Task<(bool Success, string Message)> CreateTeacherAsync(CreateTeacherAdminViewModel model);
    Task<EditTeacherAdminViewModel?> GetTeacherForEditAsync(int teacherId);
    Task<(bool Success, string Message)> UpdateTeacherAsync(EditTeacherAdminViewModel model);
    Task<(bool Success, string Message, string NewStatus)> ToggleLockTeacherAsync(int teacherId);
}
