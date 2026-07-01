using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DuAnTotNghiep.Models;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IAuditLogManagementService
    {
        Task<List<AuditLog>> GetLogsAsync(string? action, string? user, DateTime? from, DateTime? to, int page, int pageSize);
        Task<int> GetTotalCountAsync(string? action, string? user, DateTime? from, DateTime? to);
        Task<List<string>> GetDistinctActionsAsync();
        Task<AuditLog?> GetByIdAsync(long id);
    }
}
