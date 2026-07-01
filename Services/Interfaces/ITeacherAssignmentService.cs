using DuAnTotNghiep.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITeacherAssignmentService
    {
        Task<int> GetTotalPracticeTasksAsync(int? topicId, string? keyword);
        Task<List<PracticeTask>> GetPracticeTasksAsync(int? topicId, string? keyword, int page, int pageSize);
        Task<PracticeTask?> GetPracticeTaskByIdAsync(int id);
        Task CreatePracticeTaskAsync(PracticeTask task);
        Task UpdatePracticeTaskAsync(PracticeTask task);
        Task DeletePracticeTaskAsync(PracticeTask task);
        Task<bool> PracticeTaskExistsAsync(int id);
        Task<int> GetTotalSubmissionsAsync(int? taskId, string? status);
        Task<List<PracticeSubmission>> GetSubmissionsAsync(int? taskId, string? status, int page, int pageSize);
        Task<PracticeSubmission?> GetSubmissionByIdAsync(int id);
        Task UpdateSubmissionAsync(PracticeSubmission submission);
        Task<List<LearningTopic>> GetActiveTopicsAsync();
        Task<List<EnglishSkill>> GetActiveSkillsAsync();
        Task<List<PracticeTask>> GetAllPracticeTasksAsync();
    }
}
