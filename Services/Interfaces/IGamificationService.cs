using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IGamificationService
    {
        Task<int> CalculateXpAsync(int userId);
        Task<int> CalculateStreakAsync(int userId);
        Task<string> GetRankTierAsync(int userId);
        Task<int> GetLevelAsync(int userId);
        Task CheckAndGrantAchievementsAsync(int userId);
        Task<List<AchievementBadgeViewModel>> GetUserBadgesAsync(int userId);
    }
}
