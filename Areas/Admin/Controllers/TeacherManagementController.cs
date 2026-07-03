using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class TeacherManagementController : Controller
{
    private readonly IAdminTeacherManagementService _teacherManagementService;

    public TeacherManagementController(IAdminTeacherManagementService teacherManagementService)
    {
        _teacherManagementService = teacherManagementService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _teacherManagementService.GetTeacherListAsync());
    }

    public async Task<IActionResult> Profile(int teacherId)
    {
        var model = await _teacherManagementService.GetTeacherProfileAsync(teacherId);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Performance(int teacherId)
    {
        var model = await _teacherManagementService.GetTeacherPerformanceAsync(teacherId);
        return model == null ? NotFound() : View(model);
    }
}
