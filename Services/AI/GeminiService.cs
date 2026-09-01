using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services.AI
{
    public class GeminiService : IGeminiService, IAIProvider
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly IConfiguration _config;
        private readonly ILogger<GeminiService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public GeminiService(
            HttpClient httpClient,
            IOptions<GeminiSettings> settings,
            IConfiguration config,
            ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _config = config;
            _logger = logger;
        }

        private string GetApiKey()
        {
            var candidateKeys = new[]
            {
                _settings.ApiKey,
                _config["Gemini:ApiKey"],
                _config["GoogleAI:ApiKey"],
                _config["AI:ApiKey"],
                Environment.GetEnvironmentVariable("Gemini__ApiKey"),
                Environment.GetEnvironmentVariable("GoogleAI__ApiKey")
            };

            var validKey = candidateKeys.FirstOrDefault(k => !string.IsNullOrWhiteSpace(k));
            return validKey?.Trim() ?? string.Empty;
        }

        private string GetModel(string? modelOverride = null)
        {
            if (!string.IsNullOrWhiteSpace(modelOverride)) return modelOverride.Trim();
            
            var candidateModels = new[]
            {
                _settings.Model,
                _config["Gemini:Model"],
                _config["GoogleAI:Model"],
                _config["AI:Model"]
            };

            var validModel = candidateModels.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
            return !string.IsNullOrWhiteSpace(validModel) ? validModel.Trim() : "gemini-3.6-flash";
        }

        private string GetBaseUrl()
        {
            var candidateUrls = new[]
            {
                _settings.BaseUrl,
                _config["Gemini:BaseUrl"],
                _config["GoogleAI:BaseUrl"]
            };

            var validUrl = candidateUrls.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
            return (!string.IsNullOrWhiteSpace(validUrl) ? validUrl : "https://generativelanguage.googleapis.com").TrimEnd('/');
        }

        public async Task<string> GenerateAsync(
            string systemPrompt,
            string userPrompt,
            string moduleCode = "M14",
            int? promptTemplateId = null,
            int? userId = null,
            string? aiModel = null,
            CancellationToken cancellationToken = default)
        {
            return await ChatAsync(userPrompt, systemPrompt, cancellationToken);
        }

        public async Task<string> ChatAsync(string userPrompt, string? systemPrompt = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                throw new ArgumentException("User prompt không được để trống.", nameof(userPrompt));
            }

            var result = await SendGenerateContentAsync(userPrompt, systemPrompt, null, cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException($"Lỗi kết nối Gemini API [HTTP {result.HttpStatusCode}]: {result.ErrorMessage}");
            }

            return result.Content ?? string.Empty;
        }

        public async Task<GeminiResultDto> SendGenerateContentAsync(
            string userPrompt, 
            string? systemPrompt = null, 
            string? modelOverride = null, 
            CancellationToken cancellationToken = default)
        {
            var result = new GeminiResultDto();
            var apiKey = GetApiKey();
            var model = GetModel(modelOverride);
            var baseUrl = GetBaseUrl();
            result.Model = model;

            // 1. Validate API Key
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("[Diagnostic] Gemini API Key: MISSING");
                var errorMsg = "Gemini API Key chưa được cấu hình. Vui lòng thiết lập Gemini:ApiKey bằng User Secrets, .env hoặc appsettings.json.";
                _logger.LogError("Gemini API request cancelled: Missing API Key.");
                result.IsSuccess = false;
                result.ErrorMessage = errorMsg;
                result.HttpStatusCode = 401;
                return result;
            }

            _logger.LogInformation("[Diagnostic] Gemini API Key: FOUND (Length: {Length})", apiKey.Length);

            try
            {
                // Build request payload for Gemini generateContent API
                object requestPayload;
                var contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = userPrompt } } }
                };

                if (!string.IsNullOrWhiteSpace(systemPrompt))
                {
                    requestPayload = new
                    {
                        system_instruction = new
                        {
                            parts = new[] { new { text = systemPrompt } }
                        },
                        contents,
                        generationConfig = new
                        {
                            temperature = 0.7,
                            maxOutputTokens = 4096
                        }
                    };
                }
                else
                {
                    requestPayload = new
                    {
                        contents,
                        generationConfig = new
                        {
                            temperature = 0.7,
                            maxOutputTokens = 4096
                        }
                    };
                }

                // Always use v1beta endpoint (supports all newer Gemini models)
                var requestUrl = $"{baseUrl}/v1beta/models/{model}:generateContent?key={apiKey}";
                var jsonPayload = JsonSerializer.Serialize(requestPayload, _jsonOptions);

                // Safe logging: sanitize API key in log message
                _logger.LogInformation("Gửi request tới Gemini API | Model: {Model} | Target: {BaseUrl}/v1beta/models/{Model}:generateContent?key=[REDACTED]", 
                    model, baseUrl, model);

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                httpRequest.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                result.HttpStatusCode = (int)response.StatusCode;

                var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                // 2. Handle HTTP Errors (Non-2xx)
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini API trả về mã lỗi HTTP {StatusCode}: {ResponseString}", response.StatusCode, responseString);
                    result.IsSuccess = false;

                    try
                    {
                        using var doc = JsonDocument.Parse(responseString);
                        if (doc.RootElement.TryGetProperty("error", out var errorEl))
                        {
                            var message = errorEl.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;
                            var status = errorEl.TryGetProperty("status", out var stEl) ? stEl.GetString() : null;
                            var code = errorEl.TryGetProperty("code", out var cdEl) ? cdEl.GetInt32().ToString() : response.StatusCode.ToString();

                            result.ErrorMessage = $"[{status ?? code}]: {message ?? responseString}";
                        }
                        else
                        {
                            result.ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseString}";
                        }
                    }
                    catch
                    {
                        result.ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseString}";
                    }

                    return result;
                }

                // 3. Deserialize & Parse Response
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Phản hồi từ Gemini API rỗng (empty content).";
                    return result;
                }

                using var responseDoc = JsonDocument.Parse(responseString);
                var root = responseDoc.RootElement;

                // Extract text candidate
                string? generatedText = null;
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var textEl))
                        {
                            generatedText = textEl.GetString();
                        }
                    }
                }

                // Extract usage metadata if available
                if (root.TryGetProperty("usageMetadata", out var usage))
                {
                    result.PromptTokens = usage.TryGetProperty("promptTokenCount", out var pt) ? pt.GetInt32() : 0;
                    result.CompletionTokens = usage.TryGetProperty("candidatesTokenCount", out var ct) ? ct.GetInt32() : 0;
                    result.TotalTokens = usage.TryGetProperty("totalTokenCount", out var tt) ? tt.GetInt32() : 0;
                }

                if (string.IsNullOrWhiteSpace(generatedText))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Gemini API không trả về nội dung văn bản (Empty text in candidate).";
                    return result;
                }

                result.IsSuccess = true;
                result.Content = generatedText;
                _logger.LogInformation("Nhận phản hồi từ Gemini API thành công | Tokens: Prompt={PromptTokens}, Completion={CompletionTokens}, Total={TotalTokens}",
                    result.PromptTokens, result.CompletionTokens, result.TotalTokens);

                return result;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                var timeoutSeconds = _settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds : 30;
                var timeoutMsg = $"Kết nối đến Gemini API bị hết thời gian chờ (Timeout sau {timeoutSeconds}s). Detail: {ex.Message}";
                _logger.LogError(ex, "Timeout khi kết nối đến Gemini API.");
                result.IsSuccess = false;
                result.ErrorMessage = timeoutMsg;
                result.HttpStatusCode = 408;
                return result;
            }
            catch (JsonException ex)
            {
                var parseErrorMsg = $"Lỗi định dạng phản hồi JSON từ Gemini API: {ex.Message}";
                _logger.LogError(ex, "Lỗi giải mã JSON từ Gemini API.");
                result.IsSuccess = false;
                result.ErrorMessage = parseErrorMsg;
                result.HttpStatusCode = 500;
                return result;
            }
            catch (Exception ex)
            {
                var generalErrorMsg = $"Lỗi hệ thống khi gọi Gemini API: {ex.Message}";
                _logger.LogError(ex, "Lỗi ngoại lệ không xác định khi gọi Gemini API.");
                result.IsSuccess = false;
                result.ErrorMessage = generalErrorMsg;
                result.HttpStatusCode = 500;
                return result;
            }
        }
    }
}
