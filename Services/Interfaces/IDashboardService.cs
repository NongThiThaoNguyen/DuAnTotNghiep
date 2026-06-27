using System.Threading.Tasks;
using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
    }
}
