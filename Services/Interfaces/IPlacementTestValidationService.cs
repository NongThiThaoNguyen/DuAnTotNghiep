using DuAnTotNghiep.DTOs.PlacementTest.Validation;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestValidationService
    {
        Task<TestValidationResultDto> ValidatePlacementTestAsync(int placementTestId);
    }
}
