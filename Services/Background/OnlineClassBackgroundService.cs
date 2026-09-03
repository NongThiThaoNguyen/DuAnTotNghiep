using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DuAnTotNghiep.Services.Background;

public class OnlineClassBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OnlineClassBackgroundService> _logger;

    public OnlineClassBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OnlineClassBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OnlineClassBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMeetLinksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing online class Google Meet links & notifications.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcessPendingMeetLinksAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var meetService = scope.ServiceProvider.GetRequiredService<IGoogleMeetService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var now = DateTime.Now;
        var upcomingThreshold = now.AddMinutes(15);

        var pendingSchedules = await context.Schedules
            .Include(s => s.Teacher)
            .Include(s => s.Topic)
            .Where(s => !s.MeetNotificationSent && s.StartTime <= upcomingThreshold && s.EndTime >= now.AddMinutes(-30))
            .ToListAsync(stoppingToken);

        if (!pendingSchedules.Any())
        {
            return;
        }

        foreach (var schedule in pendingSchedules)
        {
            if (string.IsNullOrWhiteSpace(schedule.MeetUrl))
            {
                schedule.MeetUrl = meetService.GenerateMeetUrl(schedule.Id, schedule.Title);
            }

            // 1. Notify Teacher
            var teacherTitle = "🎥 Link phòng học Google Meet đã sẵn sàng";
            var teacherContent = $"Buổi học \"{schedule.Title}\" ({schedule.StartTime:HH:mm dd/MM/yyyy}) sắp diễn ra. Truy cập phòng học: {schedule.MeetUrl}";
            await notificationService.SendToUserAsync(
                teacherTitle,
                teacherContent,
                "ONLINE_CLASS",
                schedule.TeacherId,
                schedule.TeacherId);

            // 2. Find target students
            var studentIds = await context.Enrollments
                .AsNoTracking()
                .Where(e => e.Status == "ACTIVE" && e.Classroom.TeacherId == schedule.TeacherId)
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync(stoppingToken);

            if (!studentIds.Any())
            {
                // Fallback to active students in system if specific classroom enrollment is not filtered
                studentIds = await context.Users
                    .AsNoTracking()
                    .Where(u => u.Status == "ACTIVE" && u.Role.RoleCode == "STUDENT")
                    .Select(u => u.Id)
                    .Take(50)
                    .ToListAsync(stoppingToken);
            }

            var studentTitle = "🎥 Thông báo buổi học Online (Google Meet)";
            var studentContent = $"Buổi học \"{schedule.Title}\" ({schedule.StartTime:HH:mm dd/MM/yyyy}) đã sẵn sàng. Tham gia phòng học tại: {schedule.MeetUrl}";

            foreach (var studentId in studentIds)
            {
                await notificationService.SendToUserAsync(
                    studentTitle,
                    studentContent,
                    "ONLINE_CLASS",
                    studentId,
                    schedule.TeacherId);
            }

            schedule.MeetNotificationSent = true;
        }

        await context.SaveChangesAsync(stoppingToken);
    }
}
