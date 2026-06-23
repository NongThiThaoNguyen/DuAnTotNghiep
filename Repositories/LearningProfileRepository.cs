using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Repositories
{
    public class LearningProfileRepository : GenericRepository<StudentLearningProfile>, ILearningProfileRepository
    {
        public LearningProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<StudentLearningProfile?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(p => p.StudentSkillPreferences)
                .Include(p => p.MainGoal)
                .Include(p => p.CurrentLevel)
                .Include(p => p.TargetLevel)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdatePrioritySkillsAsync(int profileId, List<string> skillCodes)
        {
            var oldSkills = await _context.Set<StudentSkillPreference>()
                .Where(s => s.StudentProfileId == profileId)
                .ToListAsync();
            
            _context.Set<StudentSkillPreference>().RemoveRange(oldSkills);

            var newSkills = skillCodes.Select((code, index) => new StudentSkillPreference
            {
                StudentProfileId = profileId,
                SkillCode = code,
                PriorityLevel = index + 1,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.Set<StudentSkillPreference>().AddRangeAsync(newSkills);
        }
    }
}
