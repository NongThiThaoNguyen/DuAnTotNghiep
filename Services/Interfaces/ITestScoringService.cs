using DuAnTotNghiep.DTOs.PlacementTest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITestScoringService
    {
        Task<ScoreResultDto> GradeAttemptAsync(int attemptId);
        Task<List<SkillScoreDto>> CalculateSkillScoresAsync(int attemptId);
        Task<List<TopicScoreDto>> CalculateTopicScoresAsync(int attemptId);
        Task<EstimatedLevelDto> EstimateLevelAsync(int attemptId);
    }
}
