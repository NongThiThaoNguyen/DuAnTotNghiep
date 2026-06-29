using System;
using System.Threading.Tasks;
using DuAnTotNghiep.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.Extensions.Logging;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services
{
    public class CompetencyAnalysisService : ICompetencyAnalysisService
    {
        private readonly ILogger<CompetencyAnalysisService> _logger;
        private readonly ApplicationDbContext _dbContext;

        public CompetencyAnalysisService(ILogger<CompetencyAnalysisService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task AnalyzePlacementTestAsync(PlacementTestAnalysisPayload payload)
        {
            _logger.LogInformation($"[Stub AI] Starting Competency Analysis for Attempt {payload.AttemptId}, Student {payload.StudentId}");
            
            // Simulating AI Delay
            await Task.Delay(2000);

            // Here we would normally call the actual AI API, generate prompt from payload, etc.
            // For now, we stub a CompetencyAnalysis result

            var analysis = new CompetencyAnalysis
            {
                StudentId = payload.StudentId,
                TestAttemptId = payload.AttemptId,
                Summary = "Student demonstrates strong vocabulary but struggles with complex grammar.",
                Strengths = "Vocabulary, Travel topics",
                Weaknesses = "Grammar, Business topics",
                GapAnalysis = "Needs to focus more on business English grammar structures.",
                AiModel = "stub-model",
                ConfidenceScore = 0.85m,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.CompetencyAnalyses.Add(analysis);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"[Stub AI] Completed Analysis for Attempt {payload.AttemptId}");
        }
    }
}
