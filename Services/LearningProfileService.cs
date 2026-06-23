using DuAnTotNghiep.DTOs.Onboarding;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.ViewModels.Onboarding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DuAnTotNghiep.Services
{
    public class LearningProfileService : ILearningProfileService
    {
        private readonly ILearningProfileRepository _profileRepository;
        private readonly IGenericRepository<LearningGoal> _goalRepository;
        private readonly IGenericRepository<EnglishProficiencyLevel> _levelRepository;
        private readonly IGenericRepository<EnglishSkill> _skillRepository;
        private readonly IAuditLogRepository _auditRepository;

        public LearningProfileService(
            ILearningProfileRepository profileRepository,
            IGenericRepository<LearningGoal> goalRepository,
            IGenericRepository<EnglishProficiencyLevel> levelRepository,
            IGenericRepository<EnglishSkill> skillRepository,
            IAuditLogRepository auditRepository)
        {
            _profileRepository = profileRepository;
            _goalRepository = goalRepository;
            _levelRepository = levelRepository;
            _skillRepository = skillRepository;
            _auditRepository = auditRepository;
        }

        public async Task<IEnumerable<GoalDto>> GetActiveGoalsAsync()
        {
            var goals = await _goalRepository.GetAllAsync();
            return goals.Where(g => g.IsActive == true).Select(g => g.ToDto()!);
        }

        public async Task<IEnumerable<LevelDto>> GetActiveLevelsAsync()
        {
            var levels = await _levelRepository.GetAllAsync();
            return levels.OrderBy(l => l.OrderIndex).Select(l => l.ToDto()!);
        }

        public async Task<IEnumerable<SkillDto>> GetActiveSkillsAsync()
        {
            var skills = await _skillRepository.GetAllAsync();
            return skills.Where(s => s.IsActive == true).OrderBy(s => s.OrderIndex).Select(s => s.ToDto()!);
        }

        public async Task<LearningProfileDto?> GetProfileByUserIdAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return null;

            var dto = profile.ToDto();
            
            if (dto != null && profile.StudentSkillPreferences.Any())
            {
                var allSkills = await GetActiveSkillsAsync();
                var skillCodes = profile.StudentSkillPreferences.OrderBy(s => s.PriorityLevel).Select(s => s.SkillCode);
                dto.PrioritySkills = allSkills.Where(s => skillCodes.Contains(s.SkillCode)).ToList();
            }

            return dto;
        }

        public async Task<LearningProfileSummaryDto?> GetProfileSummaryAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            return profile?.ToSummaryDto();
        }

        public async Task<bool> SaveStepAsync(int userId, object stepViewModel)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            bool isNew = false;

            if (profile == null)
            {
                profile = new StudentLearningProfile
                {
                    UserId = userId,
                    OnboardingStatus = "IN_PROGRESS",
                    CreatedAt = DateTime.UtcNow
                };
                isNew = true;
            }
            else if (profile.OnboardingStatus == "NOT_STARTED")
            {
                profile.OnboardingStatus = "IN_PROGRESS";
            }

            profile.UpdatedAt = DateTime.UtcNow;

            switch (stepViewModel)
            {
                case OnboardingStep1ViewModel step1:
                    if (!await _goalRepository.ExistsAsync(g => g.Id == step1.MainGoalId && g.IsActive == true)) return false;
                    profile.MainGoalId = step1.MainGoalId;
                    break;

                case OnboardingStep2ViewModel step2:
                    if (step2.CurrentLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == step2.CurrentLevelId)) return false;
                    if (step2.TargetLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == step2.TargetLevelId)) return false;
                    profile.CurrentLevelId = step2.CurrentLevelId;
                    profile.TargetLevelId = step2.TargetLevelId;
                    profile.TargetScore = step2.TargetScore;
                    break;

                case OnboardingStep3ViewModel step3:
                    var activeSkills = (await _skillRepository.GetAllAsync()).Where(s => s.IsActive == true).Select(s => s.SkillCode).ToList();
                    if (step3.SelectedSkillCodes.Any(code => !activeSkills.Contains(code))) return false;
                    // Priority skills updated below
                    break;

                case OnboardingStep4ViewModel step4:
                    profile.DailyStudyMinutes = step4.DailyStudyMinutes;
                    profile.WeeklyStudyDays = step4.WeeklyStudyDays;
                    profile.PreferredStudyTime = step4.PreferredStudyTime;
                    break;



                default:
                    return false;
            }

            if (isNew)
            {
                await _profileRepository.AddAsync(profile);
            }
            else
            {
                _profileRepository.Update(profile);
            }

            await _profileRepository.SaveChangesAsync();

            if (stepViewModel is OnboardingStep3ViewModel step3Model)
            {
                await _profileRepository.UpdatePrioritySkillsAsync(profile.Id, step3Model.SelectedSkillCodes);
                await _profileRepository.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UpdateFullProfileAsync(int userId, UpdateLearningProfileViewModel model)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return false;

            if (!await _goalRepository.ExistsAsync(g => g.Id == model.MainGoalId)) return false;
            if (!await _levelRepository.ExistsAsync(l => l.Id == model.CurrentLevelId)) return false;
            if (model.TargetLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == model.TargetLevelId)) return false;

            profile.MainGoalId = model.MainGoalId;
            profile.CurrentLevelId = model.CurrentLevelId;
            profile.DailyStudyMinutes = model.DailyStudyMinutes;
            profile.WeeklyStudyDays = model.WeeklyStudyDays;
            profile.PreferredStudyTime = model.PreferredStudyTime;
            profile.TargetScore = model.TargetScore;
            profile.TargetLevelId = model.TargetLevelId;
            profile.LearningNote = model.LearningNote;
            profile.UpdatedAt = DateTime.UtcNow;

            _profileRepository.Update(profile);
            await _profileRepository.UpdatePrioritySkillsAsync(profile.Id, model.SelectedSkillCodes);
            await _profileRepository.SaveChangesAsync();

            await _auditRepository.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "UPDATE",
                EntityName = "StudentLearningProfile",
                EntityId = profile.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _auditRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkOnboardingCompletedAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return false;

            profile.OnboardingStatus = "COMPLETED";
            profile.UpdatedAt = DateTime.UtcNow;

            _profileRepository.Update(profile);
            await _profileRepository.SaveChangesAsync();

            await _auditRepository.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = "COMPLETE_ONBOARDING",
                EntityName = "StudentLearningProfile",
                EntityId = profile.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _auditRepository.SaveChangesAsync();

            return true;
        }
    }
}
