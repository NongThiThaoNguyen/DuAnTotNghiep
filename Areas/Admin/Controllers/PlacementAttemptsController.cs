using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class PlacementAttemptsController : Controller
    {
        private readonly IPlacementAttemptService _attemptService;

        public PlacementAttemptsController(IPlacementAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        public async Task<IActionResult> Index([FromQuery] PlacementAttemptFilter filter)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int currentUserId);

            // Get role
            string role = User.IsInRole("ADMIN") ? "ADMIN" : "TEACHER";

            var result = await _attemptService.GetAttemptsForAdminAsync(filter, currentUserId, role);
            
            ViewBag.Filter = filter;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out int currentUserId);

            string role = User.IsInRole("ADMIN") ? "ADMIN" : "TEACHER";

            var attempt = await _attemptService.GetAttemptDetailAsync(id, currentUserId, role);
            if (attempt == null)
            {
                return NotFound("Attempt not found or you do not have permission to view it.");
            }

            return View(attempt);
        }
    }
}
