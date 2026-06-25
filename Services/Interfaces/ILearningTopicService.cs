using DuAnTotNghiep.DTOs.Common;
using DuAnTotNghiep.DTOs.Topic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ILearningTopicService
    {
        Task<int> CreateTopicAsync(CreateTopicDto dto);
        Task UpdateTopicAsync(UpdateTopicDto dto);
        Task DeactivateTopicAsync(int topicId);
        Task ArchiveTopicAsync(int topicId);
        Task ReorderTopicsAsync(List<int> orderedIds);
        
        Task<TopicDetailDto?> GetDetailAsync(int topicId);
        Task<List<TopicTreeDto>> GetTopicTreeAsync();
        Task<PagedResult<TopicListItemDto>> SearchTopicsAsync(TopicSearchRequest request);
        Task<List<TopicOptionDto>> GetActiveTopicOptionsAsync();
        
        Task<bool> IsTopicUsedAsync(int topicId);
        
        // Old methods retained or adapted
        Task<List<TopicTreeDto>> GetTopicTreeBySkillAsync(int skillId);
        Task<List<int>> GetPrerequisiteChainAsync(int topicId);
        Task<bool> HasCircularParentAsync(int topicId, int? parentId);
        Task<bool> HasCircularPrerequisiteAsync(int topicId, int prerequisiteId);
    }
}
