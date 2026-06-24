using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<EnglishSkill>> GetActiveSkillsAsync();
        Task<IEnumerable<EnglishProficiencyLevel>> GetActiveLevelsAsync();
        IEnumerable<SelectListItem> GetDifficulties();
        IEnumerable<SelectListItem> GetQuestionTypes();
        Task<SelectList> GetSkillsSelectListAsync(int? selectedValue = null);
        Task<SelectList> GetLevelsSelectListAsync(int? selectedValue = null);
    }
}
