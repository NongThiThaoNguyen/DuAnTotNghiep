using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Creates fallback learning path entities when AI generation is unavailable.
/// </summary>
public static class LearningPathFallbackFactory
{
    private const string PublishedTemplateStatus = "PUBLISHED";

    public static async Task<LearningPathTemplate?> GetMatchingTemplateAsync(
        ApplicationDbContext context,
        StudentLearningProfile profile)
    {
        return await context.LearningPathTemplates
            .AsNoTracking()
            .Include(template => template.LearningPathTemplateNodes.OrderBy(node => node.OrderIndex))
            .Where(template => template.Status == PublishedTemplateStatus)
            .Where(template => !template.GoalId.HasValue || template.GoalId == profile.MainGoalId)
            .Where(template => !template.StartLevelId.HasValue || template.StartLevelId == profile.CurrentLevelId)
            .Where(template => !template.TargetLevelId.HasValue || template.TargetLevelId == profile.TargetLevelId)
            .OrderByDescending(template => template.GoalId.HasValue)
            .ThenByDescending(template => template.StartLevelId.HasValue)
            .ThenByDescending(template => template.TargetLevelId.HasValue)
            .FirstOrDefaultAsync();
    }

    public static (StudentLearningPath Path, List<LearningPathNode> Nodes) CreateTemplatePath(
        LearningPathTemplate template,
        StudentLearningProfile profile,
        int studentId,
        int competencyAnalysisId,
        string errorMessage,
        DateTime now)
    {
        var path = CreateTemplatePathEntity(template, profile, studentId, competencyAnalysisId, errorMessage, now);
        var nodes = template.LearningPathTemplateNodes
            .OrderBy(node => node.OrderIndex)
            .Select((node, index) => CreateTemplateNode(node, path, index + 1))
            .ToList();

        return (path, nodes);
    }

    public static StudentLearningPath CreateFailedPath(
        StudentLearningProfile profile,
        int studentId,
        int competencyAnalysisId,
        string errorMessage,
        DateTime now)
    {
        return new StudentLearningPath
        {
            StudentId = studentId,
            CompetencyAnalysisId = competencyAnalysisId,
            GoalId = profile.MainGoalId,
            Title = "Learning path generation failed",
            Description = "AI generation failed and no published fallback template matched.",
            AiPlanSummary = $"AI generation failed: {errorMessage}",
            Status = LearningPathStatus.Failed,
            GeneratedByAi = false,
            PathVersion = 1,
            StartDate = DateOnly.FromDateTime(now),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static StudentLearningPath CreateTemplatePathEntity(
        LearningPathTemplate template,
        StudentLearningProfile profile,
        int studentId,
        int competencyAnalysisId,
        string errorMessage,
        DateTime now)
    {
        var totalWeeks = Math.Max(1, template.DurationWeeks ?? 4);
        return new StudentLearningPath
        {
            StudentId = studentId,
            TemplateId = template.Id,
            CompetencyAnalysisId = competencyAnalysisId,
            GoalId = profile.MainGoalId,
            Title = template.TemplateName,
            Description = template.Description,
            AiPlanSummary = $"Fallback template used after AI error: {errorMessage}",
            Status = LearningPathStatus.Active,
            GeneratedByAi = false,
            PathVersion = 1,
            StartDate = DateOnly.FromDateTime(now),
            TargetEndDate = DateOnly.FromDateTime(now.AddDays(totalWeeks * 7)),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static LearningPathNode CreateTemplateNode(
        LearningPathTemplateNode templateNode,
        StudentLearningPath path,
        int orderIndex)
    {
        return new LearningPathNode
        {
            LearningPath = path,
            TopicId = templateNode.TopicId,
            NodeTitle = templateNode.NodeTitle,
            NodeType = templateNode.NodeType,
            PathPhase = "Template",
            EstimatedMinutes = templateNode.EstimatedMinutes,
            OrderIndex = orderIndex,
            Status = orderIndex == 1 ? ProgressStatus.Available : ProgressStatus.Locked,
            AiReason = "Fallback template node",
            RequiredNodeId = null
        };
    }
}
