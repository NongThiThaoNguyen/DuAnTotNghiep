using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "STUDENT")]
    public class StudyPlanController : Controller
    {
        private readonly IStudyPlanService _studyPlanService;
        private readonly IPathViewService _pathViewService;
        private readonly ApplicationDbContext _context;

        public StudyPlanController(
            IStudyPlanService studyPlanService,
            IPathViewService pathViewService,
            ApplicationDbContext context)
        {
            _studyPlanService = studyPlanService;
            _pathViewService = pathViewService;
            _context = context;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int userId);
            return userId;
        }

        // GET /Student/StudyPlan
        // Redirect to Today by default
        public IActionResult Index()
        {
            return RedirectToAction("Today");
        }

        // GET /Student/StudyPlan/Today
        public async Task<IActionResult> Today()
        {
            int userId = GetUserId();
            var vm = await _studyPlanService.GetTodayPlanAsync(userId);
            if (!vm.HasActivePath)
            {
                TempData["Info"] = "Vui lòng hoàn thành Onboarding để tạo lộ trình học tập.";
                return RedirectToAction("Index", "LearningPath", new { area = "Student" });
            }
            return View(vm);
        }

        // GET /Student/StudyPlan/Week?weekStart=2026-06-29
        public async Task<IActionResult> Week(DateTime? weekStart)
        {
            int userId = GetUserId();
            DateOnly? ws = weekStart.HasValue ? DateOnly.FromDateTime(weekStart.Value) : null;
            var vm = await _studyPlanService.GetWeeklyPlanAsync(userId, ws);
            if (!vm.HasActivePath)
            {
                return RedirectToAction("Index", "LearningPath", new { area = "Student" });
            }
            return View(vm);
        }

        // GET /Student/StudyPlan/Month?year=2026&month=7
        public async Task<IActionResult> Month(int? year, int? month)
        {
            int userId = GetUserId();
            var vm = await _studyPlanService.GetMonthlyPlanAsync(userId, year, month);
            if (!vm.HasActivePath)
            {
                return RedirectToAction("Index", "LearningPath", new { area = "Student" });
            }
            return View(vm);
        }

        // GET /Student/StudyPlan/OpenTask/{nodeId}
        public async Task<IActionResult> OpenTask(int nodeId)
        {
            int userId = GetUserId();
            if (!await _pathViewService.CanOpenNodeAsync(nodeId, userId))
            {
                TempData["Error"] = "Không thể mở nhiệm vụ này.";
                return RedirectToAction("Today");
            }
            
            var node = await _context.LearningPathNodes.FindAsync(nodeId);
            if (node == null)
            {
                TempData["Error"] = "Không tìm thấy nhiệm vụ.";
                return RedirectToAction("Today");
            }

            // Update status to InProgress if it is currently Available
            if (node.Status == "AVAILABLE")
            {
                node.Status = "IN_PROGRESS";
                _context.LearningPathNodes.Update(node);
                await _context.SaveChangesAsync();
            }

            var url = await _pathViewService.BuildNodeTargetUrlAsync(node);
            if (string.IsNullOrEmpty(url))
            {
                TempData["Error"] = "Nhiệm vụ chưa có liên kết học tập.";
                return RedirectToAction("Today");
            }

            return Redirect(url);
        }

        // POST /Student/StudyPlan/SkipTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SkipTask(int nodeId, string? reason)
        {
            int userId = GetUserId();
            var success = await _studyPlanService.MarkTaskSkippedAsync(nodeId, userId, reason);
            if (!success)
            {
                TempData["Error"] = "Không thể bỏ qua nhiệm vụ này.";
            }
            else
            {
                TempData["Success"] = "Đã bỏ qua nhiệm vụ học tập.";
            }
            return RedirectToAction("Today");
        }
    }
}
