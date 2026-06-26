using DuAnTotNghiep.DTOs.Common;
using DuAnTotNghiep.DTOs.Topic;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Enums;
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
        Task<List<TopicOptionDto>> GetActiveTopicsBySkillAsync(int skillId);
        Task<List<TopicOptionDto>> GetTopicsForQuestionAsync(int skillId, int? levelId);
        Task<bool> IsTopicAvailableAsync(int topicId);
        
        // Old methods retained or adapted
        Task<List<TopicTreeDto>> GetTopicTreeBySkillAsync(int skillId);
        Task<List<int>> GetPrerequisiteChainAsync(int topicId);
        Task<bool> HasCircularParentAsync(int topicId, int? parentId);
        Task<bool> HasCircularPrerequisiteAsync(int topicId, int prerequisiteId);

        // Task 17 Additions
        Task<List<OriginalLesson>> GetLessonsByTopicAsync(int topicId);
        Task<List<Quiz>> GetQuizzesByTopicAsync(int topicId);
        Task<List<StudentLearningPath>> GetLearningPathsByTopicAsync(int topicId);
        Task<List<LearningTopic>> GetEligibleTopicsForPathAsync();
        Task<List<LearningTopic>> GetActiveTopicsAsync();
        Task<AiTopicPayloadDto?> GetAiPayloadForTopicAsync(int topicId);

        Task UpdateLessonTopicAsync(int lessonId, int topicId);
        Task UpdateQuizTopicAsync(int quizId, int topicId);
        Task UpdateLearningPathNodeTopicAsync(int nodeId, int topicId);
        Task LinkTopicPrerequisiteAsync(int topicId, int prerequisiteTopicId);
        Task RestoreTopicAsync(int id);

        // Reference Source Methods (Module M5 Task 2)
        Task<ReferenceSource?> GetReferenceSourceByIdAsync(int id);
        Task<IEnumerable<ReferenceSource>> GetAllReferenceSourcesAsync(ReferenceSourceType? sourceType, ReferenceReviewStatus? status, string? keyword);
        Task<int> CreateReferenceSourceAsync(ReferenceSource referenceSource);
        Task UpdateReferenceSourceAsync(ReferenceSource referenceSource);
        Task DeleteReferenceSourceAsync(int id);
        Task ReviewReferenceSourceAsync(int id, ReferenceReviewStatus status, string? note);
        Task ArchiveReferenceSourceAsync(int id);
    }
}
