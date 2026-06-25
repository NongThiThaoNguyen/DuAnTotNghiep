using DuAnTotNghiep.DTOs.Skill;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IEnglishSkillService
    {
        Task<List<SkillOptionDto>> GetOptionsAsync();
        Task<List<SkillListItemDto>> GetListAsync();
        Task<SkillDetailDto?> GetByIdAsync(int id);
        Task CreateAsync(CreateSkillDto dto);
        Task UpdateAsync(UpdateSkillDto dto);
        Task DeactivateAsync(int id);
    }
}
