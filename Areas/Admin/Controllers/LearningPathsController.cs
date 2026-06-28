using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class LearningPathsController : Controller
    {
        private readonly IPathViewService _pathViewService;

        public LearningPathsController(IPathViewService pathViewService)
        {
            _pathViewService = pathViewService;
        }

        [HttpGet]
        public async Task<IActionResult> Preview(int studentId)
        {
            var model = await _pathViewService.GetCurrentPathPageAsync(studentId);
            ViewData["ReadOnly"] = true;
            return View(model);
        }
    }
}
