<<<<<<< HEAD
using DuAnTotNghiep.Services.Interfaces;
=======
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52

namespace DuAnTotNghiep.Services
{
    public class AuditService : IAuditService
    {
<<<<<<< HEAD
=======
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public AuditService(IServiceScopeFactory scopeFactory, IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
        {
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
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
                catch (Exception ex)
                {
                    // Do not bubble up exception to break the main flow.
                    Console.WriteLine($"[AuditLog Error] {ex.Message}");
                }
            });

            return Task.CompletedTask;
        }

        public async Task LogActionAsync(int? userId, string action, string? entityName, int? entityId, string? oldValue, string? newValue, string? ipAddress)
        {
            var auditLog = new AuditLog
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

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
>>>>>>> 10d440cfc50975d485254fa28852b6c95afd8a52
    }
}
