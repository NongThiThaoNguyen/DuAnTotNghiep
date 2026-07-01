using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.Student;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardViewModel> GetDashboardAsync(int userId);
    }
}
