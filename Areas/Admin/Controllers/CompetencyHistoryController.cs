using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class CompetencyHistoryController : Controller
    {
        private readonly ICompetencyAnalysisOrchestrator _orchestrator;

        private readonly ApplicationDbContext _context;

        public CompetencyHistoryController(ICompetencyAnalysisOrchestrator orchestrator, ApplicationDbContext context)
        {
            _orchestrator = orchestrator;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var analyses = await _context.CompetencyAnalyses
                .Include(a => a.Student)
                .Include(a => a.CurrentLevel)
                .OrderByDescending(a => a.CreatedAt)
                .Take(100) // basic limiting for demo
                .ToListAsync();

            return View(analyses);
        }

        public async Task<IActionResult> AuditLogs()
        {
            var rawLogs = await _context.AuditLogs
                .Include(l => l.User)
                    .ThenInclude(u => u!.Role)
                .OrderByDescending(l => l.CreatedAt)
                .Take(150)
                .ToListAsync();

            var displayLogs = rawLogs.Select(log =>
            {
                var actorName = log.User?.FullName ?? "Hệ thống";
                var actorRole = log.User?.Role?.RoleName ?? "System";
                var friendlyMsg = BuildFriendlyMessage(log, actorName, actorRole);

                return new AuditLogDisplayViewModel
                {
                    Id = log.Id,
                    CreatedAt = log.CreatedAt,
                    UserId = log.UserId,
                    ActorName = actorName,
                    ActorRole = actorRole,
                    ActionType = log.Action,
                    EntityId = log.EntityId,
                    IpAddress = log.IpAddress,
                    FriendlyMessage = friendlyMsg
                };
            }).ToList();

            return View(displayLogs);
        }

        private string BuildFriendlyMessage(Models.AuditLog log, string actorName, string actorRole)
        {
            var action = log.Action.ToLower();
            try
            {
                if (action == "generate")
                {
                    return $"Hệ thống đã tạo thành công báo cáo năng lực mã số #{log.EntityId} cho Học sinh {actorName}.";
                }
                else if (action == "regenerate")
                {
                    string status = "yêu cầu";
                    if (!string.IsNullOrEmpty(log.NewValue))
                    {
                        using var doc = JsonDocument.Parse(log.NewValue);
                        if (doc.RootElement.TryGetProperty("Status", out var statusProp))
                        {
                            var statusVal = statusProp.GetString()?.ToLower();
                            if (statusVal == "started") status = "bắt đầu yêu cầu";
                            else if (statusVal == "success") status = "hoàn tất thành công";
                        }
                    }
                    return $"{actorRole} {actorName} đã {status} tạo lại báo cáo năng lực mã số #{log.EntityId}.";
                }
                else if (action == "view_by_admin")
                {
                    return $"{actorRole} {actorName} đã xem chi tiết báo cáo năng lực mã số #{log.EntityId}.";
                }
                else if (action == "ai_failed")
                {
                    string errorMsg = "Lỗi không xác định từ dịch vụ AI";
                    if (!string.IsNullOrEmpty(log.NewValue))
                    {
                        using var doc = JsonDocument.Parse(log.NewValue);
                        if (doc.RootElement.TryGetProperty("ErrorMessage", out var errProp))
                        {
                            errorMsg = errProp.GetString() ?? errorMsg;
                        }
                    }
                    return $"Tiến trình phân tích AI thất bại trên báo cáo năng lực mã số #{log.EntityId} (Lỗi: {errorMsg}).";
                }
                else if (action == "linked_to_path")
                {
                    return $"Báo cáo năng lực mã số #{log.EntityId} đã được liên kết thành công sang lộ trình học tập của Module M8.";
                }
            }
            catch
            {
                // Fallback safe parse
            }

            return $"{actorName} ({actorRole}) đã thực hiện hành động '{log.Action}' trên thực thể '{log.EntityName}' (#{log.EntityId}).";
        }

        public async Task<IActionResult> Details(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var role = (User.FindFirstValue(ClaimTypes.Role) ?? "ADMIN").ToUpperInvariant() switch
            {
                "TEACHER" => "Teacher",
                "STUDENT" => "Student",
                _ => "Admin"
            };
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                var viewModel = await _orchestrator.GetAnalysisByIdAsync(id, userId, role, ipAddress);
                if (viewModel == null)
                {
                    return NotFound();
                }

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (DataScopeViolationException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Regenerate(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            // We need to know which student this analysis belongs to.
            // In a real app, Admin doesn't trigger for themselves, they trigger for the student.
            // For now, we will fetch the analysis to find the student ID.
            var analysis = await _context.CompetencyAnalyses.FindAsync(id);
            if (analysis == null) return NotFound();

            bool success = await _orchestrator.TriggerRegenerationAsync(id, analysis.StudentId);

            if (success)
            {
                TempData["SuccessMessage"] = "Yêu cầu phân tích lại đã được gửi. Vui lòng chờ trong giây lát.";
                return RedirectToAction(nameof(Details), new { id });
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể yêu cầu phân tích lại lúc này (Có thể do vượt quá giới hạn lượt yêu cầu).";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
