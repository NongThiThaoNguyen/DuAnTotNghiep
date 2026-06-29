using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories.Interfaces
{
    public interface ITopicPrerequisiteRepository : IGenericRepository<TopicPrerequisite>
    {
        Task<IEnumerable<TopicPrerequisite>> GetPrerequisitesByTopicIdAsync(int topicId);
        Task<bool> ExistsAsync(int topicId, int prerequisiteTopicId);
    }
}
