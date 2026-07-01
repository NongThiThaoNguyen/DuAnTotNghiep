using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;
using System.Security.Claims;

namespace DuAnTotNghiep.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "TEACHER")]
    public class HomeController : Controller
    {
        private readonly ITeacherDashboardService _dashboardService;

        public HomeController(ITeacherDashboardService dashboardService)
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
            int teacherId = GetCurrentUserId();
            if (teacherId == 0) return RedirectToAction("Login", "Account", new { area = "" });

            var model = await _dashboardService.GetDashboardAsync(teacherId);
            return View(model);
        }
    }
}
