using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.PlacementTest;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class PlacementTestService : IPlacementTestService
    {
        private readonly ILearningProfileRepository _profileRepository;
        private readonly IGenericRepository<PlacementTest> _testRepository;

        public PlacementTestService(
            ILearningProfileRepository profileRepository,
            IGenericRepository<PlacementTest> testRepository)
        {
            _profileRepository = profileRepository;
            _testRepository = testRepository;
        }

        public async Task<PlacementTestSuggestionViewModel?> BuildPlacementTestSuggestionAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            var activeTests = (await _testRepository.GetAllAsync())
                .Where(t => t.Status == "ACTIVE")
                .ToList();

            if (!activeTests.Any()) return null;

            PlacementTest? suggestedTest = null;

            // 1. Map theo Goal
            if (profile.MainGoal != null)
            {
                var goalKeyword = profile.MainGoal.GoalCode ?? profile.MainGoal.GoalName;
                if (!string.IsNullOrEmpty(goalKeyword))
                {
                    suggestedTest = activeTests.FirstOrDefault(t => t.Title.Contains(goalKeyword, System.StringComparison.OrdinalIgnoreCase));
                }
            }

            // 2. Fallback sang General nếu không tìm thấy theo Goal
            if (suggestedTest == null)
            {
                suggestedTest = activeTests.FirstOrDefault(t => t.Title.Contains("General", System.StringComparison.OrdinalIgnoreCase));
            }

            // 3. Fallback lấy test đầu tiên
            if (suggestedTest == null)
            {
                suggestedTest = activeTests.FirstOrDefault();
            }

            return new PlacementTestSuggestionViewModel
            {
                GoalName = profile.MainGoal?.GoalName ?? "Chưa xác định",
                CurrentLevelName = profile.CurrentLevel?.Name ?? "Chưa đánh giá",
                PrioritySkills = profile.StudentSkillPreferences.OrderBy(s => s.PriorityLevel).Select(s => s.SkillCode).ToList(),
                SuggestedTestId = suggestedTest!.Id,
                SuggestedTestTitle = suggestedTest.Title,
                SuggestedTestDescription = suggestedTest.Description,
                TimeLimitMinutes = suggestedTest.TimeLimitMinutes
            };
        }
    }
}
