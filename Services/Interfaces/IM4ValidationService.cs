using System.Threading.Tasks;

namespace DuAnTotNghiep.Services.Interfaces
{
    public interface IM4ValidationService
    {
        Task<bool> IsTopicTitleDuplicateAsync(string title, int? excludeId = null);
        Task<bool> IsSkillCodeDuplicateAsync(string code, int? excludeId = null);
        Task<bool> IsLevelCodeDuplicateAsync(string code, int? excludeId = null);
        Task<bool> IsParentTopicValidAsync(int? parentTopicId);
        Task<bool> IsTopicExistsAsync(int topicId);
    }
}
