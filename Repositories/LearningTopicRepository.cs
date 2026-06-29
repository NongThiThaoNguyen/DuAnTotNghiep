using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class LearningTopicRepository : GenericRepository<LearningTopic>, ILearningTopicRepository
    {
        public LearningTopicRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<LearningTopic>> GetAllTopicsWithDetailsAsync()
        {
            return await _dbSet
                .Include(t => t.TopicPrerequisites)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<LearningTopic?> GetTopicWithPrerequisitesAsync(int id)
        {
            return await _dbSet
                .Include(t => t.TopicPrerequisites)
                .Include(t => t.Skill)
                .Include(t => t.Level)
                .Include(t => t.ParentTopic)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> HasCircularParentAsync(int topicId, int? parentId)
        {
            if (!parentId.HasValue) return false;
            if (topicId == parentId.Value) return true;

            var currentParentId = parentId;
            var maxDepth = 100; // prevent infinite loops
            var depth = 0;

            while (currentParentId.HasValue && depth < maxDepth)
            {
                var parentTopic = await _dbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Id == currentParentId.Value);
                if (parentTopic == null) break;
                
                if (parentTopic.ParentTopicId == topicId)
                {
                    return true;
                }

                currentParentId = parentTopic.ParentTopicId;
                depth++;
            }

            return false;
        }

        public async Task<List<LearningTopic>> SearchTopicsAsync(string? keyword, int? skillId, int? levelId)
        {
            var query = _dbSet.AsNoTracking()
                              .Include(t => t.Skill)
                              .Include(t => t.Level)
                              .Include(t => t.ParentTopic)
                              .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(t => t.Title.ToLower().Contains(lowerKeyword) || 
                                         (t.TopicCode != null && t.TopicCode.ToLower().Contains(lowerKeyword)));
            }

            if (skillId.HasValue)
            {
                query = query.Where(t => t.SkillId == skillId.Value);
            }

            if (levelId.HasValue)
            {
                query = query.Where(t => t.LevelId == levelId.Value);
            }

            return await query.OrderBy(t => t.OrderIndex).ToListAsync();
        }

        public async Task<List<LearningTopic>> GetTopicTreeAsync()
        {
            // Load all with includes in one query, EF Core will wire up the navigation properties
            return await _dbSet.AsNoTracking()
                               .Include(t => t.Skill)
                               .Include(t => t.Level)
                               .Include(t => t.ParentTopic)
                               .OrderBy(t => t.OrderIndex)
                               .ToListAsync();
        }

        public async Task<List<LearningTopic>> GetBySkillAsync(int skillId)
        {
            return await _dbSet.AsNoTracking()
                               .Where(t => t.SkillId == skillId)
                               .OrderBy(t => t.OrderIndex)
                               .ToListAsync();
        }

        public async Task<List<LearningTopic>> GetByLevelAsync(int levelId)
        {
            return await _dbSet.AsNoTracking()
                               .Where(t => t.LevelId == levelId)
                               .OrderBy(t => t.OrderIndex)
                               .ToListAsync();
        }

        public async Task<bool> ExistsNameAsync(string name, int skillId)
        {
            return await _dbSet.AnyAsync(t => t.Title.ToLower() == name.ToLower() && t.SkillId == skillId);
        }

        public async Task<bool> IsTopicUsedAsync(int topicId)
        {
            return await _context.LearningObjectives.AnyAsync(o => o.TopicId == topicId) ||
                   await _context.LearningPathNodes.AnyAsync(n => n.TopicId == topicId) ||
                   await _context.QuestionBanks.AnyAsync(q => q.TopicId == topicId) ||
                   await _context.PracticeTasks.AnyAsync(p => p.TopicId == topicId) ||
                   await _context.OriginalLessons.AnyAsync(l => l.TopicId == topicId) ||
                   await _context.Quizzes.AnyAsync(q => q.TopicId == topicId);
        }

        public async Task AddRangeAsync(IEnumerable<LearningTopic> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public Task UpdateRangeAsync(IEnumerable<LearningTopic> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

    }
}
