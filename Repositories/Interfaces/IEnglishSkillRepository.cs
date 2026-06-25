using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IEnglishSkillRepository : IGenericRepository<EnglishSkill>
    {
        Task<List<EnglishSkill>> GetActiveSkillsAsync();
        Task<bool> ExistsCodeAsync(string code);
        Task<bool> IsSkillUsedAsync(int skillId);
    }
}
