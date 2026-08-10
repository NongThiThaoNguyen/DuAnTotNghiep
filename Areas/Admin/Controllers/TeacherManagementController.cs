using DuAnTotNghiep.Areas.Admin.ViewModels;
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

    public async Task<IActionResult> Index(string? keyword, string? status)
    {
        var model = await _teacherManagementService.GetTeacherListAsync(keyword, status);
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTeacherAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeacherAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, message) = await _teacherManagementService.CreateTeacherAsync(model);
        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", message);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int teacherId)
    {
        var model = await _teacherManagementService.GetTeacherForEditAsync(teacherId);
        if (model == null)
        {
            return NotFound();
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTeacherAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, message) = await _teacherManagementService.UpdateTeacherAsync(model);
        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", message);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(int teacherId)
    {
        var (success, message, _) = await _teacherManagementService.ToggleLockTeacherAsync(teacherId);
        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Profile(int teacherId)
    {
        var model = await _teacherManagementService.GetTeacherProfileAsync(teacherId);
        return model == null ? NotFound() : View(model);
    }

    public async Task<IActionResult> Details(int teacherId)
    {
        var model = await _teacherManagementService.GetTeacherProfileAsync(teacherId);
        return model == null ? NotFound() : View("Profile", model);
    }

    public async Task<IActionResult> Performance(int teacherId)
    {
        var model = await _teacherManagementService.GetTeacherPerformanceAsync(teacherId);
        return model == null ? NotFound() : View(model);
    }
}
