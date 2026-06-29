using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DuAnTotNghiep.Services
{
    public class TaxonomyService : ITaxonomyService
    {
        private readonly ApplicationDbContext _db;

        public TaxonomyService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<SkillDto>> GetActiveSkillsAsync(CancellationToken cancellationToken = default)
        {
            // Assuming EnglishSkill entity and a way to determine "active"
            // If no is_active field, return all or filter based on business logic
            var skills = await _db.EnglishSkills
                .Select(s => new SkillDto { Id = s.Id, Name = s.SkillName })
                .ToListAsync(cancellationToken);
            return skills;
        }

        public async Task<IEnumerable<TopicDto>> GetActiveTopicsBySkillAsync(int skillId, CancellationToken cancellationToken = default)
        {
            var topics = await _db.LearningTopics
                .Where(t => t.SkillId == skillId && t.Status != "ARCHIVED")
                .Select(t => new TopicDto
                {
                    Id = t.Id,
                    Name = t.Title,
                    SkillId = t.SkillId,
                    IsActive = true,
                    IsArchived = t.Status == "ARCHIVED"
                })
                .ToListAsync(cancellationToken);
            return topics;
        }

        public async Task<IEnumerable<ProficiencyLevelDto>> GetProficiencyLevelsAsync(CancellationToken cancellationToken = default)
        {
            var levels = await _db.EnglishProficiencyLevels
                .Select(l => new ProficiencyLevelDto { Id = l.Id, Name = l.Name, Code = l.Code })
                .ToListAsync(cancellationToken);
            return levels;
        }

        public async Task<TaxonomyValidationResult> ValidateTopicBelongsToSkillAsync(int topicId, int skillId, CancellationToken cancellationToken = default)
        {
            var topic = await _db.LearningTopics.FirstOrDefaultAsync(t => t.Id == topicId, cancellationToken);
            if (topic == null) return new TaxonomyValidationResult { IsValid = false, ErrorMessage = "Topic not found." };
            if (topic.Status == "ARCHIVED") return new TaxonomyValidationResult { IsValid = false, ErrorMessage = "Topic is archived." };
            if (topic.SkillId != skillId) return new TaxonomyValidationResult { IsValid = false, ErrorMessage = "Topic does not belong to selected skill." };
            return new TaxonomyValidationResult { IsValid = true };
        }
    }
}
