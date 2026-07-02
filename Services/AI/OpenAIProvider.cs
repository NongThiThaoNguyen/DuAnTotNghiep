using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs;
using DuAnTotNghiep.Services;
using Microsoft.Extensions.Configuration;

namespace DuAnTotNghiep.Services.AI
{
    public class OpenAIProvider : IAIProvider
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly AiUsageLogService _usageLogService;
        private readonly string _apiKey;

        public OpenAIProvider(HttpClient http, IConfiguration config, ApplicationDbContext db, AiUsageLogService usageLogService)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _config = config;
            _db = db;
            _usageLogService = usageLogService;
            _apiKey = config["AI:ApiKey"] ?? config["OpenAI:ApiKey"] ?? "";
            
            string baseAddress = "https://api.openai.com/v1/";
            if (!string.IsNullOrEmpty(config["AI:Endpoint"]))
            {
                baseAddress = config["AI:Endpoint"];
                if (!baseAddress.EndsWith("/")) baseAddress += "/";
            }
            else if (_apiKey.StartsWith("AIzaSy"))
            {
                baseAddress = "https://generativelanguage.googleapis.com/v1beta/openai/";
            }

            _http.BaseAddress = new Uri(baseAddress);
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }
        }

        public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, string moduleCode = "M14", int? promptTemplateId = null, int? userId = null, string? aiModel = null, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestStatus = "SUCCESS";
            string? errorMessage = null;
            int? inputTokens = null;
            int? outputTokens = null;
            
            var model = aiModel ?? "gpt-4o-mini";
            
            bool isGemini = _apiKey.StartsWith("AIzaSy") || 
                            (!string.IsNullOrEmpty(_config["AI:Endpoint"]) && _config["AI:Endpoint"].Contains("generativelanguage"));
            
            if (isGemini || !string.IsNullOrEmpty(_config["AI:Model"]))
            {
                if (!string.IsNullOrEmpty(_config["AI:Model"]))
                {
                    model = _config["AI:Model"];
                }
                else if (model.Contains("gpt-4o-mini") || model.Contains("gpt-3.5"))
                {
                    model = "gemini-2.5-flash";
                }
                else if (model.Contains("gpt-4"))
                {
                    model = "gemini-2.5-pro";
                }
            }

            try
            {
                var request = new
                {
                    model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    max_tokens = 1500
                };

                var json = JsonSerializer.Serialize(request);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var resp = await _http.PostAsync("chat/completions", content, cancellationToken);
                resp.EnsureSuccessStatusCode();
                var respText = await resp.Content.ReadAsStringAsync(cancellationToken);

                try
                {
                    using var doc = JsonDocument.Parse(respText);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                    {
                        var first = choices[0];
                        if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentEl))
                        {
                            var aiContentText = contentEl.GetString() ?? string.Empty;
                            if (root.TryGetProperty("usage", out var usage))
                            {
                                inputTokens = usage.TryGetProperty("prompt_tokens", out var promptTokens) ? promptTokens.GetInt32() : null;
                                outputTokens = usage.TryGetProperty("completion_tokens", out var completionTokens) ? completionTokens.GetInt32() : null;
                            }
                            await LogUsageAsync(new AiUsageLogDto
                            {
                                UserId = userId,
                                ModuleCode = moduleCode,
                                PromptTemplateId = promptTemplateId,
                                AiModel = model,
                                InputTokens = inputTokens,
                                OutputTokens = outputTokens,
                                CostEstimate = CalculateCost(inputTokens, outputTokens),
                                RequestStatus = requestStatus,
                                ErrorMessage = errorMessage,
                                DurationMs = (int)stopwatch.ElapsedMilliseconds
                            });
                            return aiContentText;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    requestStatus = "SCHEMA_ERROR";
                    errorMessage = "The AI response format was invalid.";
                    throw new InvalidOperationException("The AI response format was invalid.", ex);
                }

                return respText;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("timed", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                requestStatus = "TIMEOUT";
                errorMessage = "The AI service did not respond in time. Please try again.";
                await LogUsageAsync(new AiUsageLogDto { UserId = userId, ModuleCode = moduleCode, PromptTemplateId = promptTemplateId, AiModel = model, InputTokens = inputTokens, OutputTokens = outputTokens, CostEstimate = CalculateCost(inputTokens, outputTokens), RequestStatus = requestStatus, ErrorMessage = errorMessage, DurationMs = (int)stopwatch.ElapsedMilliseconds });
                throw;
            }
            catch (JsonException)
            {
                requestStatus = "SCHEMA_ERROR";
                errorMessage = "The AI response format was invalid.";
                await LogUsageAsync(new AiUsageLogDto { UserId = userId, ModuleCode = moduleCode, PromptTemplateId = promptTemplateId, AiModel = model, InputTokens = inputTokens, OutputTokens = outputTokens, CostEstimate = CalculateCost(inputTokens, outputTokens), RequestStatus = requestStatus, ErrorMessage = errorMessage, DurationMs = (int)stopwatch.ElapsedMilliseconds });
                throw;
            }
            catch (Exception)
            {
                requestStatus = "FAILED";
                errorMessage = "We could not complete the AI request right now. Please try again shortly.";
                await LogUsageAsync(new AiUsageLogDto { UserId = userId, ModuleCode = moduleCode, PromptTemplateId = promptTemplateId, AiModel = model, InputTokens = inputTokens, OutputTokens = outputTokens, CostEstimate = CalculateCost(inputTokens, outputTokens), RequestStatus = requestStatus, ErrorMessage = errorMessage, DurationMs = (int)stopwatch.ElapsedMilliseconds });
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private decimal? CalculateCost(int? inputTokens, int? outputTokens)
        {
            var unitPrice = _config.GetValue<decimal>("AI:UnitPrice", 0m);
            var inputCost = (inputTokens ?? 0) * unitPrice;
            var outputCost = (outputTokens ?? 0) * unitPrice;
            return inputCost + outputCost;
        }

        private async Task LogUsageAsync(AiUsageLogDto dto)
        {
            await _usageLogService.LogRequestAsync(dto);
        }
    }
}
