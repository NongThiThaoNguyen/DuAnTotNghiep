using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Services.PromptTemplates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DuAnTotNghiep.Services;

/// <summary>
/// Production implementation of <see cref="ILearningPathAiService"/> that calls
/// Google Gemini to generate personalized learning paths.
/// Falls back to <see cref="LearningPathAiService"/> (mock) when Gemini is unavailable.
/// </summary>
public class GeminiLearningPathAiService : ILearningPathAiService
{
    private readonly GeminiHttpClient _gemini;
    private readonly LearningPathAiService _mockFallback;
    private readonly ILogger<GeminiLearningPathAiService> _logger;
    private readonly int _totalWeeks;

    public GeminiLearningPathAiService(
        GeminiHttpClient gemini,
        IConfiguration configuration,
        ILogger<GeminiLearningPathAiService> logger)
    {
        _gemini = gemini;
        _logger = logger;
        _mockFallback = new LearningPathAiService();
        _totalWeeks = int.TryParse(configuration["GoogleAI:TotalWeeks"], out var w) ? w : 12;
    }

    /// <inheritdoc />
    public async Task<LearningPathOutputDto> GeneratePathFromAiAsync(LearningPathInputDto input)
    {
        if (!_gemini.IsConfigured)
        {
            _logger.LogInformation("Gemini not configured – using mock fallback for learning path.");
            return await _mockFallback.GeneratePathFromAiAsync(input);
        }

        try
        {
            var userPrompt = LearningPathPrompt.Build(input, _totalWeeks);
            var raw = await _gemini.GenerateAsync(userPrompt, LearningPathPrompt.SystemPrompt);

            if (string.IsNullOrWhiteSpace(raw))
            {
                _logger.LogWarning("Gemini returned empty response – using mock fallback.");
                return await _mockFallback.GeneratePathFromAiAsync(input);
            }

            var output = ParseGeminiResponse(raw, input);
            _logger.LogInformation(
                "Gemini generated path '{Title}' with {PhaseCount} phases for student {StudentId}.",
                output.PathTitle, output.Phases.Count, input.StudentId);
            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating learning path via Gemini – using mock fallback.");
            return await _mockFallback.GeneratePathFromAiAsync(input);
        }
    }

    /// <inheritdoc />
    public Task<(bool IsValid, string[] Errors)> ValidateAiOutputAsync(
        LearningPathOutputDto output,
        LearningPathInputDto input)
    {
        // Delegate validation to the mock service (same rules apply to both).
        return _mockFallback.ValidateAiOutputAsync(output, input);
    }

    // ── JSON Parsing ──────────────────────────────────────────────────────

    private LearningPathOutputDto ParseGeminiResponse(string json, LearningPathInputDto input)
    {
        // Strip possible markdown code fences if present.
        var cleaned = StripMarkdownFences(json);

        var root = JsonNode.Parse(cleaned)
            ?? throw new InvalidOperationException("Gemini returned invalid JSON.");

        var pathTitle = root["learning_path_title"]?.GetValue<string>()
            ?? $"{input.GoalName} Learning Path";
        var overview = root["overview"]?.GetValue<string>()
            ?? $"Personalized path from {input.CurrentLevelName} to {input.TargetLevelName}.";
        var totalWeeks = root["total_phases"]?.GetValue<int>() * 4 ?? _totalWeeks;

        var phasesNode = root["phases"]?.AsArray() ?? new JsonArray();
        var phases = new List<LearningPathOutputPhaseDto>();

        foreach (var phaseNode in phasesNode)
        {
            if (phaseNode is null) continue;

            var phaseName = phaseNode["phase_title"]?.GetValue<string>()
                ?? phaseNode["phase_id"]?.ToString() ?? "Phase";
            var weeksText = phaseNode["duration"]?.GetValue<string>() ?? string.Empty;
            var weeksCount = ParseWeeksFromDuration(weeksText);

            var modulesNode = phaseNode["modules"]?.AsArray() ?? new JsonArray();
            var nodes = new List<LearningPathOutputNodeDto>();

            foreach (var mod in modulesNode)
            {
                if (mod is null) continue;
                var node = ParseModule(mod, input, phaseName);
                if (node is not null) nodes.Add(node);
            }

            // Only add phases that have valid nodes.
            if (nodes.Count > 0)
                phases.Add(new LearningPathOutputPhaseDto
                {
                    PhaseName = phaseName,
                    Weeks = weeksCount,
                    Nodes = nodes
                });
        }

        if (phases.Count == 0)
            throw new BusinessException("AI không tạo được giai đoạn nào hợp lệ trong lộ trình.");

        return new LearningPathOutputDto
        {
            PathTitle = pathTitle,
            Summary = overview,
            TotalWeeks = totalWeeks,
            Phases = phases
        };
    }

    private LearningPathOutputNodeDto? ParseModule(JsonNode mod, LearningPathInputDto input, string phaseName)
    {
        var moduleId = mod["module_id"]?.GetValue<string>() ?? string.Empty;
        var title = mod["title"]?.GetValue<string>() ?? moduleId;
        var typeStr = mod["type"]?.GetValue<string>() ?? string.Empty;
        var resourceId = mod["resource_id"]?.GetValue<int?>();
        var estimatedHours = mod["estimated_hours"]?.GetValue<double>() ?? 1.0;
        var aiReason = mod["ai_reason"]?.GetValue<string>() ?? phaseName;

        // Map Gemini type → NodeType constant.
        string actionType = typeStr.ToUpperInvariant() switch
        {
            "QUIZ" => NodeType.Quiz,
            "LESSON" => NodeType.Lesson,
            "TOPIC" => NodeType.Topic,
            _ => moduleId.StartsWith("QUIZ", StringComparison.OrdinalIgnoreCase)
                ? NodeType.Quiz
                : NodeType.Topic
        };

        // Validate resource_id exists in the available catalog.
        if (resourceId.HasValue)
        {
            var isValid = actionType switch
            {
                NodeType.Topic => input.AvailableTopics.Any(r => r.Id == resourceId.Value),
                NodeType.Lesson => input.AvailableLessons.Any(r => r.Id == resourceId.Value),
                NodeType.Quiz => input.AvailableQuizzes.Any(r => r.Id == resourceId.Value),
                _ => false
            };

            if (!isValid)
            {
                _logger.LogWarning(
                    "Gemini referenced resource_id={Id} (type={Type}) not in catalog – skipping node '{Title}'.",
                    resourceId, actionType, title);
                return null;
            }
        }
        else
        {
            // No resource_id provided – pick first available of that type as a best-effort.
            resourceId = actionType switch
            {
                NodeType.Topic => input.AvailableTopics.FirstOrDefault()?.Id,
                NodeType.Lesson => input.AvailableLessons.FirstOrDefault()?.Id,
                NodeType.Quiz => input.AvailableQuizzes.FirstOrDefault()?.Id,
                _ => null
            };

            if (resourceId is null)
            {
                _logger.LogWarning("No catalog resource available for type {Type} – skipping node '{Title}'.", actionType, title);
                return null;
            }
        }

        return new LearningPathOutputNodeDto
        {
            NodeTitle = title,
            NodeDescription = $"{actionType} activity – {phaseName}",
            ActionType = actionType,
            TopicId = actionType == NodeType.Topic ? resourceId : null,
            LessonId = actionType == NodeType.Lesson ? resourceId : null,
            QuizId = actionType == NodeType.Quiz ? resourceId : null,
            EstimatedMinutes = (int)Math.Max(15, estimatedHours * 60),
            AiReason = aiReason,
            PathPhase = phaseName
        };
    }

    private static int ParseWeeksFromDuration(string duration)
    {
        // "Tuần 1 - Tuần 4" → 4 weeks
        var parts = duration.Split('-');
        if (parts.Length == 2)
        {
            var nums = parts.Select(p => System.Text.RegularExpressions.Regex.Match(p, @"\d+"))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Value))
                .ToList();
            if (nums.Count == 2) return nums[1] - nums[0] + 1;
        }
        return 4; // default 4 weeks per phase
    }

    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[7..];
        else if (trimmed.StartsWith("```"))
            trimmed = trimmed[3..];

        if (trimmed.EndsWith("```"))
            trimmed = trimmed[..^3];

        return trimmed.Trim();
    }
}
