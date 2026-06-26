using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.DTOs.TopicImport;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Controllers.Admin
{
    [Area("Admin")]
    [Route("Admin/TopicImport")]
    [Authorize(Roles = "Admin")]
    public class TopicImportController : Controller
    {
        private readonly ITopicImportService _importService;

        public TopicImportController(ITopicImportService importService)
        {
            _importService = importService;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        // Preview the import file without persisting data
        [HttpPost("Preview")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid file.");
                return View("Import");
            }

            // Read file into memory
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            var preview = await _importService.PreviewImportAsync(fileBytes, file.FileName);
            return View("Import", preview);
        }

        // Execute the import and seed data
        [HttpPost("Execute")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Execute(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid file.");
                return View("Import");
            }
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var fileBytes = ms.ToArray();
            await _importService.ImportTopicsAsync(fileBytes, file.FileName);
            ViewBag.Message = "Import completed successfully.";
            return View("ImportResult");
        }
    }
}
