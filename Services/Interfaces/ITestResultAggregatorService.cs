using System.Threading.Tasks;
using DuAnTotNghiep.ViewModels;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITestResultAggregatorService
    {
        /// <summary>
        /// Aggregates and normalizes placement test attempt data for AI input.
        /// Performs ownership checks, completion status validation, and mathematical aggregation.
        /// </summary>
        /// <param name="attemptId">The ID of the placement test attempt.</param>
        /// <param name="currentUserId">The ID of the user requesting this assessment.</param>
        /// <returns>A structured AssessmentInputDto containing quantitative and qualitative data.</returns>
        Task<AssessmentInputDto> AggregateTestDataAsync(int attemptId, int currentUserId);
    }
}
