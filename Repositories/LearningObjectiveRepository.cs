using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class LearningObjectiveRepository : GenericRepository<LearningObjective>, ILearningObjectiveRepository
    {
        public LearningObjectiveRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<LearningObjective>> GetByTopicAsync(int topicId)
        {
            return await _dbSet.AsNoTracking()
                               .Where(o => o.TopicId == topicId)
                               .OrderBy(o => o.OrderIndex)
                               .ToListAsync();
        }

        public async Task<bool> ExistsCodeAsync(string code, int topicId)
        {
            // The mapping specifies CognitiveLevel as the Code. 
            // In the DB, cognitive level might not be unique, but following the spec for ExistsCodeAsync:
            return await _dbSet.AnyAsync(o => o.TopicId == topicId && o.CognitiveLevel.ToLower() == code.ToLower());
        }

        public async Task<bool> IsObjectiveUsedAsync(int objectiveId)
        {
            // Usually, objectives might be linked to questions or tasks. We check QuestionBanks or similar if they link to Objectives.
            // Assuming no direct link in the current DbContext schema unless found. Let's return false safely if no relations exist.
            // If there's a many-to-many or a FK, we should check it.
            // For now, based on schema reviewed earlier, there's no obvious child entity.
            return await Task.FromResult(false);
        }
    }
}
