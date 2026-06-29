using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Maps AI learning path output objects into EF entities.
/// </summary>
public static class LearningPathEntityFactory
{
    public static StudentLearningPath CreatePath(
        LearningPathOutputDto output,
        StudentLearningProfile profile,
        int studentId,
        int competencyAnalysisId,
        DateTime now)
    {
        return new StudentLearningPath
        {
            StudentId = studentId,
            CompetencyAnalysisId = competencyAnalysisId,
            GoalId = profile.MainGoalId,
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

    public static LearningPathNode CreateNode(
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
            ScheduledDate = node.ScheduledDay.HasValue ? DateOnly.FromDateTime(now.AddDays(node.ScheduledDay.Value)) : null,
            EstimatedMinutes = node.EstimatedMinutes,
            OrderIndex = orderIndex,
            Status = orderIndex == 1 ? ProgressStatus.Available : ProgressStatus.Locked,
            AiReason = node.AiReason
        };
    }
}
