using System.Security.Claims;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class LearningPathController : Controller
    {
        private readonly IPathViewService _pathViewService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LearningPathController> _logger;

        public LearningPathController(IPathViewService pathViewService, ApplicationDbContext context)
            : this(pathViewService, context, NullLogger<LearningPathController>.Instance)
        {
        }

        [ActivatorUtilitiesConstructor]
        public LearningPathController(
            IPathViewService pathViewService,
            ApplicationDbContext context,
            ILogger<LearningPathController> logger)
        {
            _pathViewService = pathViewService;
            _context = context;
            _logger = logger;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var model = await _pathViewService.GetCurrentPathPageAsync(userId);
            return View(model);
        }

        [HttpGet("/Student/LearningPath/OpenNode/{nodeId:int}")]
        public async Task<IActionResult> OpenNode(int nodeId)
        {
            var userId = GetUserId();
            var node = await _context.LearningPathNodes
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == nodeId);

            if (node == null)
            {
                return NotFound();
            }

            if (!await _pathViewService.EnsurePathOwnerAsync(node.LearningPathId, userId))
            {
                return Forbid();
            }

            if (!await _pathViewService.CanOpenNodeAsync(nodeId, userId))
            {
                TempData["ErrorMessage"] = "Bai hoc nay hien chua duoc mo khoa.";
                return RedirectToAction(nameof(Index));
            }

            var targetUrl = await _pathViewService.BuildNodeTargetUrlAsync(node);
            if (string.IsNullOrWhiteSpace(targetUrl))
            {
                TempData["ErrorMessage"] = "Noi dung hoc tap nay chua san sang.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Student {StudentId} opened learning path node {NodeId}", userId, nodeId);
            return Redirect(targetUrl);
        }
    }
}
