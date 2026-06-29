using System.Security.Claims;
using DuAnTotNghiep.Areas.Admin.Controllers;
using DuAnTotNghiep.Areas.Student.Controllers;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels.Admin.LearningPaths;
using DuAnTotNghiep.Models.ViewModels.LearningPath.M8;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DuAnTotNghiep.Tests;

public class M8ControllerTests
{
    [Fact]
    public async Task StudentGeneratePost_WhenReady_RedirectsToSummary()
    {
        using var context = CreateContext();
        SeedCompetencyAnalysis(context);
        await context.SaveChangesAsync();
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.GenerateInitialPathAsync(7, 20))
            .ReturnsAsync(new StudentLearningPath { Id = 100, StudentId = 7, Status = LearningPathStatus.Active });
        var controller = CreateStudentController(context, engine.Object, 7);

        var result = await controller.GeneratePost();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Summary", redirect.ActionName);
    }

    [Fact]
    public async Task StudentDetail_WhenPathBelongsToAnotherStudent_ReturnsForbid()
    {
        using var context = CreateContext();
        var engine = new Mock<ILearningPathEngineService>();
        engine.Setup(service => service.GetPathDetailAsync(100, 7))
            .ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateStudentController(context, engine.Object, 7);

        var result = await controller.Detail(100);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task AdminPathHistory_WhenPageTwoRequested_ReturnsPagedModel()
    {
        using var context = CreateContext();
        SeedPathHistory(context, 25);
        await context.SaveChangesAsync();
        var controller = CreateAdminController(context);

        var result = await controller.PathHistory(page: 2);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PathHistoryViewModel>(view.Model);
        Assert.Equal(2, model.Page);
        Assert.Equal(2, model.TotalPages);
        Assert.Equal(5, model.Paths.Count);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LearningPathController CreateStudentController(
        ApplicationDbContext context,
        ILearningPathEngineService engine,
        int userId)
    {
        var pathView = new Mock<IPathViewService>();
        return new LearningPathController(pathView.Object, context, NullLogger<LearningPathController>.Instance, engine)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = CreateUser(userId, "STUDENT") }
            }
        };
    }

    private static LearningPathsController CreateAdminController(ApplicationDbContext context)
    {
        var pathView = new Mock<IPathViewService>();
        return new LearningPathsController(pathView.Object, context);
    }

    private static ClaimsPrincipal CreateUser(int userId, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth"));
    }

    private static void SeedCompetencyAnalysis(ApplicationDbContext context)
    {
        context.CompetencyAnalyses.Add(new CompetencyAnalysis
        {
            Id = 20,
            StudentId = 7,
            Summary = "Ready",
            CreatedAt = DateTime.UtcNow
        });
    }

    private static void SeedPathHistory(ApplicationDbContext context, int count)
    {
        context.Users.Add(new User { Id = 7, Email = "student@test.com", FullName = "Student", PasswordHash = "hash", Status = "ACTIVE" });
        for (var index = 0; index < count; index++)
        {
            context.StudentLearningPaths.Add(new StudentLearningPath
            {
                Id = 100 + index,
                StudentId = 7,
                Title = $"Path {index}",
                Status = LearningPathStatus.Active,
                GeneratedByAi = true,
                CreatedAt = DateTime.UtcNow.AddMinutes(-index),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-index)
            });
        }
    }
}
