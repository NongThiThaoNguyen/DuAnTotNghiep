using System.Threading;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.AI
{
    public interface IAIProvider
    {
        Task<string> GenerateAsync(string systemPrompt, string userPrompt, string moduleCode = "M14", int? promptTemplateId = null, int? userId = null, string? aiModel = null, CancellationToken cancellationToken = default);
    }
}
