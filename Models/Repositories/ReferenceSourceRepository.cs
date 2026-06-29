using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Models.ViewModels.Admin.ReferenceSources;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories
{
    public class ReferenceSourceRepository : GenericRepository<ReferenceSource>, IReferenceSourceRepository
    {
        public ReferenceSourceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ReferenceSource>> GetListAsync(ReferenceSourceType? sourceType, ReferenceReviewStatus? status, string? keyword)
        {
            var query = _dbSet.AsNoTracking()
                              .Include(r => r.CreatedByNavigation)
                              .Include(r => r.ApprovedByNavigation)
                              .AsQueryable();

            if (sourceType.HasValue)
            {
                query = query.Where(r => r.SourceType == sourceType.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(r => r.SourceName.ToLower().Contains(kw));
            }

            return await query.ToListAsync();
        }

        public async Task<ReferenceSource?> GetDetailsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.CreatedByNavigation)
                .Include(r => r.ApprovedByNavigation)
                .Include(r => r.TopicReferences)
                    .ThenInclude(tr => tr.Topic)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task CreateAsync(ReferenceSource source)
        {
            await AddAsync(source);
        }

        public Task UpdateAsync(ReferenceSource source)
        {
            Update(source);
            return Task.CompletedTask;
        }

        public async Task ArchiveAsync(int id)
        {
            var source = await GetByIdAsync(id);
            if (source != null)
            {
                source.Status = ReferenceReviewStatus.ARCHIVED;
                source.IsActive = false;
                source.UpdatedAt = DateTime.UtcNow;
                Update(source);
            }
        }

        public async Task<bool> ExistsByUrlAsync(string url, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            var targetUrl = url.Trim().ToLower();

            if (excludeId.HasValue)
            {
                return await _dbSet.AnyAsync(r => r.SourceUrl != null && r.SourceUrl.ToLower() == targetUrl && r.Id != excludeId.Value);
            }
            return await _dbSet.AnyAsync(r => r.SourceUrl != null && r.SourceUrl.ToLower() == targetUrl);
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var targetName = name.Trim().ToLower();

            if (excludeId.HasValue)
            {
                return await _dbSet.AnyAsync(r => r.SourceName.ToLower() == targetName && r.Id != excludeId.Value);
            }
            return await _dbSet.AnyAsync(r => r.SourceName.ToLower() == targetName);
        }

        public async Task<IEnumerable<ReferenceSource>> GetPendingListAsync()
        {
            return await _dbSet.AsNoTracking()
                              .Include(r => r.CreatedByNavigation)
                              .Include(r => r.ApprovedByNavigation)
                              .Where(r => r.Status == ReferenceReviewStatus.PENDING)
                              .ToListAsync();
        }

        public async Task<bool> IsUsedInTopicAsync(int id)
        {
            return await _context.TopicReferences.AnyAsync(tr => tr.ReferenceSourceId == id);
        }

        public async Task<bool> IsUsedInLessonAsync(int id)
        {
            return await _context.OriginalLessons.AnyAsync(l =>
                _context.TopicReferences.Any(tr => tr.ReferenceSourceId == id && tr.TopicId == l.TopicId));
        }

        public async Task<bool> IsUsedInAiWorkflowAsync(int id)
        {
            return await _context.AiGeneratedContents.AnyAsync(ai =>
                _context.TopicReferences.Any(tr => tr.ReferenceSourceId == id && tr.TopicId == ai.RelatedTopicId));
        }

        // --- Task 6 Implementations ---

        public async Task<(IEnumerable<ReferenceSourceListItemViewModel> Items, int TotalItems)> GetPagedListAsync(string? keyword, ReferenceSourceType? sourceType, ReferenceReviewStatus? status, int page, int pageSize)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (sourceType.HasValue)
            {
                query = query.Where(r => r.SourceType == sourceType.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(r => r.SourceName.ToLower().Contains(kw) || (r.SourceUrl != null && r.SourceUrl.ToLower().Contains(kw)));
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReferenceSourceListItemViewModel
                {
                    Id = r.Id,
                    SourceName = r.SourceName,
                    SourceUrl = r.SourceUrl,
                    SourceType = r.SourceType,
                    Status = r.Status,
                    CreatedById = r.CreatedBy,
                    CreatedByUserName = r.CreatedByNavigation != null ? r.CreatedByNavigation.FullName : "Hệ thống",
                    CreatedAt = r.CreatedAt,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<Dictionary<ReferenceReviewStatus, int>> CountByStatusAsync()
        {
            var list = await _dbSet.AsNoTracking()
                                  .GroupBy(r => r.Status)
                                  .Select(g => new { Status = g.Key, Count = g.Count() })
                                  .ToListAsync();

            var result = Enum.GetValues(typeof(ReferenceReviewStatus))
                             .Cast<ReferenceReviewStatus>()
                             .ToDictionary(s => s, s => 0);

            foreach (var item in list)
            {
                result[item.Status] = item.Count;
            }

            return result;
        }

        public async Task<List<OriginalLesson>> GetLinkedLessonsAsync(int referenceSourceId)
        {
            return await _context.OriginalLessons
                .AsNoTracking()
                .Include(l => l.Topic)
                .Where(l => _context.TopicReferences.Any(tr => tr.ReferenceSourceId == referenceSourceId && tr.TopicId == l.TopicId))
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetRecentAuditLogsAsync(int referenceSourceId, int limit)
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => a.EntityName == "ReferenceSource" && a.EntityId == referenceSourceId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddComplianceReviewAsync(ContentComplianceReview review)
        {
            await _context.ContentComplianceReviews.AddAsync(review);
        }
    }
}
