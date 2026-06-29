using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
<<<<<<< HEAD
=======
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class HomeController : Controller
    {
<<<<<<< HEAD
        public IActionResult Index()
        {
            // Trỏ thẳng về trang Quản lý User vì Admin hiện tại chỉ có chức năng này
            return RedirectToAction("Index", "User", new { area = "Admin" });
=======
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await _dashboardService.GetDashboardDataAsync();
            return View(viewModel);
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        }
    }
}
