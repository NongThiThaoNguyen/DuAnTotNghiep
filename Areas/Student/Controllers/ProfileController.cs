using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Onboarding;
using System.Linq;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class ProfileController : Controller
    {
        private readonly ILearningProfileService _profileService;

        public ProfileController(ILearningProfileService profileService)
        {
            _profileService = profileService;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> EditLearningProfile()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            if (profile == null) return RedirectToAction("Index", "Onboarding");

            var model = new UpdateLearningProfileViewModel
            {
                MainGoalId = profile.MainGoal?.Id,
                CurrentLevelId = profile.CurrentLevel?.Id,
                TargetLevelId = profile.TargetLevel?.Id,
                TargetScore = profile.TargetScore,
                DailyStudyMinutes = profile.DailyStudyMinutes,
                WeeklyStudyDays = profile.WeeklyStudyDays,
                PreferredStudyTime = profile.PreferredStudyTime,
                LearningNote = profile.LearningNote,
                SelectedSkillCodes = profile.PrioritySkills?.Select(s => s.SkillCode).ToList() ?? new System.Collections.Generic.List<string>()
            };

            ViewBag.Goals = await _profileService.GetActiveGoalsAsync();
            ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
            ViewBag.Skills = await _profileService.GetActiveSkillsAsync();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLearningProfile(UpdateLearningProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Goals = await _profileService.GetActiveGoalsAsync();
                ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
                ViewBag.Skills = await _profileService.GetActiveSkillsAsync();
                return View(model);
            }

            var success = await _profileService.EditLearningProfileAsync(GetUserId(), model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể cập nhật hồ sơ. Vui lòng kiểm tra lại thông tin và thử lại.");
                ViewBag.Goals = await _profileService.GetActiveGoalsAsync();
                ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
                ViewBag.Skills = await _profileService.GetActiveSkillsAsync();
                return View(model);
            }

            TempData["SuccessMessage"] = "Cập nhật hồ sơ học tập thành công!";
            return RedirectToAction("Index", "Home");
        }
    }
}
