using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Validators;
using DuAnTotNghiep.ViewModels;

namespace DuAnTotNghiep.Services.AI
{
    public class AIQuizGenerationService
    {
        private readonly IAIProvider _aiProvider;
        private readonly IPromptTemplateService _promptService;
        private readonly AiOutputSchemaValidator _validator;
        private readonly ApplicationDbContext _db;

        public AIQuizGenerationService(IAIProvider aiProvider, IPromptTemplateService promptService, AiOutputSchemaValidator validator, ApplicationDbContext db)
        {
            _aiProvider = aiProvider;
            _promptService = promptService;
            _validator = validator;
            _db = db;
        }

        public async Task<AIGenerationResult> GenerateQuestionsAsync(GenerateQuestionRequestDto dto)
        {
            try
            {
                var prompt = await _promptService.GetActivePromptByModuleAsync("M14_QUIZ_GENERATION");
                if (prompt == null) return new AIGenerationResult { IsSuccess = false, ErrorMessage = "No active prompt template for module M14_QUIZ_GENERATION." };

                var systemPrompt = prompt.SystemPrompt ?? string.Empty;
                var userPrompt = BuildUserPrompt(dto);

                string aiResponse;
                try
                {
                    aiResponse = await _aiProvider.GenerateAsync(systemPrompt, userPrompt, "M14", prompt.Id, dto.RequestedBy != null ? int.Parse(dto.RequestedBy) : null, "gpt-4o-mini");
                }
                catch (TaskCanceledException ex)
                {
                    return new AIGenerationResult { IsSuccess = false, ErrorMessage = "We could not complete your request because the AI service timed out. Please try again." };
                }
                catch (Exception ex)
                {
                    return new AIGenerationResult { IsSuccess = false, ErrorMessage = ex.Message };
                }

                // Response might be a JSON string or wrapped; try to parse the first JSON object/array
                string trimmed = aiResponse.Trim();
                string jsonPayload = trimmed;
                // If AI wrapped content in markdown or text, try to extract JSON block
                int firstBrace = trimmed.IndexOf('{');
                int firstBracket = trimmed.IndexOf('[');
                if (firstBrace >= 0 && (firstBracket < 0 || firstBrace < firstBracket)) jsonPayload = trimmed.Substring(firstBrace);
                else if (firstBracket >= 0) jsonPayload = trimmed.Substring(firstBracket);

                List<GeneratedQuestionPreviewViewModel> items = new List<GeneratedQuestionPreviewViewModel>();
                List<string> itemErrors = new List<string>();

                try
                {
                    using var doc = JsonDocument.Parse(jsonPayload);
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        int i = 0;
                        foreach (var el in doc.RootElement.EnumerateArray())
                        {
                            i++;
                            var itemResult = ParseQuestionElement(el);
                            if (itemResult.IsValid)
                            {
                                items.Add(itemResult.Item!);
                            }
                            else
                            {
                                itemErrors.Add($"Item {i}: {itemResult.Error}");
                            }
                        }
                    }
                    else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        var r = ParseQuestionElement(doc.RootElement);
                        if (r.IsValid) items.Add(r.Item!); else itemErrors.Add("Item 1: " + r.Error);
                    }
                    else
                    {
                        return new AIGenerationResult { IsSuccess = false, ErrorMessage = "The AI response did not contain a valid JSON payload." };
                    }
                }
                catch (JsonException ex)
                {
                    return new AIGenerationResult { IsSuccess = false, ErrorMessage = "The AI response could not be parsed. Please review the generated content." };
                }

                return new AIGenerationResult { IsSuccess = items.Count > 0, Items = items, ItemErrors = itemErrors };
            }
            catch (Exception ex)
            {
                return new AIGenerationResult { IsSuccess = false, ErrorMessage = "We could not generate the questions right now. Please try again shortly." };
            }
        }

        private (bool IsValid, GeneratedQuestionPreviewViewModel? Item, string? Error) ParseQuestionElement(JsonElement el)
        {
            try
            {
                // Convert to object and validate via validator
                var json = el.GetRawText();
                var (isValid, error) = _validator.Validate(json);
                if (!isValid) return (false, null, error);

                var model = new GeneratedQuestionPreviewViewModel();
                if (el.TryGetProperty("question_text", out var q)) model.QuestionText = q.GetString();
                if (el.TryGetProperty("options", out var opts) && opts.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var o in opts.EnumerateArray()) list.Add(o.GetString() ?? string.Empty);
                    model.Options = list;
                }
                if (el.TryGetProperty("correct_answer_index", out var idx) && idx.TryGetInt32(out var ii)) model.CorrectAnswerIndex = ii;
                if (el.TryGetProperty("explanation", out var ex)) model.Explanation = ex.GetString();
                if (el.TryGetProperty("difficulty", out var df)) model.Difficulty = df.GetString();
                if (el.TryGetProperty("skill_tags", out var tags) && tags.ValueKind == JsonValueKind.Array)
                {
                    var tlist = new List<string>();
                    foreach (var t in tags.EnumerateArray()) tlist.Add(t.GetString() ?? string.Empty);
                    model.Tags = tlist;
                }

                return (true, model, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        private string BuildUserPrompt(GenerateQuestionRequestDto dto)
        {
            return $"Generate {dto.QuestionCount} {dto.QuestionType} question(s) for skill id {dto.SkillId}, topic id {dto.TopicId}, proficiency level {dto.ProficiencyLevelId}, difficulty {dto.Difficulty}. Notes: {dto.Notes}";
        }
    }
}
