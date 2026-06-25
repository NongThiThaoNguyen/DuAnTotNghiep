using DuAnTotNghiep.DTOs.Objective;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ILearningObjectiveService
    {
        Task<int> AddObjectiveAsync(CreateObjectiveDto dto);
        Task UpdateObjectiveAsync(UpdateObjectiveDto dto);
        Task DeleteObjectiveAsync(int objectiveId);
        Task ReorderObjectivesAsync(int topicId, List<int> orderedIds);
        Task<List<ObjectiveDto>> GetObjectivesByTopicAsync(int topicId);
        Task<ObjectiveDetailDto?> GetByIdAsync(int objectiveId);
    }
}
