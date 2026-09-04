using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN")]
public class ChatMonitorController : Controller
{
    private readonly IAdminChatMonitorService _chatMonitorService;
    private readonly ApplicationDbContext _context;

    public ChatMonitorController(IAdminChatMonitorService chatMonitorService, ApplicationDbContext context)
    {
        _chatMonitorService = chatMonitorService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _chatMonitorService.GetChatStatsAsync());
    }

    public async Task<IActionResult> TeacherChats(string? search, int? teacherId, int? classroomId, string? status, DateTime? from, DateTime? to)
    {
        ViewBag.Search = search;
        ViewBag.TeacherId = teacherId;
        ViewBag.ClassroomId = classroomId;
        ViewBag.Status = status;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.Teachers = await _context.Users.AsNoTracking().Include(user => user.Role)
            .Where(user => user.Role.RoleCode == "TEACHER" && user.Status == "ACTIVE")
            .OrderBy(user => user.FullName).Select(user => new { user.Id, user.FullName }).ToListAsync();
        ViewBag.Classrooms = await _context.Classrooms.AsNoTracking().Where(classroom => classroom.Status != "CLOSED")
            .OrderBy(classroom => classroom.ClassName).Select(classroom => new { classroom.Id, classroom.ClassName }).ToListAsync();
        return View(await _chatMonitorService.GetTeacherStudentChatsAsync(search, teacherId, classroomId, status, from, to));
    }

    [HttpGet]
    public async Task<IActionResult> History(int teacherId, int studentId, string? search)
    {
        return Json(await _chatMonitorService.GetConversationAsync(teacherId, studentId, search));
    }

    public async Task<IActionResult> AiTutorSessions()
    {
        return View(await _chatMonitorService.GetAiTutorSessionsAsync());
    }
}
