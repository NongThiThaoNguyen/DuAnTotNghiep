using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class ReportsController : Controller
{
    private readonly IAdminReportService _reportService;

    public ReportsController(IAdminReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<IActionResult> Index(string? period, DateTime? fromDate, DateTime? toDate)
    {
        return View(await _reportService.GetOverviewAsync(period, fromDate, toDate));
    }

    public async Task<IActionResult> StudentProgress()
    {
        return View(await _reportService.GetStudentProgressReportAsync());
    }

    public async Task<IActionResult> TeacherActivity()
    {
        return View(await _reportService.GetTeacherActivityReportAsync());
    }



    public async Task<IActionResult> QuizPerformance()
    {
        return View(await _reportService.GetQuizPerformanceReportAsync());
    }
}
