using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.LearningPath;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Validates AI output references against active learning content.
/// </summary>
public static class LearningPathReferenceValidator
{
    private static readonly string[] ActiveTopicStatuses = { "ACTIVE", "Active" };
    private static readonly string[] ApprovedLessonStatuses = { "APPROVED", "Approved" };
    private static readonly string[] AvailableQuizStatuses = { "ACTIVE", "PUBLISHED", "APPROVED" };

    public static async Task ValidateAsync(ApplicationDbContext context, LearningPathOutputDto output)
    {
        var errors = new List<string>();
        foreach (var node in output.Phases.SelectMany(phase => phase.Nodes))
        {
            errors.AddRange(await ValidateNodeReferenceAsync(context, node));
        }

        if (errors.Count > 0) throw new BusinessException(string.Join("; ", errors));
    }

    private static async Task<List<string>> ValidateNodeReferenceAsync(
        ApplicationDbContext context,
        LearningPathOutputNodeDto node)
    {
        var errors = new List<string>();
        if (!NodeType.IsValid(node.ActionType)) errors.Add($"NodeType khong hop le: {node.ActionType}");
        if (node.TopicId.HasValue && !await IsActiveTopicAsync(context, node.TopicId.Value)) errors.Add($"Topic khong kha dung: {node.TopicId}");
        if (node.LessonId.HasValue && !await IsApprovedLessonAsync(context, node.LessonId.Value)) errors.Add($"Lesson khong kha dung: {node.LessonId}");
        if (node.QuizId.HasValue && !await IsAvailableQuizAsync(context, node.QuizId.Value)) errors.Add($"Quiz khong kha dung: {node.QuizId}");
        if (node.EstimatedMinutes <= 0) errors.Add($"Thoi luong node khong hop le: {node.NodeTitle}");
        return errors;
    }

    private static async Task<bool> IsActiveTopicAsync(ApplicationDbContext context, int topicId)
    {
        return await context.LearningTopics.AnyAsync(t => t.Id == topicId && ActiveTopicStatuses.Contains(t.Status));
    }

    private static async Task<bool> IsApprovedLessonAsync(ApplicationDbContext context, int lessonId)
    {
        return await context.OriginalLessons.AnyAsync(l => l.Id == lessonId && ApprovedLessonStatuses.Contains(l.ReviewStatus));
    }

    private static async Task<bool> IsAvailableQuizAsync(ApplicationDbContext context, int quizId)
    {
        return await context.Quizzes.AnyAsync(q => q.Id == quizId && AvailableQuizStatuses.Contains(q.Status));
    }
}
