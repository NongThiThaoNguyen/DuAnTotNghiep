using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class AuditLogManagementService : IAuditLogManagementService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        private IQueryable<AuditLog> BuildQuery(string? action, string? user, DateTime? from, DateTime? to)
        {
            var query = _context.AuditLogs.AsNoTracking().Include(l => l.User).AsQueryable();

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(l => l.Action == action);
            }

            if (!string.IsNullOrEmpty(user))
            {
                query = query.Where(l => l.User != null && (l.User.Email.Contains(user) || l.User.FullName.Contains(user)));
            }

            if (from.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                // Include the whole day
                var toDate = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(l => l.CreatedAt <= toDate);
            }

            return query;
        }

        public async Task<List<AuditLog>> GetLogsAsync(string? action, string? user, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = BuildQuery(action, user, from, to);
            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(string? action, string? user, DateTime? from, DateTime? to)
        {
            var query = BuildQuery(action, user, from, to);
            return await query.CountAsync();
        }

        public async Task<List<string>> GetDistinctActionsAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Select(l => l.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(long id)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}
