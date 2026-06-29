using System.Security.Claims;
using DuAnTotNghiep.Areas.Student.Controllers;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.LearningPath.M8;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DuAnTotNghiep.Tests;

public class M8StudentLearningPathControllerTests
{
    [Fact]
    public async Task Generate_Get_ReturnsReadinessModelForCurrentStudent()
    {
        using var context = CreateContext();
        var readiness = new LearningPathGenerateViewModel { StudentId = 7, HasCompetencyAnalysis = true };
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.CanGeneratePathAsync(7)).ReturnsAsync(readiness);
        var controller = CreateController(context, engine.Object, 7);

        var result = await controller.Generate();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(readiness, view.Model);
    }

    [Fact]
    public async Task Generate_Post_CallsEngineWithLatestCompetencyAnalysisAndRedirects()
    {
        using var context = CreateContext();
        await SeedCompetencyAnalysesAsync(context);
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.GenerateInitialPathAsync(7, 20))
            .ReturnsAsync(new StudentLearningPath { Id = 55, StudentId = 7, Status = LearningPathStatus.Active });
        var controller = CreateController(context, engine.Object, 7);

        var result = await controller.GeneratePost();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Summary", redirect.ActionName);
        engine.Verify(service => service.GenerateInitialPathAsync(7, 20), Times.Once);
    }

    [Fact]
    public async Task Detail_ReturnsOwnedPathDetailModel()
    {
        using var context = CreateContext();
        var detail = new LearningPathDetailViewModel { PathId = 99, Title = "Detail" };
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.GetPathDetailAsync(99, 7)).ReturnsAsync(detail);
        var controller = CreateController(context, engine.Object, 7);

        var result = await controller.Detail(99);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(detail, view.Model);
    }

    [Fact]
    public async Task Detail_WhenEngineRejectsNonOwner_ReturnsForbid()
    {
        using var context = CreateContext();
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.GetPathDetailAsync(99, 7))
            .ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateController(context, engine.Object, 7);

        var result = await controller.Detail(99);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Summary_ReturnsCurrentStudentSummary()
    {
        using var context = CreateContext();
        var summary = new LearningPathSummaryViewModel { PathId = 99, Title = "Summary" };
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.GetPathSummaryAsync(7)).ReturnsAsync(summary);
        var controller = CreateController(context, engine.Object, 7);

        var result = await controller.Summary();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(summary, view.Model);
    }

    [Fact]
    public async Task Regenerate_CallsEngineForCurrentStudentAndRedirectsToSummary()
    {
        using var context = CreateContext();
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.RegeneratePathAsync(7, "Need a harder path"))
            .ReturnsAsync(new StudentLearningPath { Id = 100, StudentId = 7, Status = LearningPathStatus.Active });
        var controller = CreateController(context, engine.Object, 7);

        var result = await controller.Regenerate("Need a harder path");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Summary", redirect.ActionName);
        engine.Verify(service => service.RegeneratePathAsync(7, "Need a harder path"), Times.Once);
    }

    private static async Task SeedCompetencyAnalysesAsync(ApplicationDbContext context)
    {
        context.CompetencyAnalyses.AddRange(
            new CompetencyAnalysis { Id = 10, StudentId = 7, Summary = "Old", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new CompetencyAnalysis { Id = 20, StudentId = 7, Summary = "Latest", CreatedAt = DateTime.UtcNow },
            new CompetencyAnalysis { Id = 30, StudentId = 8, Summary = "Other", CreatedAt = DateTime.UtcNow.AddDays(1) });
        await context.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LearningPathController CreateController(
        ApplicationDbContext context,
        ILearningPathEngineService engine,
        int userId)
    {
        var pathView = new Mock<IPathViewService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "STUDENT")
        }, "TestAuth"));

        return new LearningPathController(pathView.Object, context, NullLogger<LearningPathController>.Instance, engine)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }
}
