using DuAnTotNghiep.Data;
using DuAnTotNghiep.Enums;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Repositories;

/// <summary>
/// Provides Entity Framework data access for student learning paths.
/// </summary>
public class LearningPathRepository : ILearningPathRepository
{
    private readonly ApplicationDbContext _context;

    public LearningPathRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentLearningPath?> GetActivePathByStudentIdAsync(int studentId)
    {
        return await _context.StudentLearningPaths
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.Status == LearningPathStatus.Active);
    }

    public async Task<StudentLearningPath?> GetPathWithNodesAsync(int pathId)
    {
        return await _context.StudentLearningPaths
            .AsNoTracking()
            .Include(p => p.LearningPathNodes.OrderBy(n => n.OrderIndex))
            .FirstOrDefaultAsync(p => p.Id == pathId);
    }

    public async Task<List<StudentLearningPath>> GetPathHistoryByStudentIdAsync(int studentId)
    {
        return await _context.StudentLearningPaths
            .AsNoTracking()
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task AddPathAsync(StudentLearningPath path)
    {
        await _context.StudentLearningPaths.AddAsync(path);
    }

    public Task UpdatePathAsync(StudentLearningPath path)
    {
        _context.StudentLearningPaths.Update(path);
        return Task.CompletedTask;
    }

    public async Task AddNodesAsync(IEnumerable<LearningPathNode> nodes)
    {
        await _context.LearningPathNodes.AddRangeAsync(nodes);
    }

    public async Task<(IEnumerable<StudentLearningPath> Paths, int TotalCount)> GetAllPathsPagedAsync(
        int page,
        int pageSize,
        string? statusFilter)
    {
        var query = BuildPagedQuery(statusFilter);
        var totalCount = await query.CountAsync();
        var paths = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((NormalizePage(page) - 1) * NormalizePageSize(pageSize))
            .Take(NormalizePageSize(pageSize))
            .ToListAsync();

        return (paths, totalCount);
    }

    private IQueryable<StudentLearningPath> BuildPagedQuery(string? statusFilter)
    {
        var query = _context.StudentLearningPaths
            .AsNoTracking()
            .Include(p => p.Student)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(p => p.Status == statusFilter);
        }

        return query;
    }

    private static int NormalizePage(int page)
    {
        return page < 1 ? 1 : page;
    }

    private static int NormalizePageSize(int pageSize)
    {
        return pageSize < 1 ? 10 : pageSize;
    }
}
