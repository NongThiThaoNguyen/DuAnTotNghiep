using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("STUDENT"))
            {
                return RedirectToAction("Index", "Home", new { area = "Student" });
            }
            else if (User.IsInRole("ADMIN"))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            else if (User.IsInRole("TEACHER"))
            {
                return RedirectToAction("Index", "Home", new { area = "Teacher" });
            }
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
