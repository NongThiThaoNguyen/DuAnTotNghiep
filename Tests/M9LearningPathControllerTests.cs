using System.Security.Claims;
using DuAnTotNghiep.Areas.Student.Controllers;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.LearningPath;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DuAnTotNghiep.Tests;

public class M9LearningPathControllerTests
{
    [Fact]
    public void LearningPathController_UsesStudentAreaAndStudentAuthorization()
    {
        var area = typeof(LearningPathController)
            .GetCustomAttributes(typeof(AreaAttribute), inherit: false)
            .Cast<AreaAttribute>()
            .Single();
        var authorize = typeof(LearningPathController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal("Student", area.RouteValue);
        Assert.Equal("STUDENT", authorize.Roles);
    }

    [Fact]
    public async Task Index_ReturnsCurrentPathPageModel()
    {
        using var context = CreateContext();
        var expected = new LearningPathPageViewModel
        {
            HasPath = true,
            PathId = 100,
            PathTitle = "Starter Path"
        };
        var service = new Mock<IPathViewService>();
        service.Setup(s => s.GetCurrentPathPageAsync(7)).ReturnsAsync(expected);
        var controller = CreateController(service.Object, context, 7);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(expected, view.Model);
    }

    [Fact]
    public async Task OpenNode_RedirectsToTargetUrl_WhenNodeBelongsToStudentAndCanOpen()
    {
        using var context = CreateContext();
        context.StudentLearningPaths.Add(new StudentLearningPath
        {
            Id = 100,
            StudentId = 7,
            Title = "Starter Path",
            Status = "ACTIVE"
        });
        context.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 202,
            LearningPathId = 100,
            QuizId = 33,
            NodeTitle = "Quiz",
            NodeType = NodeType.Quiz,
            Status = ProgressStatus.Available,
            OrderIndex = 1
        });
        await context.SaveChangesAsync();

        var service = new Mock<IPathViewService>();
        service.Setup(s => s.CanOpenNodeAsync(202, 7)).ReturnsAsync(true);
        service.Setup(s => s.EnsurePathOwnerAsync(100, 7)).ReturnsAsync(true);
        service.Setup(s => s.BuildNodeTargetUrlAsync(It.Is<LearningPathNode>(n => n.Id == 202)))
            .ReturnsAsync("/Student/Quiz/Details/33");
        var controller = CreateController(service.Object, context, 7);

        var result = await controller.OpenNode(202);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Student/Quiz/Details/33", redirect.Url);
        service.Verify(s => s.EnsurePathOwnerAsync(100, 7), Times.Once);
        service.Verify(s => s.CanOpenNodeAsync(202, 7), Times.Once);
        service.Verify(s => s.BuildNodeTargetUrlAsync(It.Is<LearningPathNode>(n => n.Id == 202)), Times.Once);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LearningPathController CreateController(
        IPathViewService service,
        ApplicationDbContext context,
        int userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "STUDENT")
        }, "TestAuth"));

        return new LearningPathController(service, context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }
}
