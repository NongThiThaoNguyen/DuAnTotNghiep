using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.DTOs.PlacementTest;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestManagementService
    {
        Task<int> CreateAsync(CreatePlacementTestDto dto, int createdBy);
        Task UpdateAsync(UpdatePlacementTestDto dto);
        Task PublishAsync(int placementTestId);
        Task ArchiveAsync(int placementTestId);
        Task<PlacementTestDetailDto?> GetDetailAsync(int id);
        Task<PagedResult<PlacementTestListItemDto>> GetListAsync(PlacementTestFilterDto filter);
    }
}
