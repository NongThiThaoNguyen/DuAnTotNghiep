using DuAnTotNghiep.Models.ViewModels.Teacher;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherReportService
    {
        Task<ReportDashboardViewModel> GetDashboardReportAsync();
    }
}
