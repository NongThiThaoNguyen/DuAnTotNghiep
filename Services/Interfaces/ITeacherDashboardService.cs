using DuAnTotNghiep.Models.ViewModels.Teacher;

namespace DuAnTotNghiep.Services.Interfaces;

public interface ITeacherDashboardService
{
    Task<TeacherDashboardViewModel> GetDashboardAsync(int teacherId);
}
