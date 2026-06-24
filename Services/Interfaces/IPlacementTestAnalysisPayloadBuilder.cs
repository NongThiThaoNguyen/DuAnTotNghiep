using System.Threading.Tasks;
using DuAnTotNghiep.DTOs.PlacementTest;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestAnalysisPayloadBuilder
    {
        Task<PlacementTestAnalysisPayload?> BuildPayloadAsync(int attemptId, int studentId);
    }
}
