using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DuAnTotNghiep.Tests;

public class M9PathViewServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static PathViewService CreateService(ApplicationDbContext context)
    {
        return new PathViewService(context, NullLogger<PathViewService>.Instance);
    }

    [Fact]
    public async Task GetCurrentPathPageAsync_ReturnsEmptyModel_WhenStudentHasNoActivePath()
    {
        using var context = new ApplicationDbContext(CreateOptions());
        var service = CreateService(context);

        var model = await service.GetCurrentPathPageAsync(42);

        Assert.False(model.HasPath);
        Assert.Empty(model.Nodes);
        Assert.Empty(model.TodayTasks);
        Assert.Equal(0, model.Progress.TotalNodes);
    }

    [Fact]
    public async Task GetCurrentPathPageAsync_MapsActivePathNodesProgressAndTodayTasks()
    {
        var options = CreateOptions();
        var today = DateOnly.FromDateTime(DateTime.Today);

        using (var context = new ApplicationDbContext(options))
        {
            context.Users.Add(new User
            {
                Id = 7,
                Email = "student@test.com",
                FullName = "Student Test",
                PasswordHash = "hash",
                RoleId = 3,
                Status = "ACTIVE"
            });
            context.EnglishSkills.Add(new EnglishSkill
            {
                Id = 1,
                SkillCode = "GRAMMAR",
                SkillName = "Grammar",
                OrderIndex = 1,
                IsActive = true
            });
            context.LearningTopics.Add(new LearningTopic
            {
                Id = 11,
                SkillId = 1,
                Title = "Present Simple",
                DifficultyLevel = "BEGINNER",
                Status = "ACTIVE",
                OrderIndex = 1
            });
            context.StudentLearningPaths.Add(new StudentLearningPath
            {
                Id = 100,
                StudentId = 7,
                Title = "IELTS Starter",
                Description = "Daily foundation path",
                Status = "ACTIVE",
                StartDate = today.AddDays(-1),
                TargetEndDate = today.AddDays(14),
                GeneratedByAi = true,
                AiPlanSummary = "Start with grammar basics.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            context.LearningPathNodes.AddRange(
                new LearningPathNode
                {
                    Id = 201,
                    LearningPathId = 100,
                    TopicId = 11,
                    NodeTitle = "Warm up",
                    NodeDescription = "Review daily routines.",
                    NodeType = NodeType.Lesson,
                    Status = ProgressStatus.Completed,
                    OrderIndex = 1,
                    EstimatedMinutes = 20,
                    ScheduledDate = today.AddDays(-1),
                    CompletedAt = DateTime.UtcNow.AddHours(-2),
                    PathPhase = "Foundation"
                },
                new LearningPathNode
                {
                    Id = 202,
                    LearningPathId = 100,
                    TopicId = 11,
                    QuizId = 33,
                    NodeTitle = "Check understanding",
                    NodeType = NodeType.Quiz,
                    Status = ProgressStatus.Available,
                    OrderIndex = 2,
                    EstimatedMinutes = 15,
                    ScheduledDate = today,
                    AiReason = "Confirm the grammar concept before practice.",
                    PathPhase = "Foundation"
                },
                new LearningPathNode
                {
                    Id = 203,
                    LearningPathId = 100,
                    TopicId = 11,
                    NodeTitle = "Locked practice",
                    NodeType = NodeType.Practice,
                    Status = ProgressStatus.Locked,
                    OrderIndex = 3,
                    EstimatedMinutes = 25,
                    ScheduledDate = today,
                    PathPhase = "Practice"
                });
            context.StudyActivityLogs.AddRange(
                new StudyActivityLog
                {
                    StudentId = 7,
                    ActivityType = ActivityType.Learn,
                    DurationMinutes = 20,
                    CreatedAt = DateTime.Today.AddHours(9)
                },
                new StudyActivityLog
                {
                    StudentId = 7,
                    ActivityType = ActivityType.Quiz,
                    DurationMinutes = 15,
                    CreatedAt = DateTime.Today.AddDays(-1).AddHours(9)
                });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateService(context);

            var model = await service.GetCurrentPathPageAsync(7);

            Assert.True(model.HasPath);
            Assert.Equal(100, model.PathId);
            Assert.Equal("IELTS Starter", model.PathTitle);
            Assert.Equal(3, model.Nodes.Count);
            Assert.Equal(new[] { 1, 2, 3 }, model.Nodes.Select(n => n.OrderIndex));
            Assert.Equal(1, model.Progress.CompletedNodes);
            Assert.Equal(3, model.Progress.TotalNodes);
            Assert.Equal(33.33m, model.Progress.ProgressPercent);
            Assert.Equal(35, model.Progress.TotalStudyMinutes);
            Assert.Equal(2, model.Progress.CurrentStreak);
            var todayTask = Assert.Single(model.TodayTasks);
            Assert.Equal(202, todayTask.NodeId);
            Assert.Equal("/Student/Quiz/Details/33", todayTask.TargetUrl);
            Assert.False(todayTask.IsOverdue);
        }
    }

    [Fact]
    public async Task CanOpenNodeAsync_ReturnsFalseForLockedOrOtherStudentNodes()
    {
        var options = CreateOptions();
        using (var context = new ApplicationDbContext(options))
        {
            context.StudentLearningPaths.AddRange(
                new StudentLearningPath { Id = 1, StudentId = 10, Title = "Owned", Status = "ACTIVE" },
                new StudentLearningPath { Id = 2, StudentId = 11, Title = "Other", Status = "ACTIVE" });
            context.LearningPathNodes.AddRange(
                new LearningPathNode
                {
                    Id = 1,
                    LearningPathId = 1,
                    NodeTitle = "Locked",
                    NodeType = NodeType.Lesson,
                    Status = ProgressStatus.Locked,
                    OrderIndex = 1
                },
                new LearningPathNode
                {
                    Id = 2,
                    LearningPathId = 1,
                    NodeTitle = "Available",
                    NodeType = NodeType.Lesson,
                    Status = ProgressStatus.Available,
                    OrderIndex = 2
                },
                new LearningPathNode
                {
                    Id = 3,
                    LearningPathId = 2,
                    NodeTitle = "Other",
                    NodeType = NodeType.Lesson,
                    Status = ProgressStatus.Available,
                    OrderIndex = 1
                });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateService(context);

            Assert.False(await service.CanOpenNodeAsync(1, 10));
            Assert.True(await service.CanOpenNodeAsync(2, 10));
            Assert.False(await service.CanOpenNodeAsync(3, 10));
            Assert.False(await service.CanOpenNodeAsync(999, 10));
            Assert.True(await service.EnsurePathOwnerAsync(1, 10));
            Assert.False(await service.EnsurePathOwnerAsync(2, 10));
        }
    }

    [Theory]
    [InlineData(NodeType.Topic, 10, null, null, null, "/Student/Topics/Details/10")]
    [InlineData(NodeType.Lesson, null, 20, null, null, "/Student/Lesson/Details/20")]
    [InlineData(NodeType.Quiz, null, null, 30, null, "/Student/Quiz/Details/30")]
    [InlineData(NodeType.Practice, null, null, null, 40, "/Student/Practice/Details/40")]
    [InlineData(NodeType.Review, 10, null, null, null, "/Student/Progress/TopicDetail/10")]
    [InlineData(NodeType.AiTutor, 10, null, null, null, "/Student/AiTutor/Node/99")]
    public async Task BuildNodeTargetUrlAsync_UsesNodeTypeSpecificTargets(
        string nodeType,
        int? topicId,
        int? lessonId,
        int? quizId,
        int? practiceTaskId,
        string expected)
    {
        using var context = new ApplicationDbContext(CreateOptions());
        var service = CreateService(context);
        var node = new LearningPathNode
        {
            Id = 99,
            LearningPathId = 1,
            TopicId = topicId,
            LessonId = lessonId,
            QuizId = quizId,
            PracticeTaskId = practiceTaskId,
            NodeTitle = "Node",
            NodeType = nodeType,
            Status = ProgressStatus.Available,
            OrderIndex = 1
        };

        var targetUrl = await service.BuildNodeTargetUrlAsync(node);

        Assert.Equal(expected, targetUrl);
    }

    [Fact]
    public async Task MarkNodeCompletedAsync_CompletesNodeLogsActivityAndUnlocksNextLockedNode()
    {
        var options = CreateOptions();
        using (var context = new ApplicationDbContext(options))
        {
            context.StudentLearningPaths.Add(new StudentLearningPath
            {
                Id = 500,
                StudentId = 77,
                Title = "Sequential path",
                Status = "ACTIVE"
            });
            context.LearningPathNodes.AddRange(
                new LearningPathNode
                {
                    Id = 501,
                    LearningPathId = 500,
                    TopicId = 11,
                    NodeTitle = "First lesson",
                    NodeType = NodeType.Lesson,
                    Status = ProgressStatus.Available,
                    OrderIndex = 1,
                    EstimatedMinutes = 12
                },
                new LearningPathNode
                {
                    Id = 502,
                    LearningPathId = 500,
                    TopicId = 11,
                    NodeTitle = "Next quiz",
                    NodeType = NodeType.Quiz,
                    Status = ProgressStatus.Locked,
                    OrderIndex = 2
                });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            var service = CreateService(context);

            var completed = await service.MarkNodeCompletedAsync(
                nodeId: 501,
                userId: 77,
                activityType: ActivityType.Learn,
                durationMinutes: 18,
                score: null,
                metadata: "m9_test");

            Assert.True(completed);
        }

        using (var context = new ApplicationDbContext(options))
        {
            var completedNode = await context.LearningPathNodes.FindAsync(501);
            var nextNode = await context.LearningPathNodes.FindAsync(502);
            var log = await context.StudyActivityLogs.SingleAsync();

            Assert.Equal(ProgressStatus.Completed, completedNode!.Status);
            Assert.NotNull(completedNode.CompletedAt);
            Assert.Equal(ProgressStatus.Available, nextNode!.Status);
            Assert.Equal(77, log.StudentId);
            Assert.Equal(ActivityType.Learn, log.ActivityType);
            Assert.Equal(501, log.LearningPathNodeId);
            Assert.Equal(18, log.DurationMinutes);
            Assert.Equal("m9_test", log.Metadata);
        }
    }

    [Fact]
    public async Task TryUnlockNextNodesAsync_ReturnsFalse_WhenNodeDoesNotBelongToStudent()
    {
        using var context = new ApplicationDbContext(CreateOptions());
        context.StudentLearningPaths.Add(new StudentLearningPath
        {
            Id = 600,
            StudentId = 88,
            Title = "Other path",
            Status = "ACTIVE"
        });
        context.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 601,
            LearningPathId = 600,
            NodeTitle = "Other node",
            NodeType = NodeType.Lesson,
            Status = ProgressStatus.Completed,
            OrderIndex = 1
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var unlocked = await service.TryUnlockNextNodesAsync(601, 77);

        Assert.False(unlocked);
    }
}
