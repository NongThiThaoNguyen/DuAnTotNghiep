using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;

namespace DuAnTotNghiep.Services
{
    public class ReplanningRuleService : IReplanningRuleService
    {
        private readonly ApplicationDbContext _context;

        public ReplanningRuleService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves the currently active replanning rule from the database.
        /// Falls back to a default rule if none is configured.
        /// </summary>
        public async Task<ReplanningRule> GetActiveRuleAsync()
        {
            var activeRule = await _context.ReplanningRules
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            if (activeRule == null)
            {
                // Fallback to default rule configuration to ensure safety
                activeRule = new ReplanningRule
                {
                    LowScoreThreshold = 60m,
                    MissedDaysThreshold = 3,
                    FastProgressScoreThreshold = 85m,
                    AutoApplyEnabled = false,
                    SuggestionExpiryDays = 7,
                    DebounceHours = 24,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
            }

            return activeRule;
        }

        /// <summary>
        /// Checks if a new replanning event should be triggered based on:
        /// 1. Absence of any current pending suggestion of this trigger type.
        /// 2. Debounce time check (most recent event of this type is older than DebounceHours).
        /// </summary>
        public async Task<bool> ShouldTriggerAsync(int studentId, TriggerType type)
        {
            // Rule 1: Do not trigger if there's already a pending suggestion
            if (await HasPendingSuggestionAsync(studentId, type))
            {
                return false;
            }

            // Rule 2: Enforce debounce window
            var rule = await GetActiveRuleAsync();
            int debounceHours = rule?.DebounceHours ?? 24;

            var lastEvent = await _context.AiReplanningEvents
                .Where(e => e.StudentId == studentId && e.TriggerType == type.ToString())
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastEvent != null)
            {
                var timeSinceLastEvent = DateTime.UtcNow - lastEvent.CreatedAt;
                if (timeSinceLastEvent.TotalHours < debounceHours)
                {
                    return false; // Debounce active (too soon to trigger again)
                }
            }

            return true; // Debounce window has passed, safe to trigger
        }

        /// <summary>
        /// Checks if there is currently a pending (SUGGESTED) replanning event of the specified trigger type.
        /// </summary>
        public async Task<bool> HasPendingSuggestionAsync(int studentId, TriggerType type)
        {
            return await _context.AiReplanningEvents
                .AnyAsync(e => e.StudentId == studentId
                            && e.TriggerType == type.ToString()
                            && e.Status == ReplanningStatus.SUGGESTED.ToString());
        }
    }
}
