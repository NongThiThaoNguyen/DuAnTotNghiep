using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SchemaStatusController : Controller
    {
        private readonly IM4SchemaService _schemaService;

        public SchemaStatusController(IM4SchemaService schemaService)
        {
            _schemaService = schemaService;
        }

        public async Task<IActionResult> Index()
        {
            var status = await _schemaService.GetSchemaHealthStatusAsync();
            return View(status);
        }
    }
}
