using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class AchievementsController : Controller
    {
        private readonly IAchievementService _achievementService;
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;

        public AchievementsController(IAchievementService achievementService, IUserService userService, IAuditService auditService)
        {
            _achievementService = achievementService;
            _userService = userService;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var achievements = await _achievementService.GetAllAsync();
            return View(achievements);
        }

        public IActionResult Create()
        {
            return View(new AchievementFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AchievementFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var achievement = new Achievement
                {
                    Code = model.Code,
                    Title = model.Title,
                    Description = model.Description,
                    IconUrl = model.IconUrl,
                    XpReward = model.XpReward,
                    IsActive = model.IsActive
                };

                await _achievementService.CreateAsync(achievement);

                var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _auditService.LogAsync(adminId, "CREATE_ACHIEVEMENT", "Achievement", achievement.Id, null, achievement.Title);

                TempData["SuccessMessage"] = "Thêm huy hiệu thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var achievement = await _achievementService.GetByIdAsync(id);
            if (achievement == null) return NotFound();

            var model = new AchievementFormViewModel
            {
                Id = achievement.Id,
                Code = achievement.Code,
                Title = achievement.Title,
                Description = achievement.Description,
                IconUrl = achievement.IconUrl,
                XpReward = achievement.XpReward,
                IsActive = achievement.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AchievementFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var achievement = await _achievementService.GetByIdAsync(id);
                if (achievement == null) return NotFound();

                var oldTitle = achievement.Title;

                achievement.Code = model.Code;
                achievement.Title = model.Title;
                achievement.Description = model.Description;
                achievement.IconUrl = model.IconUrl;
                achievement.XpReward = model.XpReward;
                achievement.IsActive = model.IsActive;

                await _achievementService.UpdateAsync(achievement);

                var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _auditService.LogAsync(adminId, "UPDATE_ACHIEVEMENT", "Achievement", achievement.Id, oldTitle, achievement.Title);

                TempData["SuccessMessage"] = "Cập nhật huy hiệu thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var achievement = await _achievementService.GetByIdAsync(id);
            if (achievement != null)
            {
                await _achievementService.DeleteAsync(id);

                var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
                await _auditService.LogAsync(adminId, "DELETE_ACHIEVEMENT", "Achievement", id, achievement.Title, null);

                TempData["SuccessMessage"] = "Xóa huy hiệu thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UserAchievements(int userId)
        {
            var user = await _userService.GetUserDetailsAsync(userId);
            if (user == null) return NotFound();

            var userAchievements = await _achievementService.GetUserAchievementsAsync(userId);
            var allAchievements = await _achievementService.GetAllAsync();

            var model = new UserAchievementDetailViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = "/images/default-avatar.png",
                Achievements = allAchievements.Select(a => {
                    var ua = userAchievements.FirstOrDefault(u => u.AchievementId == a.Id);
                    return new UserAchievementItem
                    {
                        AchievementId = a.Id,
                        Title = a.Title,
                        IconUrl = a.IconUrl,
                        IsUnlocked = ua != null && ua.IsUnlocked,
                        UnlockedAtFormatted = ua?.UnlockedAt?.ToString("dd/MM/yyyy HH:mm")
                    };
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grant(int userId, int achievementId)
        {
            await _achievementService.GrantAchievementAsync(userId, achievementId);

            var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            await _auditService.LogAsync(adminId, "GRANT_ACHIEVEMENT", "UserAchievement", userId, null, $"AchievementId: {achievementId}");

            TempData["SuccessMessage"] = "Đã trao huy hiệu thành công.";
            return RedirectToAction(nameof(UserAchievements), new { userId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(int userId, int achievementId)
        {
            await _achievementService.RevokeAchievementAsync(userId, achievementId);

            var adminId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            await _auditService.LogAsync(adminId, "REVOKE_ACHIEVEMENT", "UserAchievement", userId, $"AchievementId: {achievementId}", null);

            TempData["SuccessMessage"] = "Đã thu hồi huy hiệu thành công.";
            return RedirectToAction(nameof(UserAchievements), new { userId = userId });
        }
    }
}
