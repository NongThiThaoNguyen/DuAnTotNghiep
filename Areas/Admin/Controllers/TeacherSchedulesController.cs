using DuAnTotNghiep.Areas.Admin.ViewModels;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class TeacherSchedulesController : Controller
{
    private readonly IAdminTeacherScheduleService _scheduleService;
    private readonly IAdminTeacherScheduleAiService _aiScheduleService;

    public TeacherSchedulesController(
        IAdminTeacherScheduleService scheduleService,
        IAdminTeacherScheduleAiService aiScheduleService)
    {
        _scheduleService = scheduleService;
        _aiScheduleService = aiScheduleService;
    }

    public async Task<IActionResult> Index([FromQuery] AdminTeacherScheduleFilterViewModel filter)
    {
        var model = await _scheduleService.GetSchedulesAsync(filter);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateAiSuggestions(AdminTeacherScheduleAiRequestViewModel aiRequest)
    {
        var model = await _scheduleService.GetSchedulesAsync(new AdminTeacherScheduleFilterViewModel
        {
            TeacherId = aiRequest.TeacherId,
            FromDate = aiRequest.FromDate,
            ToDate = aiRequest.ToDate
        });
        model.AiRequest = aiRequest;

        if (!ModelState.IsValid)
        {
            model.AiErrorMessage = "Vui lòng kiểm tra lại thông tin yêu cầu AI.";
            return View(nameof(Index), model);
        }

        try
        {
            var result = await _aiScheduleService.GenerateSuggestionsAsync(aiRequest);
            model.AiSuggestions = result.Suggestions;
            model.AiSummary = result.Summary ?? $"AI đã tạo {result.Suggestions.Count} gợi ý bằng mô hình {result.Model}.";
        }
        catch (InvalidOperationException ex)
        {
            model.AiErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            model.AiErrorMessage = "Không thể kết nối Gemini lúc này. Vui lòng kiểm tra API key hoặc thử lại sau.";
        }

        return View(nameof(Index), model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyAiSuggestion(AdminTeacherScheduleAiSuggestionViewModel suggestion)
    {
        try
        {
            await _scheduleService.CreateScheduleAsync(new AdminTeacherScheduleFormViewModel
            {
                TeacherId = suggestion.TeacherId,
                TopicId = suggestion.TopicId,
                Title = suggestion.Title,
                Description = suggestion.Description ?? suggestion.Reason,
                StartTime = suggestion.StartTime,
                EndTime = suggestion.EndTime,
                Classroom = suggestion.Classroom
            });

            TempData["SuccessMessage"] = "Đã áp dụng gợi ý AI vào lịch giáo viên.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Create()
    {
        var model = await _scheduleService.BuildCreateModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminTeacherScheduleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await _scheduleService.BuildCreateModelAsync(model));
        }

        try
        {
            await _scheduleService.CreateScheduleAsync(model);
            TempData["SuccessMessage"] = "Đã sắp xếp lịch cho giáo viên thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await _scheduleService.BuildCreateModelAsync(model));
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _scheduleService.BuildEditModelAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminTeacherScheduleFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(await _scheduleService.BuildCreateModelAsync(model));
        }

        try
        {
            var updated = await _scheduleService.UpdateScheduleAsync(model);
            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật lịch giáo viên thành công.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(await _scheduleService.BuildCreateModelAsync(model));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _scheduleService.DeleteScheduleAsync(id);
        TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted
            ? "Xóa lịch giáo viên thành công."
            : "Không tìm thấy lịch cần xóa.";

        return RedirectToAction(nameof(Index));
    }
}
