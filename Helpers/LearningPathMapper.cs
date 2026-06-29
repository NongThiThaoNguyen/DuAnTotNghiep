using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Maps AI learning path output DTOs into learning path entities.
/// </summary>
public static class LearningPathMapper
{
    public static (StudentLearningPath Path, List<LearningPathNode> Nodes) MapOutputToEntities(
        LearningPathOutputDto output,
        int studentId,
        int competencyAnalysisId)
    {
        var now = DateTime.UtcNow;
        var path = CreatePath(output, studentId, competencyAnalysisId, now);
        var nodes = output.Phases
            .SelectMany(phase => phase.Nodes)
            .Select((node, index) => CreateNode(node, path, index + 1, now))
            .ToList();

        foreach (var node in nodes) path.LearningPathNodes.Add(node);
        return (path, nodes);
    }

    private static StudentLearningPath CreatePath(
        LearningPathOutputDto output,
        int studentId,
        int competencyAnalysisId,
        DateTime now)
    {
        return new StudentLearningPath
        {
            StudentId = studentId,
            CompetencyAnalysisId = competencyAnalysisId,
            Title = output.PathTitle,
            Description = output.Summary,
            AiPlanSummary = output.Summary,
            Status = LearningPathStatus.Active,
            GeneratedByAi = true,
            PathVersion = 1,
            StartDate = DateOnly.FromDateTime(now),
            TargetEndDate = DateOnly.FromDateTime(now.AddDays(Math.Max(1, output.TotalWeeks) * 7)),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static LearningPathNode CreateNode(
        LearningPathOutputNodeDto node,
        StudentLearningPath path,
        int orderIndex,
        DateTime now)
    {
        return new LearningPathNode
        {
            LearningPath = path,
            TopicId = node.TopicId,
            LessonId = node.LessonId,
            QuizId = node.QuizId,
            PracticeTaskId = node.PracticeTaskId,
            NodeTitle = node.NodeTitle,
            NodeDescription = node.NodeDescription,
            NodeType = node.ActionType,
            PathPhase = node.PathPhase,
            ScheduledDate = GetScheduledDate(node.ScheduledDay, now),
            EstimatedMinutes = node.EstimatedMinutes,
            OrderIndex = orderIndex,
            Status = orderIndex == 1 ? ProgressStatus.Available : ProgressStatus.Locked,
            AiReason = node.AiReason
        };
    }

    private static DateOnly? GetScheduledDate(int? scheduledDay, DateTime now)
    {
        return scheduledDay.HasValue ? DateOnly.FromDateTime(now.AddDays(scheduledDay.Value)) : null;
    }
}
