using System.Threading.Tasks;
using DuAnTotNghiep.DTOs.PlacementTest;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ICompetencyAnalysisService
    {
        Task AnalyzePlacementTestAsync(PlacementTestAnalysisPayload payload);
    }
}
