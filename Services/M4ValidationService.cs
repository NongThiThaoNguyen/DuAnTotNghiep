using DuAnTotNghiep.Data;
using DuAnTotNghiep.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class M4ValidationService : IM4ValidationService
    {
        private readonly ApplicationDbContext _context;

        public M4ValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTopicTitleDuplicateAsync(string title, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(title)) return false;
            
            var query = _context.LearningTopics.AsNoTracking();
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(t => t.Title.ToLower() == title.ToLower());
        }

        public async Task<bool> IsSkillCodeDuplicateAsync(string code, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            
            var query = _context.EnglishSkills.AsNoTracking();
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(s => s.SkillCode.ToLower() == code.ToLower());
        }

        public async Task<bool> IsLevelCodeDuplicateAsync(string code, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            
            var query = _context.EnglishProficiencyLevels.AsNoTracking();
            if (excludeId.HasValue)
            {
                query = query.Where(l => l.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(l => l.Code.ToLower() == code.ToLower());
        }

        public async Task<bool> IsParentTopicValidAsync(int? parentTopicId)
        {
            if (!parentTopicId.HasValue) return true; // Null is valid (Root topic)
            return await _context.LearningTopics.AnyAsync(t => t.Id == parentTopicId.Value);
        }

        public async Task<bool> IsTopicExistsAsync(int topicId)
        {
            return await _context.LearningTopics.AnyAsync(t => t.Id == topicId);
        }
    }
}
