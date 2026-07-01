using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly ApplicationDbContext _context;

        public AchievementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Achievement>> GetAllAsync()
        {
            return await _context.Achievements
                .AsNoTracking()
                .OrderBy(a => a.Code)
                .ToListAsync();
        }

        public async Task<Achievement?> GetByIdAsync(int id)
        {
            return await _context.Achievements.FindAsync(id);
        }

        public async Task CreateAsync(Achievement achievement)
        {
            achievement.CreatedAt = DateTime.UtcNow;
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Achievement achievement)
        {
            _context.Achievements.Update(achievement);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement != null)
            {
                _context.Achievements.Remove(achievement);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserAchievement>> GetUserAchievementsAsync(int userId)
        {
            return await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(ua => ua.UnlockedAt)
                .ToListAsync();
        }

        public async Task GrantAchievementAsync(int userId, int achievementId)
        {
            var userAchievement = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            if (userAchievement == null)
            {
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement == null) return;

                userAchievement = new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievementId,
                    IsUnlocked = true,
                    UnlockedAt = DateTime.UtcNow,
                    ProgressValue = achievement.XpReward,
                    TargetValue = achievement.XpReward
                };
                _context.UserAchievements.Add(userAchievement);
                await _context.SaveChangesAsync();
            }
            else if (!userAchievement.IsUnlocked)
            {
                userAchievement.IsUnlocked = true;
                userAchievement.UnlockedAt = DateTime.UtcNow;
                _context.UserAchievements.Update(userAchievement);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAchievementAsync(int userId, int achievementId)
        {
            var userAchievement = await _context.UserAchievements
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

            if (userAchievement != null && userAchievement.IsUnlocked)
            {
                userAchievement.IsUnlocked = false;
                userAchievement.UnlockedAt = null;
                _context.UserAchievements.Update(userAchievement);
                await _context.SaveChangesAsync();
            }
        }
    }
}
