using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DuAnTotNghiep.Services;

/// <summary>
/// Lightweight HTTP client for Google Gemini generateContent REST API.
/// </summary>
public class GeminiHttpClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<GeminiHttpClient> _logger;

    // Base URL template – {model} and {key} substituted at call time.
    private const string EndpointTemplate =
        "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent?key={1}";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

    public GeminiHttpClient(
        HttpClient http,
        IConfiguration configuration,
        ILogger<GeminiHttpClient> logger)
    {
        _http = http;
        _apiKey = configuration["GoogleAI:ApiKey"] ?? string.Empty;
        _model = configuration["GoogleAI:Model"] ?? "gemini-2.0-flash";
        _logger = logger;
    }

    /// <summary>
    /// Sends a chat-style request (optional system prompt + user message) to Gemini
    /// and returns the raw text response.
    /// Returns null if Gemini is not configured or if the request fails.
    /// </summary>
    public async Task<string?> GenerateAsync(
        string userMessage,
        string? systemInstruction = null,
        CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Gemini API key not configured – skipping real AI call.");
            return null;
        }

        var url = string.Format(EndpointTemplate, _model, _apiKey);

        var requestBody = BuildRequestBody(userMessage, systemInstruction);

        try
        {
            var response = await _http.PostAsync(
                url,
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("Gemini API error {Status}: {Body}", (int)response.StatusCode, errorBody);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(
                cancellationToken: ct);

            var text = result?.Candidates?.FirstOrDefault()
                ?.Content?.Parts?.FirstOrDefault()
                ?.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Gemini returned empty text.");
                return null;
            }

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception calling Gemini API.");
            return null;
        }
    }

    // ── Internal request/response models ──────────────────────────────────

    private static object BuildRequestBody(string userMessage, string? systemInstruction)
    {
        var contents = new[]
        {
            new { role = "user", parts = new[] { new { text = userMessage } } }
        };

        if (!string.IsNullOrWhiteSpace(systemInstruction))
        {
            return new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents,
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 4096,
                    responseMimeType = "application/json"
                }
            };
        }

        return new
        {
            contents,
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 4096,
                responseMimeType = "application/json"
            }
        };
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
