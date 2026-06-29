using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Admin.LearningPaths;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class LearningPathsController : Controller
    {
        private const int PageSize = 20;

        private readonly IPathViewService _pathViewService;
        private readonly ApplicationDbContext? _context;

        public LearningPathsController(IPathViewService pathViewService)
        {
            _pathViewService = pathViewService;
        }

        [ActivatorUtilitiesConstructor]
        public LearningPathsController(IPathViewService pathViewService, ApplicationDbContext context)
            : this(pathViewService)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int studentId)
        {
            var model = await _pathViewService.GetCurrentPathPageAsync(studentId);
            ViewData["ReadOnly"] = true;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PathHistory(int page = 1, string? status = null)
        {
            page = Math.Max(1, page);
            var query = BuildPathHistoryQuery(status);
            var totalCount = await query.CountAsync();
            var paths = await query
                .OrderByDescending(path => path.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return View(new PathHistoryViewModel
            {
                Paths = paths.Select(MapHistoryItem).ToList(),
                Page = page,
                Status = status ?? string.Empty,
                TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize))
            });
        }

        [HttpGet]
        public async Task<IActionResult> PathDetail(int id)
        {
            var path = await Context.StudentLearningPaths
                .AsNoTracking()
                .Include(item => item.Student)
                .Include(item => item.LearningPathNodes.OrderBy(node => node.OrderIndex))
                .FirstOrDefaultAsync(item => item.Id == id);
            if (path == null) return NotFound();

            var log = await GetLatestLearningPathLogAsync(path.StudentId);
            return View(MapPathDetail(path, log));
        }

        [HttpGet]
        public async Task<IActionResult> GenerationLogs()
        {
            var logs = await Context.AiUsageLogs
                .AsNoTracking()
                .Include(log => log.User)
                .Include(log => log.PromptTemplate)
                .Where(log => log.ModuleCode == "LEARNING_PATH")
                .OrderByDescending(log => log.CreatedAt)
                .ToListAsync();

            return View(new GenerationLogViewModel
            {
                Logs = logs.Select(MapGenerationLog).ToList()
            });
        }

        private IQueryable<StudentLearningPath> BuildPathHistoryQuery(string? status)
        {
            var query = Context.StudentLearningPaths
                .AsNoTracking()
                .Include(path => path.Student)
                .AsQueryable();

            return string.IsNullOrWhiteSpace(status)
                ? query
                : query.Where(path => path.Status == status);
        }

        private static PathHistoryItemViewModel MapHistoryItem(StudentLearningPath path)
        {
            return new PathHistoryItemViewModel
            {
                PathId = path.Id,
                StudentName = path.Student?.FullName ?? string.Empty,
                Title = path.Title,
                Status = path.Status,
                GeneratedByAi = path.GeneratedByAi,
                CreatedAt = path.CreatedAt
            };
        }

        private async Task<AiUsageLog?> GetLatestLearningPathLogAsync(int studentId)
        {
            return await Context.AiUsageLogs
                .AsNoTracking()
                .Include(log => log.PromptTemplate)
                .Where(log => log.UserId == studentId && log.ModuleCode == "LEARNING_PATH")
                .OrderByDescending(log => log.CreatedAt)
                .FirstOrDefaultAsync();
        }

        private static PathDetailAdminViewModel MapPathDetail(StudentLearningPath path, AiUsageLog? log)
        {
            return new PathDetailAdminViewModel
            {
                PathId = path.Id,
                StudentName = path.Student?.FullName ?? string.Empty,
                Title = path.Title,
                Status = path.Status,
                AiPlanSummary = path.AiPlanSummary ?? string.Empty,
                CreatedAt = path.CreatedAt,
                Nodes = path.LearningPathNodes.OrderBy(node => node.OrderIndex).Select(MapDetailNode).ToList(),
                AiModel = log?.AiModel ?? string.Empty,
                PromptVersion = log?.PromptTemplate == null ? string.Empty : $"v{log.PromptTemplate.VersionNo}",
                ErrorMessage = log?.ErrorMessage ?? string.Empty
            };
        }

        private static PathDetailNodeAdminViewModel MapDetailNode(LearningPathNode node)
        {
            return new PathDetailNodeAdminViewModel
            {
                NodeId = node.Id,
                OrderIndex = node.OrderIndex,
                Title = node.NodeTitle,
                NodeType = node.NodeType,
                Status = node.Status,
                EstimatedMinutes = node.EstimatedMinutes,
                AiReason = node.AiReason ?? string.Empty
            };
        }

        private static GenerationLogItemViewModel MapGenerationLog(AiUsageLog log)
        {
            return new GenerationLogItemViewModel
            {
                LogId = log.Id,
                UserName = log.User?.FullName ?? string.Empty,
                AiModel = log.AiModel ?? string.Empty,
                RequestStatus = log.RequestStatus,
                ErrorMessage = log.ErrorMessage ?? string.Empty,
                CreatedAt = log.CreatedAt
            };
        }

        private ApplicationDbContext Context
        {
            get
            {
                return _context ?? throw new InvalidOperationException("ApplicationDbContext is not configured.");
            }
        }
    }
}
