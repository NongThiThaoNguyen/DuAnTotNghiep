using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.ViewModels.AILearn;

namespace DuAnTotNghiep.Controllers;

[Authorize]
public class AchievementsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AchievementsController(ApplicationDbContext context)
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

        var userAchievements = await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var badges = userAchievements.Select(ua => new AchievementBadgeViewModel
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

        int earnedCount = badges.Count(b => b.IsUnlocked);

        var vm = new AchievementsViewModel
        {
            EarnedCount = earnedCount,
            Badges = badges
        };

        return View(vm);
    }
}
