using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.ViewModels.Progress;
using DuAnTotNghiep.DTOs.Progress;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class ProgressRepository : GenericRepository<StudentProgressSnapshot>, IProgressRepository
    {
        public ProgressRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<StudentProgressSnapshot?> GetDashboardData(int studentId)
        {
            return await _dbSet
                .Where(s => s.StudentId == studentId && s.SkillId == null && s.TopicId == null)
                .OrderByDescending(s => s.SnapshotDate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<LearningHistoryItemViewModel>> GetHistory(int studentId, HistoryFilter filter)
        {
            var query = _context.StudyActivityLogs
                .Include(a => a.Topic)
                    .ThenInclude(t => t!.Skill)
                .Include(a => a.LearningPathNode)
                .Where(a => a.StudentId == studentId);

            if (!string.IsNullOrWhiteSpace(filter.ActivityType))
            {
                query = query.Where(a => a.ActivityType == filter.ActivityType.ToUpper());
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);
            }

            var logs = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .AsNoTracking()
                .ToListAsync();

            return logs.Select(a => new LearningHistoryItemViewModel
            {
                LogId = a.Id,
                ActivityDate = a.CreatedAt.AddHours(7),
                ActivityType = a.ActivityType,
                Title = a.Topic?.Title ?? a.LearningPathNode?.NodeTitle ?? "Hoạt động chung",
                SkillName = a.Topic?.Skill?.SkillName ?? "Khác",
                DurationMinutes = a.DurationMinutes,
                Score = a.Score,
                MetadataJson = a.Metadata
            }).ToList();
        }

        public async Task<StudentProgressSnapshot?> GetSkillProgress(int studentId, int skillId)
        {
            return await _dbSet
                .Include(s => s.Skill)
                .Where(s => s.StudentId == studentId && s.SkillId == skillId && s.TopicId == null)
                .OrderByDescending(s => s.SnapshotDate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<StudentProgressSnapshot?> GetTopicProgress(int studentId, int topicId)
        {
            return await _dbSet
                .Include(s => s.Topic)
                .Where(s => s.StudentId == studentId && s.TopicId == topicId)
                .OrderByDescending(s => s.SnapshotDate)
                .ThenByDescending(s => s.Id)
                .FirstOrDefaultAsync();
        }

        public async Task UpsertSnapshot(StudentProgressSnapshot snapshot)
        {
            var existing = await _dbSet.FirstOrDefaultAsync(s => 
                s.StudentId == snapshot.StudentId && 
                s.SkillId == snapshot.SkillId && 
                s.TopicId == snapshot.TopicId && 
                s.SnapshotDate == snapshot.SnapshotDate);

            if (existing != null)
            {
                existing.ProgressPercent = snapshot.ProgressPercent;
                existing.AverageScore = snapshot.AverageScore;
                existing.CompletedNodes = snapshot.CompletedNodes;
                existing.TotalStudyMinutes = snapshot.TotalStudyMinutes;
                existing.WeakPoints = snapshot.WeakPoints;
                _dbSet.Update(existing);
            }
            else
            {
                await _dbSet.AddAsync(snapshot);
            }
        }
    }
}
