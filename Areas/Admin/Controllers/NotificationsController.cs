using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models;
using System.Linq;
using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public NotificationsController(INotificationService notificationService, IUserService userService, ApplicationDbContext context)
        {
            _notificationService = notificationService;
            _userService = userService;
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;
            var notifications = await _notificationService.GetAllAsync(page, pageSize);
            var totalCount = await _notificationService.GetTotalCountAsync();

            var viewModel = new NotificationListViewModel
            {
                Notifications = notifications,
                CurrentPage = page,
                TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize),
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateNotificationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (model.TargetType == "SPECIFIC_USER")
            {
                if (!model.TargetUserId.HasValue)
                {
                    ModelState.AddModelError("TargetUserId", "Vui lòng nhập ID người dùng cụ thể.");
                    return View(model);
                }

                await _notificationService.SendToUserAsync(model.Title, model.Content, model.NotificationType, model.TargetUserId.Value, currentUserId);
            }
            else if (model.TargetType == "STUDENTS")
            {
                await _notificationService.SendToAllStudentsAsync(model.Title, model.Content, model.NotificationType, currentUserId);
            }
            else if (model.TargetType == "TEACHERS")
            {
                var teacherUsers = await _context.Users.Where(u => u.Role.RoleCode == "TEACHER").ToListAsync();
                foreach(var u in teacherUsers)
                {
                    await _notificationService.SendToUserAsync(model.Title, model.Content, model.NotificationType, u.Id, currentUserId);
                }
            }
            else // ALL
            {
                var allUsers = await _context.Users.ToListAsync();
                foreach(var u in allUsers)
                {
                    await _notificationService.SendToUserAsync(model.Title, model.Content, model.NotificationType, u.Id, currentUserId);
                }
            }

            TempData["SuccessMessage"] = "Gửi thông báo thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var notification = await _notificationService.GetByIdAsync(id);
            if (notification == null) return NotFound();

            return View(notification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _notificationService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Đã xóa thông báo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
