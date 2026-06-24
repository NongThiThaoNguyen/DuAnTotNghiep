using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services.Background
{
    public class AiAnalysisBackgroundService : BackgroundService
    {
        private readonly IAiAnalysisQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AiAnalysisBackgroundService> _logger;

        public AiAnalysisBackgroundService(
            IAiAnalysisQueue queue,
            IServiceProvider serviceProvider,
            ILogger<AiAnalysisBackgroundService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Analysis Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (attemptId, studentId) = await _queue.DequeueAsync(stoppingToken);
                    await ProcessAttemptAnalysisAsync(attemptId, studentId, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Prevent throwing if stoppingToken was signaled
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing AI analysis.");
                }
            }
        }

        private async Task ProcessAttemptAnalysisAsync(int attemptId, int studentId, CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var payloadBuilder = scope.ServiceProvider.GetRequiredService<IPlacementTestAnalysisPayloadBuilder>();
            var aiService = scope.ServiceProvider.GetRequiredService<ICompetencyAnalysisService>();

            string moduleCode = $"M6_ATTEMPT_{attemptId}";

            try
            {
                // Find or create pending log
                var aiLog = await dbContext.AiUsageLogs
                    .FirstOrDefaultAsync(l => l.UserId == studentId && l.ModuleCode == moduleCode);

                if (aiLog != null)
                {
                    aiLog.RequestStatus = "RUNNING";
                    _logger.LogInformation($"Attempt {attemptId} - AI Status updated to RUNNING");
                }
                else
                {
                    aiLog = new AiUsageLog
                    {
                        UserId = studentId,
                        ModuleCode = moduleCode,
                        RequestStatus = "RUNNING",
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.AiUsageLogs.Add(aiLog);
                }

                await dbContext.SaveChangesAsync(stoppingToken);

                // Build Payload
                var payload = await payloadBuilder.BuildPayloadAsync(attemptId, studentId);
                if (payload == null)
                {
                    throw new InvalidOperationException("Could not build payload (missing profile or attempt).");
                }

                // Trigger AI Analysis
                await aiService.AnalyzePlacementTestAsync(payload);

                // Update Status
                aiLog.RequestStatus = "COMPLETED";
                await dbContext.SaveChangesAsync(stoppingToken);
                
                _logger.LogInformation($"Attempt {attemptId} - AI Analysis COMPLETED successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process AI analysis for Attempt {attemptId}");
                
                var errorLog = await dbContext.AiUsageLogs
                    .FirstOrDefaultAsync(l => l.UserId == studentId && l.ModuleCode == moduleCode);
                
                if (errorLog != null)
                {
                    errorLog.RequestStatus = "FAILED";
                    errorLog.ErrorMessage = ex.Message;
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}
