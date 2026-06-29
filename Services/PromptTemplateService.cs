using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class PromptTemplateService :
        IPromptTemplateService,
        DuAnTotNghiep.Services.Interfaces.IPromptTemplateService
    {
        private readonly ApplicationDbContext _db;

        public PromptTemplateService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AiPromptTemplate?> GetActivePromptByModuleAsync(
            string moduleCode,
            CancellationToken cancellationToken = default)
        {
            return await _db.AiPromptTemplates
                .Where(p => p.ModuleCode == moduleCode && p.Status == "ACTIVE")
                .OrderByDescending(p => p.VersionNo)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(string SystemPrompt, string UserPromptTemplate, string OutputSchema)> GetActivePromptAsync(
            string moduleCode)
        {
            var template = await _db.AiPromptTemplates
                .Where(x => x.ModuleCode == moduleCode && x.Status == "ACTIVE")
                .OrderByDescending(x => x.VersionNo)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                throw new System.Exception($"No active prompt template found for module: {moduleCode}");
            }

            return (
                template.SystemPrompt,
                template.UserPromptTemplate ?? string.Empty,
                template.OutputSchema ?? string.Empty);
        }
    }
}
