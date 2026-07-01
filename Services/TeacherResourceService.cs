using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class TeacherResourceService : ITeacherResourceService
    {
        private readonly ApplicationDbContext _context;

        public TeacherResourceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(List<ReferenceSource> Items, int TotalItems)> GetResourcesAsync(string? keyword, ReferenceSourceType? sourceType, int page, int pageSize)
        {
            var query = _context.ReferenceSources.AsNoTracking();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.SourceName.Contains(keyword) ||
                                         (r.Description != null && r.Description.Contains(keyword)) ||
                                         (r.Author != null && r.Author.Contains(keyword)));
            }

            if (sourceType.HasValue)
            {
                query = query.Where(r => r.SourceType == sourceType.Value);
            }

            int totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<ReferenceSource?> GetResourceByIdAsync(int id)
        {
            return await _context.ReferenceSources
                .Include(r => r.CreatedByNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task CreateResourceAsync(ReferenceSource resource, int teacherId)
        {
            resource.CreatedAt = DateTime.UtcNow;
            resource.UpdatedAt = DateTime.UtcNow;
            resource.CreatedBy = teacherId;
            resource.IsActive = true;
            resource.Status = ReferenceReviewStatus.APPROVED; // Teacher created is auto approved

            _context.Add(resource);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateResourceAsync(ReferenceSource resource)
        {
            var existing = await _context.ReferenceSources.FindAsync(resource.Id);
            if (existing != null)
            {
                existing.SourceName = resource.SourceName;
                existing.SourceUrl = resource.SourceUrl;
                existing.SourceType = resource.SourceType;
                existing.LicenseNote = resource.LicenseNote;
                existing.Author = resource.Author;
                existing.Organization = resource.Organization;
                existing.Description = resource.Description;
                existing.UsagePolicy = resource.UsagePolicy;
                existing.IsActive = resource.IsActive;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteResourceAsync(int id)
        {
            var resource = await _context.ReferenceSources.FindAsync(id);
            if (resource != null)
            {
                _context.ReferenceSources.Remove(resource);
                await _context.SaveChangesAsync();
            }
        }

        public bool ResourceExists(int id)
        {
            return _context.ReferenceSources.Any(e => e.Id == id);
        }
    }
}
