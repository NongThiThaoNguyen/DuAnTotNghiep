using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.ViewModels.LearningPath;

namespace DuAnTotNghiep.Tests;

public class M9CoreDefinitionsTests
{
    [Fact]
    public void NodeType_ExposesExpectedConstantsAndValidation()
    {
        Assert.Equal("TOPIC", NodeType.Topic);
        Assert.Equal("LESSON", NodeType.Lesson);
        Assert.Equal("QUIZ", NodeType.Quiz);
        Assert.Equal("PRACTICE", NodeType.Practice);
        Assert.Equal("REVIEW", NodeType.Review);
        Assert.Equal("AI_TUTOR", NodeType.AiTutor);

        Assert.Equal(
            new[] { "TOPIC", "LESSON", "QUIZ", "PRACTICE", "REVIEW", "AI_TUTOR" },
            NodeType.All);
        Assert.True(NodeType.IsValid("lesson"));
        Assert.False(NodeType.IsValid("UNKNOWN"));
        Assert.False(NodeType.IsValid(""));
    }

    [Fact]
    public void LearningPathViewModels_ExposeRequiredContracts()
    {
        var completedAt = DateTime.UtcNow;
        var scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var node = new PathNodeViewModel
        {
            NodeId = 10,
            Title = "Present Simple",
            Description = "Learn daily routines.",
            NodeType = NodeType.Lesson,
            Status = ProgressStatus.Available,
            OrderIndex = 2,
            EstimatedMinutes = 15,
            AiReason = "Builds a grammar foundation.",
            TargetUrl = "/Student/Lesson/Details/3",
            IsClickable = true,
            CssClass = "node-available",
            IconClass = "fa-solid fa-book-open",
            StatusLabel = "Available",
            CompletedAt = completedAt,
            ScheduledDate = scheduledDate,
            TopicName = "Grammar"
        };

        var todayTask = new TodayTaskViewModel
        {
            NodeId = node.NodeId,
            Title = node.Title,
            NodeType = node.NodeType,
            AiReason = node.AiReason,
            EstimatedMinutes = node.EstimatedMinutes,
            TargetUrl = node.TargetUrl,
            IsOverdue = false
        };

        var progress = new PathProgressSummaryViewModel
        {
            TotalNodes = 10,
            CompletedNodes = 4,
            InProgressNodes = 1,
            ProgressPercent = 40m,
            TotalStudyMinutes = 120,
            CurrentStreak = 3
        };

        var page = new LearningPathPageViewModel
        {
            PathId = 5,
            PathTitle = "IELTS Starter Path",
            PathDescription = "Daily path",
            PathStatus = "ACTIVE",
            StartDate = scheduledDate,
            TargetEndDate = scheduledDate.AddDays(30),
            GeneratedByAi = true,
            AiPlanSummary = "Focus on grammar and vocabulary.",
            HasPath = true,
            Progress = progress
        };
        page.Nodes.Add(node);
        page.TodayTasks.Add(todayTask);

        Assert.Equal(10, page.Nodes.Single().NodeId);
        Assert.Equal("Present Simple", page.TodayTasks.Single().Title);
        Assert.Equal(40m, page.Progress.ProgressPercent);
        Assert.True(page.HasPath);
    }
}
