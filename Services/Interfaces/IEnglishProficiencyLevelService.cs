using DuAnTotNghiep.DTOs.Level;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IEnglishProficiencyLevelService
    {
        Task<List<LevelOptionDto>> GetOptionsAsync();
        Task<List<LevelListItemDto>> GetListAsync();
        Task<LevelDetailDto?> GetByIdAsync(int id);
        Task CreateAsync(CreateLevelDto dto);
        Task UpdateAsync(UpdateLevelDto dto);
        Task DeactivateAsync(int id);
    }
}
