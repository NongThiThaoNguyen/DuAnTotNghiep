using System;
using System.Security.Claims;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Controllers
{
    [Authorize(Roles = "Student")]
    public class CompetencyAnalysisController : Controller
    {
        private readonly ICompetencyAnalysisOrchestrator _orchestrator;
        private readonly ApplicationDbContext _context;

        public CompetencyAnalysisController(ICompetencyAnalysisOrchestrator orchestrator, ApplicationDbContext context)
        {
            _orchestrator = orchestrator;
            _context = context;
        }

        public async Task<IActionResult> Latest()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var viewModel = await _orchestrator.GetLatestAnalysisAsync(userId);
            if (viewModel == null)
            {
                TempData["InfoMessage"] = "Bạn cần hoàn thành bài kiểm tra đầu vào để hệ thống phân tích năng lực.";
                return RedirectToAction("Index", "PlacementTest");
            }

            return RedirectToAction("Result", new { id = viewModel.AnalysisId });
        }

        public async Task<IActionResult> Result(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role) ?? "Student";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                var viewModel = await _orchestrator.GetAnalysisByIdAsync(id, userId, role, ipAddress);
                if (viewModel == null)
                {
                    return NotFound();
                }

                if (viewModel.Status == "PENDING")
                {
                    return View("PendingView", viewModel);
                }

                // Query database to check if Student already has an active Learning Path
                var hasLearningPath = await _context.StudentLearningPaths
                    .AnyAsync(lp => lp.StudentId == viewModel.StudentId || (role == "Student" && lp.StudentId == userId));
                
                // Check if this analysis is the latest one
                var latestAnalysisId = await _context.CompetencyAnalyses
                    .Where(a => a.StudentId == viewModel.StudentId && a.Status == "COMPLETED")
                    .OrderByDescending(a => a.CreatedAt)
                    .Select(a => a.Id)
                    .FirstOrDefaultAsync();

                ViewBag.HasLearningPath = hasLearningPath;
                ViewBag.IsLatest = (latestAnalysisId == id);

                return View(viewModel);
            }
            catch (UnauthorizedAccessException)
            {
                // IDOR Protection: Hide resource existence by returning 404 instead of 403 for Students
                if (role == "Student")
                {
                    return NotFound();
                }
                return Forbid();
            }
            catch (DataScopeViolationException)
            {
                // Data Scope Isolation: Return 404 to avoid revealing student information to unauthorized teachers
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

            bool success = await _orchestrator.TriggerRegenerationAsync(id, userId);
            if (success)
            {
                TempData["SuccessMessage"] = "Hệ thống đang tiến hành phân tích lại dựa trên mục tiêu mới của bạn.";
            }
            else
            {
                TempData["ErrorMessage"] = "Yêu cầu không hợp lệ hoặc bạn đã yêu cầu tạo lại quá nhiều lần. Vui lòng thử lại sau.";
            }

            return RedirectToAction("Result", new { id });
        }

        [HttpGet]
        [Authorize(Roles = "Student,Teacher,Admin")]
        public async Task<IActionResult> GetIntegrationData(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }

            var role = User.FindFirstValue(ClaimTypes.Role) ?? "Student";

            try
            {
                var integrationDto = await _orchestrator.GetLearningPathInputAsync(id, userId, role);
                if (integrationDto == null)
                {
                    return NotFound(new { error = "ANALYSIS_NOT_FOUND", message = "Competency analysis not found." });
                }

                return Json(integrationDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Obfuscation: Return NotFound to hide resource presence for unauthorized students
                if (role == "Student")
                {
                    return NotFound(new { error = "ANALYSIS_NOT_FOUND", message = "Competency analysis not found." });
                }
                return Forbid();
            }
            catch (DataScopeViolationException)
            {
                // Obfuscation: Return NotFound to hide resource presence for unauthorized teachers
                return NotFound(new { error = "ANALYSIS_NOT_FOUND", message = "Competency analysis not found." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "INVALID_ANALYSIS_STATE", message = ex.Message });
            }
        }
    }
}
