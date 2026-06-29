using DuAnTotNghiep.DTOs.PlacementTest;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementRequirementService
    {
        Task<bool> IsPlacementTestRequiredAsync(int studentId);
        Task<bool> HasCompletedPlacementTestAsync(int studentId);
        Task<PlacementFlowResultDto> GetStudentFlowStatusAsync(int studentId);
    }
}
