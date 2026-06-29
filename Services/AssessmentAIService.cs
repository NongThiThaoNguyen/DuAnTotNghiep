using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DuAnTotNghiep.Services
{
    public class AssessmentAIService : IAssessmentAIService
    {
        private readonly DuAnTotNghiep.Services.Interfaces.IPromptTemplateService _tmpl;
        private readonly IHttpClientFactory _http;
        private readonly AiProviderSettings _cfg;
        private readonly ILogger<AssessmentAIService> _log;
        private readonly IResponseValidator _val;
        private readonly IAiLoggingService _aiLog;

        public AssessmentAIService(
            DuAnTotNghiep.Services.Interfaces.IPromptTemplateService tmpl,
            IHttpClientFactory http,
            IOptions<AiProviderSettings> cfg,
            ILogger<AssessmentAIService> log,
            IResponseValidator val,
            IAiLoggingService aiLog)
        {
            _tmpl = tmpl;
            _http = http;
            _cfg = cfg.Value;
            _log = log;
            _val = val;
            _aiLog = aiLog;
        }

        public async Task<AssessmentAiResponse> AnalyzeAsync(AssessmentAiRequest req)
        {
            // 1. Get active prompt template
            var (sys, usr, schema) = await _tmpl.GetActivePromptAsync("ASSESSMENT");
            var prompt = BuildPrompt(usr, req);

            var payload = new
            {
                model = _cfg.Model,
                messages = new[]
                {
                    new { role = "system", content = sys },
                    new { role = "user", content = prompt }
                }
            };

            // Phase 1: Pre-flight Logging
            // We assume User ID is not directly available here, so we pass null or parse it if added to req.
            long logId = await _aiLog.LogRequestAsync(null, "ASSESSMENT", null, _cfg.Model, prompt);

            string json = null;
            Exception err = null;
            var tries = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // 2. Call AI with retry up to 2 times
            while (tries <= 2 && json == null)
            {
                tries++;
                try
                {
                    var client = _http.CreateClient("AiProvider");
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var resp = await client.PostAsJsonAsync(string.Empty, payload);

                    if (!resp.IsSuccessStatusCode)
                    {
                        var errTxt = await resp.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"HTTP {resp.StatusCode}: {errTxt}");
                    }

                    var txt = await resp.Content.ReadAsStringAsync();

                    using var doc = JsonDocument.Parse(txt);
                    json = doc.RootElement.GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "AI assessment call attempt {Try} failed.", tries);
                    err = ex;
                }
            }

            sw.Stop();
            var latencyMs = (int)sw.ElapsedMilliseconds;

            // Phase 2: Post-flight Logging
            if (json != null)
            {
                // In a real scenario, you'd extract input/output token counts from the response JSON (e.g., usage.prompt_tokens)
                // For now, we put 0, 0, 0m as placeholders.
                await _aiLog.LogResponseAsync(logId, json, 0, 0, 0m, latencyMs);
            }
            else
            {
                // Extract clean message for the log to isolate the error
                var cleanMsg = err?.Message;
                if (err is HttpRequestException httpErr) cleanMsg = httpErr.Message;
                await _aiLog.LogErrorAsync(logId, cleanMsg ?? "Unknown AI Error", latencyMs);
                throw new InvalidOperationException("AI Call failed", err);
            }

            // 4. Validate output
            var vr = await _val.ValidateAsync(json, schema);
            if (!vr.IsValid)
                throw new InvalidOperationException(string.Join("; ", vr.Errors));

            // 5. Deserialize
            var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<AssessmentAiResponse>(json, opt)!;
        }

        private string BuildPrompt(string tmpl, AssessmentAiRequest req)
        {
            var result = tmpl;
            foreach (var p in typeof(AssessmentAiRequest).GetProperties())
            {
                var placeholder = $"{{{p.Name}}}";
                var val = p.GetValue(req)?.ToString() ?? "";
                result = result.Replace(placeholder, val, StringComparison.Ordinal);
            }
            return result;
        }
    }
}
