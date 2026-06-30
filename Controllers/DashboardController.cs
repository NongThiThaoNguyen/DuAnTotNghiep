using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IStudentDashboardService _dashboardService;

        public DashboardController(IStudentDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
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

            var vm = await _dashboardService.GetDashboardAsync(userId);
            return View(vm);
        }
    }
}
