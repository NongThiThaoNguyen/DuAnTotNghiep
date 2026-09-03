using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;

        public UserController(
            IUserService userService,
            IUserProfileService profileService,
            ILearningProfileService learningProfileService,
            DuAnTotNghiep.Models.Repositories.Interfaces.IAuditLogRepository auditRepository,
            ApplicationDbContext context)
        {
            _userService = userService;
            _profileService = profileService;
            _learningProfileService = learningProfileService;
            _auditRepository = auditRepository;
            _context = context;
        }

        public async Task<IActionResult> Index([FromQuery] UserFilterViewModel filter)
        {
            var model = await _userService.GetPagedUsersAsync(filter);
            return View(model);
        }

        public async Task<IActionResult> StudentList(string? keyword, string? pathStatus, int? levelId, decimal? minAttendanceRate, int page = 1)
        {
            const int pageSize = 20;
            page = Math.Max(1, page);
            minAttendanceRate = minAttendanceRate.HasValue
                ? Math.Clamp(minAttendanceRate.Value, 0, 100)
                : null;

            var query = _context.Users.AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.StudentLearningProfile)
                    .ThenInclude(p => p!.CurrentLevel)
                .Include(u => u.StudentLearningPaths)
                .Where(u => u.Role.RoleCode == "STUDENT");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword));
            }

            if (levelId.HasValue)
            {
                query = query.Where(u => u.StudentLearningProfile != null && u.StudentLearningProfile.CurrentLevelId == levelId.Value);
            }

            if (!string.IsNullOrWhiteSpace(pathStatus))
            {
                query = query.Where(u => u.StudentLearningPaths.Any(p => p.Status == pathStatus));
            }

            var students = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var studentIds = students.Select(u => u.Id).ToList();

            var attendances = await _context.Attendances.AsNoTracking()
                .Where(a => studentIds.Contains(a.StudentId))
                .Select(a => new { a.StudentId, a.Status })
                .ToListAsync();

            var attendanceGroups = attendances
                .GroupBy(a => a.StudentId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Total = g.Count(),
                        Present = g.Count(a => a.Status == "PRESENT" || a.Status == "LATE")
                    });

            var testAttempts = await _context.TestAttempts.AsNoTracking()
                .Where(a => studentIds.Contains(a.StudentId) && (a.Status == "GRADED" || a.Status == "SUBMITTED"))
                .Select(a => new { a.StudentId, a.TotalScore, a.SubmittedAt, a.StartedAt })
                .ToListAsync();

            var latestAssessments = testAttempts
                .GroupBy(a => a.StudentId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Score = g.OrderByDescending(x => x.SubmittedAt ?? x.StartedAt).Select(x => x.TotalScore).FirstOrDefault()
                    });

            var rows = students.Select(student =>
            {
                attendanceGroups.TryGetValue(student.Id, out var attendance);
                var total = attendance?.Total ?? 0;
                var rate = total == 0 ? 0 : Math.Round(attendance!.Present * 100m / total, 2);
                var latestPath = student.StudentLearningPaths.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();

                string evalScore = "Chưa có";
                if (latestAssessments.TryGetValue(student.Id, out var assess) && assess.Score.HasValue)
                {
                    evalScore = $"{assess.Score.Value:0.#}";
                }
                else if (student.StudentLearningProfile?.TargetScore.HasValue == true)
                {
                    evalScore = $"{student.StudentLearningProfile.TargetScore.Value:0.#}";
                }

                return new StudentListItemViewModel
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    Status = student.Status,
                    LevelName = student.StudentLearningProfile?.CurrentLevel?.Name ?? "Chưa có",
                    EvaluationScore = evalScore,
                    PathStatus = latestPath?.Status ?? "Chưa có",
                    AttendanceRate = rate,
                    CreatedAt = student.CreatedAt
                };
            });

            if (minAttendanceRate.HasValue)
            {
                rows = rows.Where(row => row.AttendanceRate >= minAttendanceRate.Value);
            }

            var materializedRows = rows.ToList();
            var model = new StudentManagementListViewModel
            {
                Keyword = keyword,
                PathStatus = pathStatus,
                LevelId = levelId,
                MinAttendanceRate = minAttendanceRate,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = materializedRows.Count,
                Levels = await _context.EnglishProficiencyLevels.AsNoTracking()
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new AdminOptionViewModel { Id = l.Id, Text = l.Name })
                    .ToListAsync(),
                Items = materializedRows
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
            };

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
        public async Task<IActionResult> ToggleLock(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.ToggleLockAsync(id, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã thay đổi trạng thái khóa tài khoản thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể thay đổi trạng thái khóa tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _userService.ChangeRoleAsync(model.UserId, model.NewRoleId, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Đã thay đổi phân quyền thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể thay đổi phân quyền cho tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ResetPassword(int id)
        {
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Mật khẩu không được để trống.";
                return RedirectToAction("ResetPassword", new { id });
            }

            var result = await _userService.AdminResetPasswordAsync(id, newPassword, adminId, ipAddress);
            if (result)
            {
                TempData["SuccessMessage"] = "Mật khẩu mới đã được thiết lập.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể reset mật khẩu cho tài khoản này.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Statistics(int id)
        {
            var adminIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(adminIdString, out int adminId);
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var stats = await _userService.GetUserStatisticsAsync(id);
            if (stats == null) return NotFound();

            await _auditRepository.AddAsync(new DuAnTotNghiep.Models.AuditLog
            {
                UserId = id,
                Action = "ADMIN_VIEW_USER_STATISTICS",
                EntityName = "User",
                EntityId = id,
                NewValue = $"Viewed by Admin ID {adminId}",
                IpAddress = ipAddress,
                CreatedAt = System.DateTime.UtcNow
            });
            await _auditRepository.SaveChangesAsync();

            return View(stats);
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
