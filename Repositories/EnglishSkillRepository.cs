using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class EnglishSkillRepository : GenericRepository<EnglishSkill>, IEnglishSkillRepository
    {
        public EnglishSkillRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<EnglishSkill>> GetActiveSkillsAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .OrderBy(s => s.OrderIndex)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsCodeAsync(string code)
        {
            return await _dbSet.AnyAsync(s => s.SkillCode.ToLower() == code.ToLower());
        }

        public async Task<EnglishSkill?> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.SkillCode.ToLower() == code.ToLower());
        }

        public async Task<bool> IsSkillUsedAsync(int skillId)
        {
            return await _context.LearningTopics.AnyAsync(t => t.SkillId == skillId) ||
                   await _context.QuestionBanks.AnyAsync(q => q.SkillId == skillId) ||
                   await _context.PlacementTestSections.AnyAsync(s => s.SkillId == skillId);
        }
    }
}
