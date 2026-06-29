using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DuAnTotNghiep.DTOs;

namespace DuAnTotNghiep.Services
{
    /// <summary>
    /// Provides taxonomy data and validation for AI generation forms.
    /// Validation MUST ensure topic is active, belongs to the selected skill, and is not archived.
    /// If validation fails, return a TaxonomyValidationResult with IsValid=false and a clear ErrorMessage.
    /// Implementations should avoid calling AI when validation fails.
    /// </summary>
    public interface ITaxonomyService
    {
        Task<IEnumerable<SkillDto>> GetActiveSkillsAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<TopicDto>> GetActiveTopicsBySkillAsync(int skillId, CancellationToken cancellationToken = default);

        Task<IEnumerable<ProficiencyLevelDto>> GetProficiencyLevelsAsync(CancellationToken cancellationToken = default);

        Task<TaxonomyValidationResult> ValidateTopicBelongsToSkillAsync(int topicId, int skillId, CancellationToken cancellationToken = default);
    }
}
