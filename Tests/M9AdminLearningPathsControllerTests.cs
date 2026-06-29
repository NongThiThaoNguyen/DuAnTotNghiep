using DuAnTotNghiep.Areas.Admin.Controllers;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.LearningPath;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DuAnTotNghiep.Tests;

public class M9AdminLearningPathsControllerTests
{
    [Fact]
    public void LearningPathsController_UsesAdminAreaAndAuthorization()
    {
        var area = typeof(LearningPathsController)
            .GetCustomAttributes(typeof(AreaAttribute), inherit: false)
            .Cast<AreaAttribute>()
            .Single();
        var authorize = typeof(LearningPathsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal("Admin", area.RouteValue);
        Assert.Equal("ADMIN", authorize.Roles);
    }

    [Fact]
    public async Task Preview_ReturnsReadOnlyLearningPathModelForRequestedStudent()
    {
        var model = new LearningPathPageViewModel
        {
            HasPath = true,
            PathId = 44,
            PathTitle = "Admin Preview"
        };
        var service = new Mock<IPathViewService>();
        service.Setup(s => s.GetCurrentPathPageAsync(77)).ReturnsAsync(model);
        var controller = new LearningPathsController(service.Object);

        var result = await controller.Preview(77);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(model, view.Model);
        Assert.True((bool)view.ViewData["ReadOnly"]!);
        service.Verify(s => s.GetCurrentPathPageAsync(77), Times.Once);
    }
}
