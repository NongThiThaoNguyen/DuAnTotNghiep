using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Data;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class ProgressSnapshotBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ProgressSnapshotBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Trì hoãn khởi động ban đầu khoảng 30 giây để tránh làm chậm tiến trình khởi chạy ứng dụng
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var progressService = scope.ServiceProvider.GetRequiredService<IProgressTrackingService>();

                        // Lấy tất cả studentId trong hệ thống để định kỳ cập nhật snapshot tiến độ
                        var studentIds = await context.Users
                            .Where(u => u.Role.RoleCode == "STUDENT")
                            .Select(u => u.Id)
                            .ToListAsync(stoppingToken);

                        foreach (var studentId in studentIds)
                        {
                            if (stoppingToken.IsCancellationRequested) break;

                            try
                            {
                                await progressService.RecalculateStudentProgress(studentId);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error recalculating progress for student ID {studentId}: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in ProgressSnapshotBackgroundService execution: {ex.Message}");
                }

                // Chờ 24 giờ trước chu kỳ chạy tiếp theo
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
