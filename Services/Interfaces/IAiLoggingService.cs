using System;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAiLoggingService
    {
        /// <summary>
        /// Phase 1: Logs the initial API request to the database.
        /// Returns the ID of the created log record to be used in Phase 2.
        /// </summary>
        Task<long> LogRequestAsync(int? userId, string moduleCode, int? promptTemplateId, string aiModel, string promptInput);

        /// <summary>
        /// Phase 2: Updates the existing log record with the response, tokens, and latency.
        /// </summary>
        Task LogResponseAsync(long logId, string responseOutput, int inputTokens, int outputTokens, decimal costEstimate, int latencyMs);

        /// <summary>
        /// Phase 2 (Error): Updates the existing log record when an exception or API error occurs.
        /// </summary>
        Task LogErrorAsync(long logId, string errorMessage, int latencyMs);
    }
}
