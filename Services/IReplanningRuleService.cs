using System.Threading.Tasks;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;

namespace DuAnTotNghiep.Services
{
    public interface IReplanningRuleService
    {
        /// <summary>
        /// Retrieves the currently active replanning rule.
        /// </summary>
        Task<ReplanningRule> GetActiveRuleAsync();

        /// <summary>
        /// Determines if a new replanning event should be triggered for a student based on debounce logic.
        /// </summary>
        Task<bool> ShouldTriggerAsync(int studentId, TriggerType type);

        /// <summary>
        /// Checks if there is any pending replanning suggestion (status = 'SUGGESTED') for a student.
        /// </summary>
        Task<bool> HasPendingSuggestionAsync(int studentId, TriggerType type);
    }
}
