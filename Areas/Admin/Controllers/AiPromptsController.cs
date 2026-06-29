using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AiPromptsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AiPromptsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _db.AiPromptTemplates
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new {
                    p.Id, p.PromptCode, p.PromptName, p.ModuleCode, p.VersionNo, p.Status, p.CreatedAt
                })
                .ToListAsync();

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.AiPromptTemplates.FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}
