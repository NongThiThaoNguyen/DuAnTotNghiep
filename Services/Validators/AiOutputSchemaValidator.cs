using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DuAnTotNghiep.Services.Validators
{
    public class AiOutputSchemaValidator
    {
        /// <summary>
        /// Validates that the AI output JSON contains required shape: question_text (string), options (array of strings), correct_answer_index (int).
        /// Returns (true, null) when valid; otherwise (false, errorMessage).
        /// This is a lightweight validator to avoid adding JSON Schema dependencies.
        /// </summary>
        public (bool IsValid, string? ErrorMessage) Validate(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return (false, "Empty JSON output.");
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return (false, "Root JSON must be an object.");

                if (!root.TryGetProperty("question_text", out var q) || q.ValueKind != JsonValueKind.String)
                    return (false, "Missing or invalid 'question_text' (string).");

                if (!root.TryGetProperty("options", out var opts) || opts.ValueKind != JsonValueKind.Array)
                    return (false, "Missing or invalid 'options' (array of strings).");

                var options = new List<string>();
                foreach (var it in opts.EnumerateArray())
                {
                    if (it.ValueKind != JsonValueKind.String) return (false, "All 'options' must be strings.");
                    options.Add(it.GetString() ?? string.Empty);
                }
                if (options.Count == 0) return (false, "'options' must contain at least one element.");

                if (!root.TryGetProperty("correct_answer_index", out var idx) || idx.ValueKind != JsonValueKind.Number)
                    return (false, "Missing or invalid 'correct_answer_index' (integer).");

                if (!idx.TryGetInt32(out var correctIndex)) return (false, "'correct_answer_index' is not a valid integer.");
                if (correctIndex < 0 || correctIndex >= options.Count) return (false, "'correct_answer_index' is out of bounds of 'options'.");

                // optional checks
                if (root.TryGetProperty("difficulty", out var diff) && diff.ValueKind != JsonValueKind.String)
                    return (false, "If present, 'difficulty' must be a string.");

                if (root.TryGetProperty("skill_tags", out var tags) && tags.ValueKind != JsonValueKind.Array)
                    return (false, "If present, 'skill_tags' must be an array of strings.");

                if (root.TryGetProperty("skill_tags", out var stags))
                {
                    foreach (var t in stags.EnumerateArray())
                        if (t.ValueKind != JsonValueKind.String) return (false, "All 'skill_tags' must be strings.");
                }

                return (true, null);
            }
            catch (JsonException ex)
            {
                return (false, "Invalid JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, "Validation error: " + ex.Message);
            }
        }
    }
}
