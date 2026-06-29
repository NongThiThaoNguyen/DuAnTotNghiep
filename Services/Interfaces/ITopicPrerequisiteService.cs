using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.DTOs.Topic;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITopicPrerequisiteService
    {
        Task AddPrerequisiteAsync(int topicId, int prerequisiteId, int? createdBy);
        Task RemovePrerequisiteAsync(int topicId, int prerequisiteId);
        Task<IList<TopicOptionDto>> GetPrerequisitesAsync(int topicId);
        Task<bool> ValidatePrerequisiteCycleAsync(int topicId, int prerequisiteId);
    }
}
