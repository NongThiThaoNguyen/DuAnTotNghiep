using DuAnTotNghiep.Areas.Admin.Controllers;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Admin.LearningPaths;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8AdminLearningPathsControllerTests
{
    [Fact]
    public async Task PathHistory_FiltersByStatusAndReturnsPagedPaths()
    {
        using var context = CreateContext();
        SeedPaths(context);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.PathHistory(page: 1, status: LearningPathStatus.Active);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PathHistoryViewModel>(view.Model);
        var path = Assert.Single(model.Paths);
        Assert.Equal(LearningPathStatus.Active, path.Status);
        Assert.Equal(1, model.Page);
        Assert.Equal(LearningPathStatus.Active, model.Status);
    }

    [Fact]
    public async Task PathDetail_ReturnsPathWithNodesForAnyStudent()
    {
        using var context = CreateContext();
        SeedPaths(context);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.PathDetail(100);

        var view = Assert.IsType<ViewResult>(result);
        var path = Assert.IsType<PathDetailAdminViewModel>(view.Model);
        Assert.Equal(100, path.PathId);
        Assert.NotEmpty(path.Nodes);
    }

    [Fact]
    public async Task GenerationLogs_ReturnsOnlyLearningPathLogs()
    {
        using var context = CreateContext();
        context.Users.Add(new User { Id = 7, Email = "log@test.com", FullName = "Log Student", PasswordHash = "hash", Status = "ACTIVE" });
        context.AiPromptTemplates.Add(new AiPromptTemplate
        {
            Id = 9,
            PromptCode = "LP",
            PromptName = "Learning Path",
            ModuleCode = "LEARNING_PATH",
            SystemPrompt = "Prompt",
            Status = "ACTIVE",
            VersionNo = 3,
            CreatedAt = DateTime.UtcNow
        });
        context.AiUsageLogs.AddRange(
            new AiUsageLog { Id = 1, UserId = 7, PromptTemplateId = 9, ModuleCode = "LEARNING_PATH", AiModel = "gemini", RequestStatus = "SUCCESS", CreatedAt = DateTime.UtcNow },
            new AiUsageLog { Id = 2, ModuleCode = "OTHER", RequestStatus = "SUCCESS", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.GenerationLogs();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<GenerationLogViewModel>(view.Model);
        var log = Assert.Single(model.Logs);
        Assert.Equal("LEARNING_PATH", model.ModuleCode);
        Assert.Equal("gemini", log.AiModel);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LearningPathsController CreateController(ApplicationDbContext context)
    {
        var pathView = new Mock<IPathViewService>();
        return new LearningPathsController(pathView.Object, context);
    }

    private static void SeedPaths(ApplicationDbContext context)
    {
        var now = DateTime.UtcNow;
        context.Users.Add(new User
        {
            Id = 7,
            Email = "student@test.com",
            FullName = "Student",
            PasswordHash = "hash",
            Status = "ACTIVE"
        });
        context.StudentLearningPaths.AddRange(
            CreatePath(100, 7, LearningPathStatus.Active, now),
            CreatePath(101, 7, LearningPathStatus.Archived, now.AddDays(-1)));
        context.LearningPathNodes.Add(CreateNode(201, 100));
    }

    private static StudentLearningPath CreatePath(int id, int studentId, string status, DateTime createdAt)
    {
        return new StudentLearningPath
        {
            Id = id,
            StudentId = studentId,
            Title = $"Path {id}",
            Status = status,
            GeneratedByAi = true,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private static LearningPathNode CreateNode(int id, int pathId)
    {
        return new LearningPathNode
        {
            Id = id,
            LearningPathId = pathId,
            NodeTitle = "Node",
            NodeType = NodeType.Lesson,
            Status = ProgressStatus.Available,
            OrderIndex = 1
        };
    }
}
