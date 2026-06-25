using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface ILearningObjectiveRepository : IGenericRepository<LearningObjective>
    {
        Task<List<LearningObjective>> GetByTopicAsync(int topicId);
        Task<bool> ExistsCodeAsync(string code, int topicId);
        Task<bool> IsObjectiveUsedAsync(int objectiveId);
    }
}
