using DuAnTotNghiep.Models.DTOs.LearningPath;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Exceptions;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Builds deterministic mock AI output for the learning path engine.
/// </summary>
public static class LearningPathMockOutputBuilder
{
    public static LearningPathOutputDto Generate(LearningPathInputDto input)
    {
        var nodes = new List<LearningPathOutputNodeDto?>
        {
            CreateNode(input.AvailableTopics.FirstOrDefault(), NodeType.Topic, "Start with core topic", 1),
            CreateNode(input.AvailableLessons.FirstOrDefault(), NodeType.Lesson, "Study the recommended lesson", 2),
            CreateNode(input.AvailableQuizzes.FirstOrDefault(), NodeType.Quiz, "Check understanding with a quiz", 3)
        }.Where(n => n != null).Cast<LearningPathOutputNodeDto>().ToList();

        if (nodes.Count == 0) throw new BusinessException("Không có nội dung học tập khả dụng để tạo lộ trình.");

        return new LearningPathOutputDto
        {
            PathTitle = $"{input.GoalName} learning path".Trim(),
            Summary = $"Focus on {string.Join(", ", input.PriorityTopics.DefaultIfEmpty("core skills"))}.",
            TotalWeeks = 4,
            Phases = new List<LearningPathOutputPhaseDto> { new() { PhaseName = "Foundation", Weeks = 4, Nodes = nodes } }
        };
    }

    private static LearningPathOutputNodeDto? CreateNode(
        LearningPathResourceDto? resource,
        string nodeType,
        string reason,
        int scheduledDay)
    {
        if (resource == null) return null;

        return new LearningPathOutputNodeDto
        {
            NodeTitle = resource.Name,
            NodeDescription = reason,
            ActionType = nodeType,
            TopicId = nodeType == NodeType.Topic ? resource.Id : null,
            LessonId = nodeType == NodeType.Lesson ? resource.Id : null,
            QuizId = nodeType == NodeType.Quiz ? resource.Id : null,
            EstimatedMinutes = 20,
            AiReason = reason,
            ScheduledDay = scheduledDay,
            PathPhase = "Foundation"
        };
    }
}
