using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories
{
    public class StudentProgressSnapshotRepository : GenericRepository<StudentProgressSnapshot>, IStudentProgressSnapshotRepository
    {
        public StudentProgressSnapshotRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<StudentProgressSnapshot?> GetLatestOverallSnapshotAsync(int studentId)
        {
            return await _dbSet
                .Where(s => s.StudentId == studentId && s.SkillId == null && s.TopicId == null)
                .OrderByDescending(s => s.SnapshotDate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<StudentProgressSnapshot>> GetLatestSkillSnapshotsAsync(int studentId)
        {
            var latestDates = await _dbSet
                .Where(s => s.StudentId == studentId && s.SkillId != null && s.TopicId == null)
                .GroupBy(s => s.SkillId)
                .Select(g => new { SkillId = g.Key, MaxDate = g.Max(x => x.SnapshotDate) })
                .ToListAsync();

            var latestSnapshots = new List<StudentProgressSnapshot>();
            foreach (var group in latestDates)
            {
                var snapshot = await _dbSet
                    .Include(s => s.Skill)
                    .Where(s => s.StudentId == studentId && s.SkillId == group.SkillId && s.TopicId == null && s.SnapshotDate == group.MaxDate)
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync();
                if (snapshot != null)
                {
                    latestSnapshots.Add(snapshot);
                }
            }

            return latestSnapshots;
        }

        public async Task<List<StudentProgressSnapshot>> GetLatestTopicSnapshotsAsync(int studentId)
        {
            var latestDates = await _dbSet
                .Where(s => s.StudentId == studentId && s.TopicId != null)
                .GroupBy(s => s.TopicId)
                .Select(g => new { TopicId = g.Key, MaxDate = g.Max(x => x.SnapshotDate) })
                .ToListAsync();

            var latestSnapshots = new List<StudentProgressSnapshot>();
            foreach (var group in latestDates)
            {
                var snapshot = await _dbSet
                    .Include(s => s.Topic)
                    .Where(s => s.StudentId == studentId && s.TopicId == group.TopicId && s.SnapshotDate == group.MaxDate)
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync();
                if (snapshot != null)
                {
                    latestSnapshots.Add(snapshot);
                }
            }

            return latestSnapshots;
        }

        public async Task<List<StudentProgressSnapshot>> GetHistoricalSnapshotsAsync(int studentId, DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet
                .Where(s => s.StudentId == studentId && s.SkillId == null && s.TopicId == null && s.SnapshotDate >= startDate && s.SnapshotDate <= endDate)
                .OrderBy(s => s.SnapshotDate)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
