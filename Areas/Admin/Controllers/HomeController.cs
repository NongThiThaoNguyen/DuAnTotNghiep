using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Trỏ thẳng về trang Quản lý User vì Admin hiện tại chỉ có chức năng này
            return RedirectToAction("Index", "User", new { area = "Admin" });
        }
    }
}
