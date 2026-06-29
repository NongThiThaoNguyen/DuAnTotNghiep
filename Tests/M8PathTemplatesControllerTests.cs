using DuAnTotNghiep.Areas.Admin.Controllers;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8PathTemplatesControllerTests
{
    [Fact]
    public async Task Create_Post_SavesTemplateWithNodes()
    {
        using var context = CreateContext();
        var controller = CreateController(context);
        var template = CreateTemplate();
        template.LearningPathTemplateNodes.Add(CreateNode());

        var result = await controller.Create(template);

        Assert.IsType<RedirectToActionResult>(result);
        var saved = await context.LearningPathTemplates.Include(t => t.LearningPathTemplateNodes).SingleAsync();
        Assert.Equal("Template A", saved.TemplateName);
        Assert.Single(saved.LearningPathTemplateNodes);
    }

    [Fact]
    public async Task Publish_WhenTemplateHasNoNodes_DoesNotPublish()
    {
        using var context = CreateContext();
        context.LearningPathTemplates.Add(CreateTemplate());
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        var result = await controller.Publish(1);

        Assert.IsType<RedirectToActionResult>(result);
        var template = await context.LearningPathTemplates.FindAsync(1);
        Assert.Equal("DRAFT", template!.Status);
    }

    [Fact]
    public async Task Publish_WhenTemplateHasNodes_SetsPublished()
    {
        using var context = CreateContext();
        var template = CreateTemplate();
        template.LearningPathTemplateNodes.Add(CreateNode());
        context.LearningPathTemplates.Add(template);
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        await controller.Publish(1);

        var saved = await context.LearningPathTemplates.FindAsync(1);
        Assert.Equal("PUBLISHED", saved!.Status);
    }

    [Fact]
    public async Task Archive_SetsTemplateStatusArchived()
    {
        using var context = CreateContext();
        context.LearningPathTemplates.Add(CreateTemplate());
        await context.SaveChangesAsync();
        var controller = CreateController(context);

        await controller.Archive(1);

        var saved = await context.LearningPathTemplates.FindAsync(1);
        Assert.Equal("ARCHIVED", saved!.Status);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static PathTemplatesController CreateController(ApplicationDbContext context)
    {
        var controller = new PathTemplatesController(context);
        controller.TempData = new TempDataDictionary(
            new DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
        return controller;
    }

    private static LearningPathTemplate CreateTemplate()
    {
        return new LearningPathTemplate
        {
            Id = 1,
            TemplateName = "Template A",
            DurationWeeks = 4,
            Status = "DRAFT",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static LearningPathTemplateNode CreateNode()
    {
        return new LearningPathTemplateNode
        {
            NodeTitle = "Node A",
            NodeType = NodeType.Lesson,
            EstimatedMinutes = 20,
            OrderIndex = 1
        };
    }
}
