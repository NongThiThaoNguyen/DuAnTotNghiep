using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.ViewModels.LearningPath;
using DuAnTotNghiep.ViewModels.LearningPath.M8;

namespace DuAnTotNghiep.Helpers;

/// <summary>
/// Maps learning path entities to M8 student-facing view models.
/// </summary>
public static class LearningPathViewModelFactory
{
    public static LearningPathSummaryViewModel CreateSummary(StudentLearningPath path)
    {
        var nodes = OrderedNodes(path);
        var currentNode = FindCurrentNode(nodes);
        var nextNode = FindNextNode(nodes, currentNode);

        return new LearningPathSummaryViewModel
        {
            PathId = path.Id,
            Title = path.Title,
            Status = path.Status,
            AiPlanSummary = path.AiPlanSummary ?? string.Empty,
            TotalNodes = nodes.Count,
            CompletedNodes = nodes.Count(n => n.Status == ProgressStatus.Completed),
            CurrentNodeTitle = currentNode?.NodeTitle ?? string.Empty,
            NextNodeTitle = nextNode?.NodeTitle ?? string.Empty,
            StartDate = path.StartDate,
            TargetEndDate = path.TargetEndDate,
            PriorityTopics = nodes.Select(n => n.Topic?.Title)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!)
                .Distinct()
                .ToList(),
            GeneratedByAi = path.GeneratedByAi,
            PathVersion = path.PathVersion
        };
    }

    public static LearningPathDetailViewModel CreateDetail(StudentLearningPath path)
    {
        var summary = CreateSummary(path);
        return new LearningPathDetailViewModel
        {
            PathId = summary.PathId,
            Title = summary.Title,
            Status = summary.Status,
            AiPlanSummary = summary.AiPlanSummary,
            TotalNodes = summary.TotalNodes,
            CompletedNodes = summary.CompletedNodes,
            CurrentNodeTitle = summary.CurrentNodeTitle,
            NextNodeTitle = summary.NextNodeTitle,
            StartDate = summary.StartDate,
            TargetEndDate = summary.TargetEndDate,
            PriorityTopics = summary.PriorityTopics,
            GeneratedByAi = summary.GeneratedByAi,
            PathVersion = summary.PathVersion,
            Nodes = OrderedNodes(path).Select(CreateNode).ToList()
        };
    }

    private static List<LearningPathNode> OrderedNodes(StudentLearningPath path)
    {
        return path.LearningPathNodes.OrderBy(n => n.OrderIndex).ToList();
    }

    private static LearningPathNode? FindCurrentNode(List<LearningPathNode> nodes)
    {
        return nodes.FirstOrDefault(n => n.Status == ProgressStatus.InProgress)
            ?? nodes.FirstOrDefault(n => n.Status == ProgressStatus.Available)
            ?? nodes.FirstOrDefault(n => n.Status == ProgressStatus.NeedReview);
    }

    private static LearningPathNode? FindNextNode(List<LearningPathNode> nodes, LearningPathNode? currentNode)
    {
        if (currentNode == null) return nodes.FirstOrDefault(n => n.Status == ProgressStatus.Locked);
        return nodes.FirstOrDefault(n => n.OrderIndex > currentNode.OrderIndex && n.Status == ProgressStatus.Locked);
    }

    private static PathNodeViewModel CreateNode(LearningPathNode node)
    {
        return new PathNodeViewModel
        {
            NodeId = node.Id,
            Title = node.NodeTitle,
            Description = node.NodeDescription,
            NodeType = node.NodeType,
            Status = node.Status,
            OrderIndex = node.OrderIndex,
            EstimatedMinutes = node.EstimatedMinutes,
            AiReason = node.AiReason,
            IsClickable = node.Status == ProgressStatus.Available || node.Status == ProgressStatus.InProgress,
            CssClass = $"m8-node-status--{node.Status.ToLowerInvariant().Replace("_", "-")}",
            IconClass = GetIconClass(node.NodeType),
            StatusLabel = node.Status.Replace("_", " "),
            CompletedAt = node.CompletedAt,
            ScheduledDate = node.ScheduledDate,
            TopicName = node.Topic?.Title,
            PathPhase = node.PathPhase
        };
    }

    private static string GetIconClass(string nodeType)
    {
        return nodeType switch
        {
            NodeType.Topic => "fa-solid fa-book-open",
            NodeType.Lesson => "fa-solid fa-graduation-cap",
            NodeType.Quiz => "fa-solid fa-circle-question",
            NodeType.Practice => "fa-solid fa-pen-to-square",
            NodeType.Review => "fa-solid fa-rotate-right",
            NodeType.AiTutor => "fa-solid fa-robot",
            _ => "fa-solid fa-circle"
        };
    }
}
