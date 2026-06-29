using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Models.ViewModels.PlacementTest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestService
    {
        Task<PlacementTestSuggestionViewModel?> BuildPlacementTestSuggestionAsync(int userId);
        Task<PlacementTestDto?> GetAvailableTestForStudentAsync(int studentId);
        Task<TestAttemptDto> StartAttemptAsync(int studentId, int placementTestId);
        Task<TestAttemptDto?> GetCurrentAttemptAsync(int studentId, int placementTestId);
        Task<bool> CanStartAttemptAsync(int studentId, int placementTestId);
        Task<TestTakingViewModel?> GetTestTakingViewModelAsync(int attemptId, int studentId);
        Task<SaveAnswerResultDto> SaveAnswerAsync(SaveAnswerInputDto input, int studentId);
        Task<SubmitResultDto> SubmitAttemptAsync(int attemptId, int studentId, List<AnswerInputDto> answers);
        Task<TestResultViewModel> GetResultForStudentAsync(int attemptId, int studentId);
    }
}
