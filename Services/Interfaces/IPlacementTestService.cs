using DuAnTotNghiep.ViewModels.PlacementTest;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestService
    {
        Task<PlacementTestSuggestionViewModel?> BuildPlacementTestSuggestionAsync(int userId);
    }
}
