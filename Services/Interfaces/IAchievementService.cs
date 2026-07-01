using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAchievementService
    {
        Task<List<Achievement>> GetAllAsync();
        Task<Achievement?> GetByIdAsync(int id);
        Task CreateAsync(Achievement achievement);
        Task UpdateAsync(Achievement achievement);
        Task DeleteAsync(int id);
        Task<List<UserAchievement>> GetUserAchievementsAsync(int userId);
        Task GrantAchievementAsync(int userId, int achievementId);
        Task RevokeAchievementAsync(int userId, int achievementId);
    }
}
