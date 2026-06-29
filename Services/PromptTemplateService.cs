<<<<<<< HEAD
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
=======
using System.Linq;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
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

<<<<<<< HEAD
        public async Task<AiPromptTemplate?> GetActivePromptByModuleAsync(string moduleCode, CancellationToken cancellationToken = default)
        {
            var result = await _db.AiPromptTemplates
                .FirstOrDefaultAsync(p => p.ModuleCode == moduleCode && p.Status == "ACTIVE", cancellationToken);
            return result;
=======
        public async Task<(string SystemPrompt, string UserPromptTemplate, string OutputSchema)> GetActivePromptAsync(string moduleCode)
        {
            var t = await _db.AiPromptTemplates
                .Where(x => x.ModuleCode == moduleCode && x.Status == "ACTIVE")
                .OrderByDescending(x => x.VersionNo)
                .FirstOrDefaultAsync();

            if (t == null)
                throw new System.Exception($"No active prompt template found for module: {moduleCode}");

            return (t.SystemPrompt, t.UserPromptTemplate ?? "", t.OutputSchema ?? "");
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
        }
    }
}
