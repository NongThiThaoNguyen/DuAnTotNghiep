using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Student.Controllers;

[Area("Student")]
[Authorize(Roles = "STUDENT")]
public class ScheduleController : Controller
{
    private readonly IClassEnrollmentService _enrollmentService;

    public ScheduleController(IClassEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    public async Task<IActionResult> Index(string? weekStart)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var selectedDate = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(weekStart)
            && DateTime.TryParseExact(weekStart, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            selectedDate = parsedDate;
        }

        var monday = selectedDate.Date.AddDays(selectedDate.DayOfWeek == DayOfWeek.Sunday
            ? -6
            : DayOfWeek.Monday - selectedDate.DayOfWeek);
        await _enrollmentService.EnsureStudentSchedulesAsync(userId);
        var vm = await _enrollmentService.GetStudentScheduleAsync(userId, monday);
        return View(vm);
    }
}