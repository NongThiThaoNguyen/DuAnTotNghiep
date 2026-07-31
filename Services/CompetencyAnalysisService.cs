using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.DTOs.PlacementTest;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Services.PromptTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services
{
    public class CompetencyAnalysisService : ICompetencyAnalysisService
    {
        private readonly ILogger<CompetencyAnalysisService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly GeminiHttpClient _gemini;

        public CompetencyAnalysisService(
            ILogger<CompetencyAnalysisService> logger,
            ApplicationDbContext dbContext,
            GeminiHttpClient gemini)
        {
            _logger = logger;
            _dbContext = dbContext;
            _gemini = gemini;
        }

        public async Task AnalyzePlacementTestAsync(PlacementTestAnalysisPayload payload)
        {
            _logger.LogInformation(
                "[AI] Starting Competency Analysis for Attempt {AttemptId}, Student {StudentId}",
                payload.AttemptId, payload.StudentId);

            // ── 1. Mark previous analyses as not latest ───────────────────
            var previousAnalyses = await _dbContext.CompetencyAnalyses
                .Where(a => a.StudentId == payload.StudentId && a.IsLatest)
                .ToListAsync();
            foreach (var prev in previousAnalyses)
                prev.IsLatest = false;

            // ── 2. Try Gemini; fallback to stub on failure ─────────────────
            CompetencyAnalysisResult result;
            if (_gemini.IsConfigured)
            {
                result = await AnalyzeWithGeminiAsync(payload)
                    ?? BuildStubResult(payload);
            }
            else
            {
                _logger.LogInformation("[AI] Gemini not configured – using stub analysis.");
                result = BuildStubResult(payload);
            }

            // ── 3. Persist CompetencyAnalysis ─────────────────────────────
            var analysis = new CompetencyAnalysis
            {
                StudentId        = payload.StudentId,
                TestAttemptId    = payload.AttemptId,
                Summary          = result.Summary,
                Strengths        = result.Strengths,
                Weaknesses       = result.Weaknesses,
                GapAnalysis      = result.GapAnalysis,
                AiModel          = _gemini.IsConfigured ? "gemini-2.0-flash" : "stub-model",
                ConfidenceScore  = result.ConfidenceScore,
                Status           = "COMPLETED",
                IsLatest         = true,
                PrioritizedTopicsJson = result.PrioritizedTopics.Any()
                    ? JsonSerializer.Serialize(result.PrioritizedTopics) : null,
                KnowledgeGapsJson = result.KnowledgeGaps.Any()
                    ? JsonSerializer.Serialize(result.KnowledgeGaps) : null,
                CreatedAt        = DateTime.UtcNow
            };

            _dbContext.CompetencyAnalyses.Add(analysis);
            await _dbContext.SaveChangesAsync();

            // ── 4. Persist CompetencySkillScores ──────────────────────────
            var scores = await BuildSkillScoresAsync(result.SkillScores, analysis.Id, payload);
            if (scores.Any())
            {
                _dbContext.CompetencySkillScores.AddRange(scores);
                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation(
                "[AI] Competency Analysis COMPLETED for Attempt {AttemptId} (analysisId={Id}).",
                payload.AttemptId, analysis.Id);
        }

        // ── Gemini call ────────────────────────────────────────────────────

        private async Task<CompetencyAnalysisResult?> AnalyzeWithGeminiAsync(
            PlacementTestAnalysisPayload payload)
        {
            try
            {
                var userPrompt = CompetencyAnalysisPrompt.Build(payload);
                var raw = await _gemini.GenerateAsync(userPrompt, CompetencyAnalysisPrompt.SystemPrompt);
                if (string.IsNullOrWhiteSpace(raw)) return null;

                return ParseGeminiResponse(raw, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini call failed for Attempt {AttemptId} – using stub.", payload.AttemptId);
                return null;
            }
        }

        private CompetencyAnalysisResult? ParseGeminiResponse(
            string json, PlacementTestAnalysisPayload payload)
        {
            try
            {
                // Strip markdown fences if present.
                var cleaned = json.Trim();
                if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase)) cleaned = cleaned[7..];
                else if (cleaned.StartsWith("```")) cleaned = cleaned[3..];
                if (cleaned.EndsWith("```")) cleaned = cleaned[..^3];
                cleaned = cleaned.Trim();

                var root = JsonNode.Parse(cleaned);
                if (root is null) return null;

                var skillNodes = root["skill_scores"]?.AsArray() ?? new JsonArray();
                var aiSkills = skillNodes
                    .Where(n => n != null)
                    .Select(n => new AiSkillScore
                    {
                        SkillName      = n!["skill_name"]?.GetValue<string>() ?? string.Empty,
                        Score          = (decimal)(n["score"]?.GetValue<double>() ?? 0.5),
                        PriorityLevel  = n["priority_level"]?.GetValue<int>() ?? 1,
                        WeaknessNote   = n["weakness_note"]?.GetValue<string>()
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s.SkillName))
                    .ToList();

                return new CompetencyAnalysisResult
                {
                    Summary           = root["summary"]?.GetValue<string>() ?? BuildFallbackSummary(payload),
                    Strengths         = root["strengths"]?.GetValue<string>() ?? string.Empty,
                    Weaknesses        = root["weaknesses"]?.GetValue<string>() ?? string.Empty,
                    GapAnalysis       = root["gap_analysis"]?.GetValue<string>() ?? string.Empty,
                    ConfidenceScore   = (decimal)(root["confidence_score"]?.GetValue<double>() ?? 0.75),
                    SkillScores       = aiSkills,
                    PrioritizedTopics = root["prioritized_topics"]?.AsArray()
                        .Select(n => n?.GetValue<string>() ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList() ?? new(),
                    KnowledgeGaps     = root["knowledge_gaps"]?.AsArray()
                        .Select(n => n?.GetValue<string>() ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList() ?? new()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Gemini competency analysis response.");
                return null;
            }
        }

        // ── Skill score persistence ────────────────────────────────────────

        private async Task<List<CompetencySkillScore>> BuildSkillScoresAsync(
            List<AiSkillScore> aiScores,
            int analysisId,
            PlacementTestAnalysisPayload payload)
        {
            // Load all skills from DB to map by name.
            var allSkills = await _dbContext.EnglishSkills.AsNoTracking().ToListAsync();
            var skillMap = allSkills.ToDictionary(
                s => s.SkillName.Trim().ToLowerInvariant(),
                s => s.Id);

            var result = new List<CompetencySkillScore>();

            // If Gemini returned skill_scores, use those.
            if (aiScores.Any())
            {
                foreach (var ai in aiScores)
                {
                    if (!skillMap.TryGetValue(ai.SkillName.Trim().ToLowerInvariant(), out var skillId))
                    {
                        _logger.LogWarning("Unknown skill name from AI: '{Name}' – skipping.", ai.SkillName);
                        continue;
                    }

                    result.Add(new CompetencySkillScore
                    {
                        CompetencyAnalysisId = analysisId,
                        SkillId              = skillId,
                        Score                = ai.Score,
                        PriorityLevel        = ai.PriorityLevel,
                        WeaknessNote         = ai.WeaknessNote
                    });
                }
            }
            else
            {
                // Fallback: derive from payload.SkillScores.
                foreach (var ps in payload.SkillScores)
                {
                    result.Add(new CompetencySkillScore
                    {
                        CompetencyAnalysisId = analysisId,
                        SkillId              = ps.SkillId,
                        Score                = ps.Percentage / 100m,
                        PriorityLevel        = ps.Percentage < 50 ? 3 : (ps.Percentage < 75 ? 2 : 1),
                        WeaknessNote         = ps.Percentage < 50
                            ? $"Cần cải thiện {ps.SkillName}" : null
                    });
                }
            }

            return result;
        }

        // ── Stub fallback ──────────────────────────────────────────────────

        private static CompetencyAnalysisResult BuildStubResult(PlacementTestAnalysisPayload payload)
        {
            var weak = payload.SkillScores
                .Where(s => s.Percentage < 60)
                .Select(s => s.SkillName)
                .ToList();
            var strong = payload.SkillScores
                .Where(s => s.Percentage >= 70)
                .Select(s => s.SkillName)
                .ToList();

            return new CompetencyAnalysisResult
            {
                Summary         = BuildFallbackSummary(payload),
                Strengths       = strong.Any() ? string.Join(", ", strong) : "Chưa xác định",
                Weaknesses      = weak.Any() ? string.Join(", ", weak) : "Chưa xác định",
                GapAnalysis     = $"Cần cải thiện từ {payload.CurrentLevel} lên {payload.TargetLevel}.",
                ConfidenceScore = 0.75m,
                SkillScores     = payload.SkillScores
                    .Select(s => new AiSkillScore
                    {
                        SkillName     = s.SkillName,
                        Score         = s.Percentage / 100m,
                        PriorityLevel = s.Percentage < 50 ? 3 : (s.Percentage < 75 ? 2 : 1),
                        WeaknessNote  = s.Percentage < 50 ? $"Cần cải thiện {s.SkillName}" : null
                    }).ToList(),
                PrioritizedTopics = new(),
                KnowledgeGaps     = new()
            };
        }

        private static string BuildFallbackSummary(PlacementTestAnalysisPayload payload)
            => $"Học viên đạt {payload.TotalScore:F1}/100 điểm. " +
               $"Trình độ ước tính: {payload.EstimatedLevel}. " +
               $"Mục tiêu: {payload.LearningGoal}.";

        // ── Internal DTOs ──────────────────────────────────────────────────

        private sealed class CompetencyAnalysisResult
        {
            public string Summary { get; set; } = string.Empty;
            public string Strengths { get; set; } = string.Empty;
            public string Weaknesses { get; set; } = string.Empty;
            public string GapAnalysis { get; set; } = string.Empty;
            public decimal ConfidenceScore { get; set; }
            public List<AiSkillScore> SkillScores { get; set; } = new();
            public List<string> PrioritizedTopics { get; set; } = new();
            public List<string> KnowledgeGaps { get; set; } = new();
        }

        private sealed class AiSkillScore
        {
            public string SkillName { get; set; } = string.Empty;
            public decimal Score { get; set; }
            public int PriorityLevel { get; set; }
            public string? WeaknessNote { get; set; }
        }
    }
}
