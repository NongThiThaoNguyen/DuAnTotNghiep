using System.Threading.Tasks;
using DuAnTotNghiep.Models.DTOs.PlacementTest;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ICompetencyAnalysisService
    {
        Task AnalyzePlacementTestAsync(PlacementTestAnalysisPayload payload);
    }
}
