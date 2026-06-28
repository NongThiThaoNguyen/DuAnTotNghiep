using System.Security.Claims;
using DuAnTotNghiep.Areas.Student.Controllers;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs.Progress;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DuAnTotNghiep.Tests;

public class M9ProgressControllerIntegrationTests
{
    [Fact]
    public async Task Record_CompletingPathNode_UsesPathViewServiceAndRefreshesSnapshots()
    {
        using var context = CreateContext();
        var studentProgressService = new Mock<IStudentProgressService>();
        var trackingService = new Mock<IProgressTrackingService>();
        var progressRepo = new Mock<IProgressRepository>();
        var pathViewService = new Mock<IPathViewService>();
        pathViewService
            .Setup(s => s.MarkNodeCompletedAsync(501, 77, ActivityType.Learn, 18, null, "m9_record"))
            .ReturnsAsync(true);

        var controller = new ProgressController(
            studentProgressService.Object,
            trackingService.Object,
            progressRepo.Object,
            context,
            pathViewService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "77"),
                        new Claim(ClaimTypes.Role, "STUDENT")
                    }, "TestAuth"))
                }
            }
        };

        var result = await controller.Record(new ActivityLogCreateDto
        {
            ActivityType = ActivityType.Learn,
            LearningPathNodeId = 501,
            DurationMinutes = 18,
            Metadata = "m9_record"
        });

        Assert.IsType<OkObjectResult>(result);
        pathViewService.Verify(
            s => s.MarkNodeCompletedAsync(501, 77, ActivityType.Learn, 18, null, "m9_record"),
            Times.Once);
        studentProgressService.Verify(s => s.UpdateProgressSnapshotsAsync(77), Times.Once);
        trackingService.Verify(
            s => s.RecordLessonCompleted(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()),
            Times.Never);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
