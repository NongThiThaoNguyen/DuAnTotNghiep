using DuAnTotNghiep.Models.DTOs.PlacementTestSection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestSectionService
    {
        Task<int> AddSectionAsync(CreateSectionDto dto);
        Task UpdateSectionAsync(UpdateSectionDto dto);
        Task ReorderSectionAsync(int placementTestId, List<SectionOrderDto> sections);
        Task DeleteSectionIfUnusedAsync(int sectionId);
        Task<List<PlacementTestSectionDto>> GetSectionsAsync(int placementTestId);
    }
}
