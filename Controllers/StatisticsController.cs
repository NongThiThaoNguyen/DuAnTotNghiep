using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            int userId = GetCurrentUserId();
            if (userId == 0) return RedirectToAction("Login", "Account");

            var studyLogs = await _context.StudyActivityLogs
                .Where(log => log.StudentId == userId)
                .AsNoTracking()
                .ToListAsync();

            int totalMinutes = studyLogs.Sum(l => l.DurationMinutes ?? 0);
            int totalLessons = studyLogs.Count(l => l.ActivityType == "LESSON" || l.ActivityType == "ARTICLE");

            var quizAttempts = await _context.QuizAttempts
                .Where(a => a.StudentId == userId && a.Score.HasValue)
                .AsNoTracking()
                .Select(a => a.Score!.Value)
                .ToListAsync();

            decimal avgScore = quizAttempts.Any() ? quizAttempts.Average() : 0m;
            decimal accuracy = avgScore * 10; // Accuracy out of 100%

            // Calculate weekly streak (same logic as dashboard)
            int weeklyStreak = CalculateStreak(studyLogs);

            // Group by day of week for current week (Monday to Sunday)
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var monday = today.AddDays(-1 * diff);

            var weeklyMinutes = new List<int>();
            for (int i = 0; i < 7; i++)
            {
                var targetDate = monday.AddDays(i);
                var targetDateOnly = DateOnly.FromDateTime(targetDate);
                
                int mins = studyLogs
                    .Where(l => DateOnly.FromDateTime(l.CreatedAt.Date) == targetDateOnly)
                    .Sum(l => l.DurationMinutes ?? 0);
                
                weeklyMinutes.Add(mins);
            }

            var vm = new StatisticsViewModel
            {
                TotalStudyMinutes = totalMinutes,
                TotalLessonsCompleted = totalLessons,
                AverageQuizAccuracy = accuracy,
                WeeklyStreak = weeklyStreak,
                WeeklyStudyMinutes = weeklyMinutes
            };

            return View(vm);
        }

        private static int CalculateStreak(List<StudyActivityLog> logs)
        {
            var studyDates = logs
                .Where(l => l.ActivityType != "LOGIN")
                .Select(l => DateOnly.FromDateTime(l.CreatedAt.Date))
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

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
    }
}
