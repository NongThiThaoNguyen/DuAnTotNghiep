using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services;

public sealed class ScheduleMeetBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduleMeetBackgroundService> _logger;

    public ScheduleMeetBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduleMeetBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await GenerateUpcomingLinksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tự tạo Google Meet cho lịch học sắp diễn ra.");
            }
        }
    }

    private async Task GenerateUpcomingLinksAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var meetService = scope.ServiceProvider.GetRequiredService<IGoogleMeetService>();
        var now = DateTime.Now;
        var until = now.AddMinutes(15);
        var schedules = await context.Schedules
            .Where(s => string.IsNullOrWhiteSpace(s.MeetUrl)
                && s.StartTime <= until
                && s.EndTime >= now)
            .ToListAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            schedule.MeetUrl = meetService.GenerateMeetUrl(schedule.Id, schedule.Title);
        }

        if (schedules.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}