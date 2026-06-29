using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Models.Repositories
{
    public class StudyActivityLogRepository : GenericRepository<StudyActivityLog>, IStudyActivityLogRepository
    {
        public StudyActivityLogRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<StudyActivityLog>> GetRecentActivitiesAsync(int studentId, int count)
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

        public async Task<List<StudyActivityLog>> GetActivitiesForStreakAsync(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalStudyMinutesAsync(int studentId)
        {
            return await _dbSet
                .Where(a => a.StudentId == studentId && a.DurationMinutes.HasValue)
                .SumAsync(a => a.DurationMinutes!.Value);
        }
    }
}
