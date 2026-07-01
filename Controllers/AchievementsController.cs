using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class AchievementsController : Controller
    {
        private readonly IGamificationService _gamificationService;

        public AchievementsController(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
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

            var badges = await _gamificationService.GetUserBadgesAsync(userId);
            int earnedCount = badges.Count(b => b.IsUnlocked);

            var vm = new AchievementsViewModel
            {
                EarnedCount = earnedCount,
                Badges = badges
            };

            return View(vm);
        }
    }
}
