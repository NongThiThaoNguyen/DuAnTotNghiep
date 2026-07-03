using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces;

public interface IAdminTeacherManagementService
{
    Task<TeacherManagementIndexViewModel> GetTeacherListAsync();
    Task<TeacherProfileAdminViewModel?> GetTeacherProfileAsync(int teacherId);
    Task<TeacherPerformanceAdminViewModel?> GetTeacherPerformanceAsync(int teacherId);
}
