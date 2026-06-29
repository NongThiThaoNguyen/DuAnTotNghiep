using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;

namespace DuAnTotNghiep.Services
{
    public class AuditService : IAuditService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task LogAsync(int? userId, string action, string entityName, int? entityId, string? oldValue = null, string? newValue = null)
        {
            // Get IP from current request context before moving to background thread
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            // Fire and forget
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var log = new AuditLog
                    {
                        UserId = userId,
                        Action = action,
                        EntityName = entityName,
                        EntityId = entityId,
                        OldValue = oldValue,
                        NewValue = newValue,
                        IpAddress = ipAddress,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.AuditLogs.Add(log);
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    // Audit persistence should not interrupt the main flow.
                }
            });

            return Task.CompletedTask;
        }

        public Task LogActionAsync(
            int? userId,
            string action,
            string entityName,
            int? entityId,
            string? oldValue = null,
            string? newValue = null,
            string? ipAddress = null)
        {
            return LogAsync(userId, action, entityName, entityId, oldValue, newValue);
        }
    }
}
