using System;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class ExportController : Controller
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> ExportUsers()
        {
            try
            {
                var content = await _exportService.ExportUsersToExcelAsync();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Users_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xuất dữ liệu: " + ex.Message;
                return RedirectToAction("Index", "User");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportPlacementResults()
        {
            try
            {
                var content = await _exportService.ExportPlacementResultsToExcelAsync();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"PlacementResults_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xuất dữ liệu: " + ex.Message;
                return RedirectToAction("Index", "PlacementAttempts");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportAuditLogs(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var content = await _exportService.ExportAuditLogsToExcelAsync(fromDate, toDate);
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AuditLogs_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xuất dữ liệu: " + ex.Message;
                return RedirectToAction("Index", "AuditLogs");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportAiUsageLogs()
        {
            try
            {
                var content = await _exportService.ExportAiUsageLogsToExcelAsync();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AiUsageLogs_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xuất dữ liệu: " + ex.Message;
                return RedirectToAction("Index", "AiUsageLogs");
            }
        }
    }
}
