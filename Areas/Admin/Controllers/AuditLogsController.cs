using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Areas.Admin.ViewModels;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogManagementService _auditLogService;

        public AuditLogsController(IAuditLogManagementService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> Index(string? logAction, string? user, DateTime? from, DateTime? to, int page = 1)
        {
            int pageSize = 20;

            var logs = await _auditLogService.GetLogsAsync(logAction, user, from, to, page, pageSize);
            var totalCount = await _auditLogService.GetTotalCountAsync(logAction, user, from, to);
            var actions = await _auditLogService.GetDistinctActionsAsync();

            var viewModel = new AuditLogListViewModel
            {
                Logs = logs,
                AvailableActions = actions,
                ActionFilter = logAction,
                UserFilter = user,
                FromDate = from,
                ToDate = to,
                CurrentPage = page,
                TotalRecords = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(long id)
        {
            var log = await _auditLogService.GetByIdAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }
    }
}
