using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IPromptTemplateService
    {
        Task<(string SystemPrompt, string UserPromptTemplate, string OutputSchema)> GetActivePromptAsync(string moduleCode);
    }
}
