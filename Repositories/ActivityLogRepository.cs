using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.DTOs.Progress;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class ActivityLogRepository : GenericRepository<StudyActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task AddActivityLog(StudyActivityLog log)
        {
            await _dbSet.AddAsync(log);
        }

        public async Task AddActivityLog(ActivityLogCreateDto dto)
        {
            var log = new StudyActivityLog
            {
                StudentId = dto.StudentId,
                ActivityType = dto.ActivityType,
                TopicId = dto.TopicId,
                LearningPathNodeId = dto.LearningPathNodeId,
                DurationMinutes = dto.DurationMinutes,
                Score = dto.Score,
                Metadata = dto.Metadata,
                CreatedAt = System.DateTime.UtcNow
            };
            await _dbSet.AddAsync(log);
        }

        public async Task<List<StudyActivityLog>> GetRecentActivities(int studentId, int count)
        {
            return await _dbSet
                .Include(a => a.Topic)
                .Include(a => a.LearningPathNode)
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<StudyActivityLog>> GetActivitiesForStreak(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalStudyMinutes(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId && a.DurationMinutes.HasValue)
                .SumAsync(a => a.DurationMinutes!.Value);
        }
    }
}
