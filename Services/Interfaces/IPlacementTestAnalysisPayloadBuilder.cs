using System.Threading.Tasks;
using DuAnTotNghiep.Models.DTOs.PlacementTest;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestAnalysisPayloadBuilder
    {
        Task<PlacementTestAnalysisPayload?> BuildPayloadAsync(int attemptId, int studentId);
    }
}
