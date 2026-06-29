using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models.Exceptions;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Coordinates one AI generation attempt and records its usage log.
/// </summary>
public static class LearningPathAiGenerationHelper
{
    private const string LearningPathModuleCode = "LEARNING_PATH";
    private const string MockAiModel = "mock-m8";
    private const string SuccessStatus = "SUCCESS";
    private const string FailedStatus = "FAILED";

    public static async Task<(bool IsSuccess, LearningPathOutputDto? Output, string ErrorMessage)> TryGenerateAsync(
        ApplicationDbContext context,
        ILearningPathAiService aiService,
        LearningPathInputDto input,
        int studentId)
    {
        var promptTemplateId = await GetPromptTemplateIdAsync(context);
        try
        {
            var output = await aiService.GeneratePathFromAiAsync(input);
            await ValidateAiOutputAsync(context, aiService, output, input);
            AddAiUsageLog(context, studentId, SuccessStatus, null, promptTemplateId, input, output);
            return (true, output, string.Empty);
        }
        catch (Exception ex)
        {
            AddAiUsageLog(context, studentId, FailedStatus, ex.Message, promptTemplateId, input, null);
            return (false, null, ex.Message);
        }
    }

    private static async Task<int?> GetPromptTemplateIdAsync(ApplicationDbContext context)
    {
        return await context.AiPromptTemplates
            .AsNoTracking()
            .Where(template => template.ModuleCode == LearningPathModuleCode && template.Status == "ACTIVE")
            .OrderByDescending(template => template.VersionNo)
            .ThenByDescending(template => template.Id)
            .Select(template => (int?)template.Id)
            .FirstOrDefaultAsync();
    }

    private static async Task ValidateAiOutputAsync(
        ApplicationDbContext context,
        ILearningPathAiService aiService,
        LearningPathOutputDto output,
        LearningPathInputDto input)
    {
        var validation = await aiService.ValidateAiOutputAsync(output, input);
        if (!validation.IsValid) throw new BusinessException(string.Join("; ", validation.Errors));
        await LearningPathReferenceValidator.ValidateAsync(context, output);
    }

    private static void AddAiUsageLog(
        ApplicationDbContext context,
        int studentId,
        string requestStatus,
        string? errorMessage,
        int? promptTemplateId,
        LearningPathInputDto input,
        LearningPathOutputDto? output)
    {
        context.AiUsageLogs.Add(new AiUsageLog
        {
            UserId = studentId,
            ModuleCode = LearningPathModuleCode,
            PromptTemplateId = promptTemplateId,
            AiModel = MockAiModel,
            InputTokens = EstimateInputTokens(input),
            OutputTokens = output == null ? null : EstimateOutputTokens(output),
            RequestStatus = requestStatus,
            ErrorMessage = errorMessage,
            CreatedAt = DateTime.UtcNow
        });
    }

    private static int EstimateInputTokens(LearningPathInputDto input)
    {
        var textLength = input.GoalName.Length + input.CurrentLevelName.Length + input.TargetLevelName.Length;
        textLength += input.Strengths.Length + input.Weaknesses.Length;
        textLength += input.SkillPriorities.Sum(value => value.Length);
        textLength += input.PriorityTopics.Sum(value => value.Length);
        textLength += input.AvailableTopics.Sum(value => value.Name.Length);
        textLength += input.AvailableLessons.Sum(value => value.Name.Length);
        textLength += input.AvailableQuizzes.Sum(value => value.Name.Length);
        return Math.Max(1, textLength / 4);
    }

    private static int EstimateOutputTokens(LearningPathOutputDto output)
    {
        var nodes = output.Phases.SelectMany(phase => phase.Nodes).ToList();
        var textLength = output.PathTitle.Length + output.Summary.Length;
        textLength += nodes.Sum(node => node.NodeTitle.Length + node.NodeDescription.Length + node.AiReason.Length);
        return Math.Max(1, (textLength / 4) + nodes.Count);
    }
}
