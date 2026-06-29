using System;
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AiUsageLogsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AiUsageLogsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? module, string? model, DateTime? fromDate, DateTime? toDate, string? status)
        {
            var query = _db.AiUsageLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(module))
            {
                query = query.Where(x => x.ModuleCode != null && x.ModuleCode.Contains(module));
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(x => x.AiModel != null && x.AiModel.Contains(model));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt < toDate.Value.Date.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.RequestStatus == status);
            }

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.ModuleCode,
                    x.PromptTemplateId,
                    x.AiModel,
                    x.InputTokens,
                    x.OutputTokens,
                    x.CostEstimate,
                    x.RequestStatus,
                    x.ErrorMessage,
                    x.DurationMs,
                    x.CreatedAt
                })
                .ToListAsync();

            ViewBag.Module = module;
            ViewBag.Model = model;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Status = status;
            ViewBag.TotalTokens = items.Sum(x => (long?)(x.InputTokens ?? 0) + (long?)(x.OutputTokens ?? 0) ?? 0);
            ViewBag.TotalCost = items.Sum(x => x.CostEstimate ?? 0m);

            return View(items);
        }
    }
}
