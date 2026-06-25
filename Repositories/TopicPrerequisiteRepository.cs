using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class TopicPrerequisiteRepository : GenericRepository<TopicPrerequisite>, ITopicPrerequisiteRepository
    {
        public TopicPrerequisiteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TopicPrerequisite>> GetPrerequisitesByTopicIdAsync(int topicId)
        {
            return await _dbSet
                .Include(tp => tp.PrerequisiteTopic)
                .Where(tp => tp.TopicId == topicId)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(int topicId, int prerequisiteTopicId)
        {
            return await _dbSet.AnyAsync(tp => tp.TopicId == topicId && tp.PrerequisiteTopicId == prerequisiteTopicId);
        }
    }
}
