using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class ChatMonitorController : Controller
{
    private readonly IAdminChatMonitorService _chatMonitorService;

    public ChatMonitorController(IAdminChatMonitorService chatMonitorService)
    {
        _chatMonitorService = chatMonitorService;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _chatMonitorService.GetChatStatsAsync());
    }

    public async Task<IActionResult> TeacherChats()
    {
        return View(await _chatMonitorService.GetTeacherStudentChatsAsync());
    }

    public async Task<IActionResult> AiTutorSessions()
    {
        return View(await _chatMonitorService.GetAiTutorSessionsAsync());
    }
}
