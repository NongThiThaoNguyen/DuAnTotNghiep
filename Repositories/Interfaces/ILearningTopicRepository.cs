using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories.Interfaces
{
    public interface ILearningTopicRepository : IGenericRepository<LearningTopic>
    {
        Task<IEnumerable<LearningTopic>> GetAllTopicsWithDetailsAsync();
        Task<LearningTopic?> GetTopicWithPrerequisitesAsync(int id);
        Task<bool> HasCircularParentAsync(int topicId, int? parentId);
        
        Task<List<LearningTopic>> SearchTopicsAsync(string? keyword, int? skillId, int? levelId);
        Task<List<LearningTopic>> GetTopicTreeAsync();
        Task<List<LearningTopic>> GetBySkillAsync(int skillId);
        Task<List<LearningTopic>> GetByLevelAsync(int levelId);
        Task<bool> ExistsNameAsync(string name, int skillId);
        Task<bool> IsTopicUsedAsync(int topicId);
    }
}
