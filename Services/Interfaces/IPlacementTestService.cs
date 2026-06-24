using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.ViewModels.PlacementTest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPlacementTestService
    {
        Task<PlacementTestSuggestionViewModel?> BuildPlacementTestSuggestionAsync(int userId);
        Task<PlacementTestDto?> GetAvailableTestForStudentAsync(int studentId);
        Task<TestAttemptDto> StartAttemptAsync(int studentId, int placementTestId);
        Task<SubmitResultDto> SubmitAttemptAsync(int attemptId, int studentId, List<AnswerInputDto> answers);
    }
}
