using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class PromptTemplateService : IPromptTemplateService
    {
        private readonly ApplicationDbContext _db;

        public PromptTemplateService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<(string SystemPrompt, string UserPromptTemplate, string OutputSchema)> GetActivePromptAsync(string moduleCode)
        {
            var t = await _db.AiPromptTemplates
                .Where(x => x.ModuleCode == moduleCode && x.Status == "ACTIVE")
                .OrderByDescending(x => x.VersionNo)
                .FirstOrDefaultAsync();

            if (t == null)
                throw new System.Exception($"No active prompt template found for module: {moduleCode}");

            return (t.SystemPrompt, t.UserPromptTemplate ?? "", t.OutputSchema ?? "");
        }
    }
}
