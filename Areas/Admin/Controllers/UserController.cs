using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IUserProfileService _profileService;
        private readonly ILearningProfileService _learningProfileService;
        private readonly DuAnTotNghiep.Models.Repositories.Interfaces.IAuditLogRepository _auditRepository;

        public UserController(
            IUserService userService, 
            IUserProfileService profileService,
            ILearningProfileService learningProfileService,
            DuAnTotNghiep.Models.Repositories.Interfaces.IAuditLogRepository auditRepository)
        {
            _userService = userService;
            _profileService = profileService;
            _learningProfileService = learningProfileService;
            _auditRepository = auditRepository;
        }

        public async Task<IActionResult> Index([FromQuery] UserFilterViewModel filter)
        {
            var model = await _userService.GetPagedUsersAsync(filter);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userDetails = await _profileService.GetAdminUserProfileAsync(id);
                return View(userDetails);
            }
            catch (DuAnTotNghiep.Models.Exceptions.NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.LockUserAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Khóa tài khoản thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể khóa tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.UnlockUserAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Mở khóa tài khoản thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể mở khóa tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.ResetPasswordAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Mật khẩu mới đã được tạo và gửi qua email cho người dùng.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể reset mật khẩu cho tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LearningProfile(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "System";

            try
            {
                var profile = await _learningProfileService.GetProfileByUserIdAsync(id);
                if (profile == null)
                {
                    TempData["ErrorMessage"] = "Người dùng này chưa có hồ sơ học tập.";
                    return RedirectToAction("Details", new { id });
                }

                await _auditRepository.AddAsync(new DuAnTotNghiep.Models.AuditLog
                {
                    UserId = id,
                    Action = "ADMIN_VIEW_LEARNING_PROFILE",
                    EntityName = "StudentLearningProfile",
                    EntityId = profile.Id,
                    OldValue = "Hidden",
                    NewValue = $"Viewed by Admin ID {adminId}",
                    IpAddress = ipAddress,
                    CreatedAt = System.DateTime.UtcNow
                });
                await _auditRepository.SaveChangesAsync();

                var auditLogs = await _auditRepository.GetAuditsByUserAsync(id);
                ViewBag.AuditLogs = auditLogs
                    .Where(a => a.EntityName == "StudentLearningProfile")
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();

                ViewBag.StudentId = id;
                return View(profile);
            }
            catch (DuAnTotNghiep.Models.Exceptions.NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetOnboarding(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "System";

            var result = await _learningProfileService.ResetOnboardingStatusAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã reset trạng thái Onboarding về Bắt đầu.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể reset trạng thái Onboarding (hồ sơ không tồn tại hoặc lỗi).";
            }

            return RedirectToAction("LearningProfile", new { id });
        }
    }
}
