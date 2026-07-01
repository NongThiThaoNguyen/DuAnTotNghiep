using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly ApplicationDbContext _context;

        public GamificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task EnsureAchievementsSeededAsync()
        {
            var existingCodes = await _context.Achievements
                .Select(a => a.Code)
                .ToListAsync();

            var definitions = new List<Achievement>
            {
                new Achievement 
                { 
                    Code = "FIRST_STEP", 
                    Title = "Khởi đầu", 
                    Description = "Tích lũy được 100 XP học tập đầu tiên.", 
                    IconUrl = "🌱", 
                    XpReward = 100, 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Achievement 
                { 
                    Code = "STUDIOUS", 
                    Title = "Chăm chỉ", 
                    Description = "Hoàn thành 5 bài học trên hệ thống.", 
                    IconUrl = "📚", 
                    XpReward = 200, 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Achievement 
                { 
                    Code = "BREAKTHROUGH", 
                    Title = "Bứt phá", 
                    Description = "Hoàn thành 3 bài kiểm tra Quiz.", 
                    IconUrl = "⚡", 
                    XpReward = 300, 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow 
                },
                new Achievement 
                { 
                    Code = "PERSISTENT", 
                    Title = "Kiên trì", 
                    Description = "Duy trì chuỗi học tập liên tục 5 ngày.", 
                    IconUrl = "🔥", 
                    XpReward = 500, 
                    IsActive = true, 
                    CreatedAt = DateTime.UtcNow 
                }
            };

            foreach (var def in definitions)
            {
                if (!existingCodes.Contains(def.Code))
                {
                    _context.Achievements.Add(def);
                }
            }

            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> CalculateXpAsync(int userId)
        {
            var logs = await _context.StudyActivityLogs
                .Where(l => l.StudentId == userId)
                .ToListAsync();

            int studyMinutes = logs.Sum(l => l.DurationMinutes ?? 0);
            int completedLessons = logs.Count(l => l.ActivityType == "LESSON" || l.ActivityType == "ARTICLE");
            int completedQuizzes = logs.Count(l => l.ActivityType == "QUIZ");

            int unlockedAchievementsXp = await _context.UserAchievements
                .Where(ua => ua.UserId == userId && ua.IsUnlocked)
                .SumAsync(ua => ua.Achievement.XpReward);

            return studyMinutes * 10 + completedLessons * 50 + completedQuizzes * 100 + unlockedAchievementsXp;
        }

        public async Task<int> CalculateStreakAsync(int userId)
        {
            var studyDates = await _context.StudyActivityLogs
                .Where(l => l.StudentId == userId && l.ActivityType != "LOGIN")
                .Select(l => DateOnly.FromDateTime(l.CreatedAt.Date))
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            if (studyDates.Count == 0)
            {
                return 0;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var yesterday = today.AddDays(-1);
            if (studyDates[0] != today && studyDates[0] != yesterday)
            {
                return 0;
            }

            int streak = 1;
            for (int i = 0; i < studyDates.Count - 1; i++)
            {
                if (studyDates[i].AddDays(-1) == studyDates[i + 1])
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<string> GetRankTierAsync(int userId)
        {
            int xp = await CalculateXpAsync(userId);
            if (xp >= 3000) return "Gold";
            if (xp >= 1000) return "Silver";
            return "Bronze";
        }

        public async Task<int> GetLevelAsync(int userId)
        {
            int xp = await CalculateXpAsync(userId);
            return xp / 1000 + 1;
        }

        public async Task CheckAndGrantAchievementsAsync(int userId)
        {
            await EnsureAchievementsSeededAsync();

            var achievements = await _context.Achievements.Where(a => a.IsActive).ToListAsync();
            var userAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            // Calculate current progress metrics
            int totalXp = await CalculateXpAsync(userId);
            int streak = await CalculateStreakAsync(userId);

            var logs = await _context.StudyActivityLogs.Where(l => l.StudentId == userId).ToListAsync();
            int completedLessons = logs.Count(l => l.ActivityType == "LESSON" || l.ActivityType == "ARTICLE");
            int completedQuizzes = logs.Count(l => l.ActivityType == "QUIZ");

            foreach (var ach in achievements)
            {
                var userAch = userAchievements.FirstOrDefault(ua => ua.AchievementId == ach.Id);
                int target = ach.Code switch
                {
                    "FIRST_STEP" => 100,
                    "STUDIOUS" => 5,
                    "BREAKTHROUGH" => 3,
                    "PERSISTENT" => 5,
                    _ => 999
                };

                int progress = ach.Code switch
                {
                    "FIRST_STEP" => totalXp,
                    "STUDIOUS" => completedLessons,
                    "BREAKTHROUGH" => completedQuizzes,
                    "PERSISTENT" => streak,
                    _ => 0
                };

                if (userAch == null)
                {
                    userAch = new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = ach.Id,
                        IsUnlocked = false,
                        ProgressValue = progress,
                        TargetValue = target
                    };
                    _context.UserAchievements.Add(userAch);
                }
                else
                {
                    userAch.ProgressValue = progress;
                }

                if (progress >= target && !userAch.IsUnlocked)
                {
                    userAch.IsUnlocked = true;
                    userAch.UnlockedAt = DateTime.UtcNow;

                    // Trigger notification
                    var notification = new Notification
                    {
                        TargetUserId = userId,
                        Title = "Đã mở khóa Thành tích!",
                        Content = $"Chúc mừng bạn đã nhận được Huy hiệu '{ach.Title}' ({ach.IconUrl}) và nhận thêm {ach.XpReward} XP!",
                        NotificationType = "SYSTEM",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notification);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<AchievementBadgeViewModel>> GetUserBadgesAsync(int userId)
        {
            await EnsureAchievementsSeededAsync();
            await CheckAndGrantAchievementsAsync(userId);

            var userAchievements = await _context.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            return userAchievements.Select(ua => new AchievementBadgeViewModel
            {
                Id = ua.AchievementId,
                Code = ua.Achievement.Code,
                Title = ua.Achievement.Title,
                Description = ua.Achievement.Description,
                IconUrl = ua.Achievement.IconUrl,
                XpReward = ua.Achievement.XpReward,
                IsUnlocked = ua.IsUnlocked,
                UnlockedAtLabel = ua.UnlockedAt?.ToString("dd/MM/yyyy"),
                ProgressValue = ua.ProgressValue,
                TargetValue = ua.TargetValue
            }).ToList();
        }
    }
}
