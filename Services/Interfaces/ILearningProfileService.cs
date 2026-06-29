using DuAnTotNghiep.Models.DTOs.Onboarding;
using DuAnTotNghiep.Models.ViewModels.Onboarding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ILearningProfileService
    {
        Task<IEnumerable<GoalDto>> GetActiveGoalsAsync();
        Task<IEnumerable<LevelDto>> GetActiveLevelsAsync();
        Task<IEnumerable<SkillDto>> GetActiveSkillsAsync();
        Task<LearningProfileDto?> GetProfileByUserIdAsync(int userId);
        Task<LearningProfileSummaryDto?> GetProfileSummaryAsync(int userId);
        Task<bool> SaveStepAsync(int userId, object stepViewModel);
        Task<bool> UpdateFullProfileAsync(int userId, UpdateLearningProfileViewModel model);
        Task<bool> EditLearningProfileAsync(int userId, UpdateLearningProfileViewModel model);
        Task<bool> MarkOnboardingCompletedAsync(int userId);
        Task<bool> ResetOnboardingStatusAsync(int studentId, int adminId, string ipAddress);
        Task<bool> ValidateProfileActiveDataAsync(int userId);
    }
}
