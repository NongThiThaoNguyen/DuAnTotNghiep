using System;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services
{
    public class AiLoggingService : IAiLoggingService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AiLoggingService> _logger;

        public AiLoggingService(IServiceScopeFactory scopeFactory, ILogger<AiLoggingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<long> LogRequestAsync(int? userId, string moduleCode, int? promptTemplateId, string aiModel, string promptInput)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var log = new AiUsageLog
                {
                    UserId = userId,
                    ModuleCode = moduleCode,
                    PromptTemplateId = promptTemplateId,
                    AiModel = aiModel,
                    PromptInput = promptInput,
                    RequestStatus = "Sending",
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.AiUsageLogs.Add(log);
                await dbContext.SaveChangesAsync();

                return log.Id;
            }
            catch (Exception ex)
            {
                // Error Isolation: Swallow exception so it doesn't break the main business flow
                _logger.LogError(ex, "Failed to log AI request (Phase 1).");
                return 0; // Return 0 to indicate failed logging, handled gracefully in Phase 2
            }
        }

        public async Task LogResponseAsync(long logId, string responseOutput, int inputTokens, int outputTokens, decimal costEstimate, int latencyMs)
        {
            if (logId <= 0) return;

            // Fire-and-forget in background to not block the main thread
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var log = await dbContext.AiUsageLogs.FindAsync(logId);
                    if (log != null)
                    {
                        log.ResponseOutput = responseOutput;
                        log.InputTokens = inputTokens;
                        log.OutputTokens = outputTokens;
                        log.CostEstimate = costEstimate;
                        log.LatencyMs = latencyMs;
                        log.RequestStatus = "Success";

                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to log AI response (Phase 2) for LogId {logId}.");
                }
            });
        }

        public async Task LogErrorAsync(long logId, string errorMessage, int latencyMs)
        {
            if (logId <= 0) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var log = await dbContext.AiUsageLogs.FindAsync(logId);
                    if (log != null)
                    {
                        log.ErrorMessage = errorMessage;
                        log.LatencyMs = latencyMs;
                        log.RequestStatus = "Failed";

                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to log AI error (Phase 2) for LogId {logId}.");
                }
            });
        }
    }
}
