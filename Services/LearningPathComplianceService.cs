using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

/// <summary>
/// Validates M8 learning path content against publication and reference-only rules.
/// </summary>
public class LearningPathComplianceService : ILearningPathComplianceService
{
    private const string ReferenceOnly = "REFERENCE_ONLY";
    private static readonly string[] AllowedContentStatuses = { "ACTIVE", "PUBLISHED", "APPROVED" };

    private readonly ApplicationDbContext _context;

    public LearningPathComplianceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsCompliant, List<string> Violations)> ValidateContentComplianceAsync(
        List<LearningPathNode> nodes)
    {
        var violations = new List<string>();
        foreach (var node in nodes)
        {
            await ValidateTopicAsync(node, violations);
            await ValidateLessonAsync(node, violations);
            await ValidateQuizAsync(node, violations);
        }

        return (violations.Count == 0, violations);
    }

    private async Task ValidateTopicAsync(LearningPathNode node, List<string> violations)
    {
        if (!node.TopicId.HasValue) return;

        var topic = await _context.LearningTopics.AsNoTracking().FirstOrDefaultAsync(t => t.Id == node.TopicId.Value);
        if (topic == null)
        {
            violations.Add($"Topic {node.TopicId} không tồn tại.");
            return;
        }

        if (!IsAllowedStatus(topic.Status)) violations.Add($"Topic {topic.Id} không ở trạng thái được phép.");
        if (await TopicUsesReferenceOnlySourceAsync(topic.Id)) violations.Add($"Topic {topic.Id} dùng nguồn REFERENCE_ONLY.");
    }

    private async Task ValidateLessonAsync(LearningPathNode node, List<string> violations)
    {
        if (!node.LessonId.HasValue) return;

        var lesson = await _context.OriginalLessons.AsNoTracking().FirstOrDefaultAsync(l => l.Id == node.LessonId.Value);
        if (lesson == null)
        {
            violations.Add($"Lesson {node.LessonId} không tồn tại.");
            return;
        }

        if (!IsAllowedStatus(lesson.ReviewStatus)) violations.Add($"Lesson {lesson.Id} chưa được duyệt.");
        if (IsReferenceOnly(lesson.SourceType)) violations.Add($"Lesson {lesson.Id} dùng nguồn REFERENCE_ONLY.");
    }

    private async Task ValidateQuizAsync(LearningPathNode node, List<string> violations)
    {
        if (!node.QuizId.HasValue) return;

        var quiz = await _context.Quizzes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == node.QuizId.Value);
        if (quiz == null)
        {
            violations.Add($"Quiz {node.QuizId} không tồn tại.");
            return;
        }

        if (!IsAllowedStatus(quiz.Status)) violations.Add($"Quiz {quiz.Id} chưa được publish/approved.");
        if (await QuizUsesReferenceOnlyQuestionAsync(quiz.Id)) violations.Add($"Quiz {quiz.Id} dùng câu hỏi REFERENCE_ONLY.");
    }

    private async Task<bool> TopicUsesReferenceOnlySourceAsync(int topicId)
    {
        return await _context.TopicReferences.AsNoTracking().AnyAsync(reference =>
            reference.TopicId == topicId
            && (reference.ReferenceSource.SourceType == ReferenceSourceType.REFERENCE_ONLY
                || reference.ReferenceSource.UsagePolicy == ReferenceUsagePolicy.REFERENCE_ONLY));
    }

    private async Task<bool> QuizUsesReferenceOnlyQuestionAsync(int quizId)
    {
        return await _context.QuizQuestions.AsNoTracking().AnyAsync(question =>
            question.QuizId == quizId && question.Question.SourceType == ReferenceOnly);
    }

    private static bool IsAllowedStatus(string status)
    {
        return AllowedContentStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsReferenceOnly(string sourceType)
    {
        return ReferenceOnly.Equals(sourceType, StringComparison.OrdinalIgnoreCase);
    }
}
