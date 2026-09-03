using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.LearningPath;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ILevelUpAssessmentService
    {
        Task<LevelUpAssessmentViewModel?> GetLevelUpAssessmentAsync(int studentId);
        Task<LevelUpResultViewModel> SubmitLevelUpAssessmentAsync(int studentId, int testId, Dictionary<int, string> answers);
    }
}
