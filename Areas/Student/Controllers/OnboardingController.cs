using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.DTOs.Onboarding;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class OnboardingController : Controller
    {
        private readonly ILearningProfileService _profileService;

        public OnboardingController(ILearningProfileService profileService)
        {
            _profileService = profileService;
        }

        private int GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            throw new System.Exception("Không tìm thấy thông tin định danh của người dùng.");
        }

        // Helper Method: Chống Bypass (nhảy cóc) step
        private string? GetMissingStep(LearningProfileDto? profile)
        {
            if (profile == null || profile.MainGoal == null) return "Goal";
            // Bắt buộc phải có TargetLevel hoặc TargetScore
            if (profile.TargetLevel == null && profile.TargetScore == null) return "Level";
            if (profile.PrioritySkills == null || profile.PrioritySkills.Count == 0) return "Skills";
            if (profile.DailyStudyMinutes == null || profile.WeeklyStudyDays == null) return "StudyTime";
            
            return null;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Start");
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            if (profile != null && profile.OnboardingStatus == "COMPLETED")
            {
                // Nếu đã xong Onboarding, đưa về Dashboard
                return RedirectToAction("Index", "Home", new { area = "Student" });
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Goal()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            
            ViewBag.Goals = await _profileService.GetActiveGoalsAsync();

            var model = new OnboardingStep1ViewModel
            {
                MainGoalId = profile?.MainGoal?.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Goal(OnboardingStep1ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Goals = await _profileService.GetActiveGoalsAsync();
                return View(model);
            }

            var success = await _profileService.SaveStepAsync(GetUserId(), model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu dữ liệu. Vui lòng thử lại.");
                ViewBag.Goals = await _profileService.GetActiveGoalsAsync();
                return View(model);
            }

            return RedirectToAction("Level");
        }

        [HttpGet]
        public async Task<IActionResult> Level()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            var missing = GetMissingStep(profile);
            
            // Nếu step bị thiếu trước đó là Goal -> Ép về Goal
            if (missing != null && missing != "Level" && missing == "Goal") return RedirectToAction(missing);

            ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
            ViewBag.GoalCode = profile?.MainGoal?.GoalCode; // Truyền GoalCode cho View

            var model = new OnboardingStep2ViewModel
            {
                CurrentLevelId = profile?.CurrentLevel?.Id,
                TargetLevelId = profile?.TargetLevel?.Id,
                TargetScore = profile?.TargetScore,
                GoalCode = profile?.MainGoal?.GoalCode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Level(OnboardingStep2ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
                ViewBag.GoalCode = model.GoalCode;
                return View(model);
            }

            var success = await _profileService.SaveStepAsync(GetUserId(), model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu dữ liệu. Vui lòng thử lại.");
                ViewBag.Levels = await _profileService.GetActiveLevelsAsync();
                ViewBag.GoalCode = model.GoalCode;
                return View(model);
            }

            return RedirectToAction("Skills");
        }

        [HttpGet]
        public async Task<IActionResult> Skills()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            var missing = GetMissingStep(profile);
            if (missing != null && missing != "Skills") return RedirectToAction(missing);

            ViewBag.Skills = await _profileService.GetActiveSkillsAsync();

            var model = new OnboardingStep3ViewModel
            {
                SelectedSkillCodes = profile?.PrioritySkills?.Select(s => s.SkillCode).ToList() ?? new System.Collections.Generic.List<string>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Skills(OnboardingStep3ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Skills = await _profileService.GetActiveSkillsAsync();
                return View(model);
            }

            var success = await _profileService.SaveStepAsync(GetUserId(), model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Danh mục kỹ năng đã thay đổi hoặc có lỗi xảy ra. Vui lòng chọn lại.");
                ViewBag.Skills = await _profileService.GetActiveSkillsAsync();
                return View(model);
            }

            return RedirectToAction("StudyTime");
        }

        [HttpGet]
        public async Task<IActionResult> StudyTime()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            var missing = GetMissingStep(profile);
            if (missing != null && missing != "StudyTime") return RedirectToAction(missing);

            var model = new OnboardingStep4ViewModel
            {
                DailyStudyMinutes = profile?.DailyStudyMinutes,
                WeeklyStudyDays = profile?.WeeklyStudyDays,
                PreferredStudyTime = profile?.PreferredStudyTime
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudyTime(OnboardingStep4ViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _profileService.SaveStepAsync(GetUserId(), model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu dữ liệu. Vui lòng thử lại.");
                return View(model);
            }

            return RedirectToAction("Confirm");
        }

        [HttpGet]
        public async Task<IActionResult> Confirm()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            var missing = GetMissingStep(profile);
            if (missing != null) return RedirectToAction(missing);

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete()
        {
            var profile = await _profileService.GetProfileByUserIdAsync(GetUserId());
            var missing = GetMissingStep(profile);
            if (missing != null) return RedirectToAction(missing);

            var success = await _profileService.MarkOnboardingCompletedAsync(GetUserId());
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Không thể hoàn tất quá trình thiết lập. Vui lòng thử lại.");
                return View("Confirm", profile);
            }

            return RedirectToAction("Index", "Home", new { area = "Student" });
        }
    }
}
