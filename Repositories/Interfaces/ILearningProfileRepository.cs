using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface ILearningProfileRepository : IGenericRepository<StudentLearningProfile>
    {
        Task<StudentLearningProfile?> GetByUserIdAsync(int userId);
        Task UpdatePrioritySkillsAsync(int profileId, List<string> skillCodes);
    }
}
