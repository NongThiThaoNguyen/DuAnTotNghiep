using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class ResponseValidator : IResponseValidator
    {
        public Task<ValidationResult> ValidateAsync(string json, string schema)
        {
            var res = new ValidationResult { IsValid = true };
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Check required fields (case-insensitive for student ease)
                string[] fields = { "summary", "strengths", "weaknesses", "gaps", "currentLevel", "recommendedLevel", "confidenceScore", "priorityTopics", "recommendedActions" };

                foreach (var f in fields)
                {
                    bool hasLower = root.TryGetProperty(f, out _);
                    bool hasUpper = root.TryGetProperty(f.Substring(0, 1).ToUpper() + f.Substring(1), out _);

                    if (!hasLower && !hasUpper)
                    {
                        res.IsValid = false;
                        res.Errors.Add($"Missing required field: {f}");
                    }
                }
            }
            catch (Exception ex)
            {
                res.IsValid = false;
                res.Errors.Add($"JSON format error: {ex.Message}");
            }
            return Task.FromResult(res);
        }
    }
}
