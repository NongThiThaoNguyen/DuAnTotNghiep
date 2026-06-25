using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface IEnglishProficiencyLevelRepository : IGenericRepository<EnglishProficiencyLevel>
    {
        Task<List<EnglishProficiencyLevel>> GetActiveLevelsAsync();
        Task<bool> ExistsCodeAsync(string code);
    }
}
