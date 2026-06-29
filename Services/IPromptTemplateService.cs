using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services
{
    public interface IPromptTemplateService
    {
        /// <summary>
        /// Returns the active prompt template for the given module code, or null if none ACTIVE.
        /// Implementations MUST NOT call AI if this returns null.
        /// </summary>
        Task<AiPromptTemplate?> GetActivePromptByModuleAsync(string moduleCode, CancellationToken cancellationToken = default);
    }
}
