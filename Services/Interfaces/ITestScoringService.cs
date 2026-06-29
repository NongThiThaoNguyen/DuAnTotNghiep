using DuAnTotNghiep.DTOs.PlacementTest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface ITestScoringService
    {
        Task<ScoreResultDto> GradeAttemptAsync(int attemptId);
        Task<List<SkillScoreDto>> CalculateSkillScoresAsync(int attemptId);
<<<<<<< HEAD
<<<<<<< HEAD
=======
        Task<List<TopicScoreDto>> CalculateTopicScoresAsync(int attemptId);
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
=======
        Task<List<TopicScoreDto>> CalculateTopicScoresAsync(int attemptId);
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        Task<EstimatedLevelDto> EstimateLevelAsync(int attemptId);
    }
}
