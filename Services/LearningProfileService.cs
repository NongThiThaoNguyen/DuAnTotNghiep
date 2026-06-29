using DuAnTotNghiep.Models.DTOs.Onboarding;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Services.Interfaces;
using DuAnTotNghiep.Models.ViewModels.Onboarding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DuAnTotNghiep.Services
{
    public class LearningProfileService : ILearningProfileService
    {
        private readonly ILearningProfileRepository _profileRepository;
        private readonly IGenericRepository<LearningGoal> _goalRepository;
        private readonly IGenericRepository<EnglishProficiencyLevel> _levelRepository;
        private readonly IGenericRepository<EnglishSkill> _skillRepository;
        private readonly IAuditLogRepository _auditRepository;
        private readonly IGenericRepository<StudentLearningPath> _learningPathRepository;
        private readonly IGenericRepository<AiReplanningEvent> _replanningEventRepository;

        public LearningProfileService(
            ILearningProfileRepository profileRepository,
            IGenericRepository<LearningGoal> goalRepository,
            IGenericRepository<EnglishProficiencyLevel> levelRepository,
            IGenericRepository<EnglishSkill> skillRepository,
            IAuditLogRepository auditRepository,
            IGenericRepository<StudentLearningPath> learningPathRepository,
            IGenericRepository<AiReplanningEvent> replanningEventRepository)
        {
            _profileRepository = profileRepository;
            _goalRepository = goalRepository;
            _levelRepository = levelRepository;
            _skillRepository = skillRepository;
            _auditRepository = auditRepository;
            _learningPathRepository = learningPathRepository;
            _replanningEventRepository = replanningEventRepository;
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
                    
                    if (profile.MainGoalId.HasValue)
                    {
                        var goal = await _goalRepository.GetByIdAsync(profile.MainGoalId.Value);
                        if (goal != null)
                        {
                            var code = goal.GoalCode;
                            if (code == "IELTS" && step2.TargetScore.HasValue && (step2.TargetScore < 0 || step2.TargetScore > 9)) return false;
                            if (code == "TOEIC" && step2.TargetScore.HasValue && (step2.TargetScore < 0 || step2.TargetScore > 990)) return false;
                        }
                    }

                    profile.CurrentLevelId = step2.CurrentLevelId;
                    profile.TargetLevelId = step2.TargetLevelId;
                    profile.TargetScore = step2.TargetScore;
                    break;

                case OnboardingStep3ViewModel step3:
                    var activeSkills = (await _skillRepository.GetAllAsync()).Where(s => s.IsActive == true).Select(s => s.SkillCode).ToList();
                    step3.SelectedSkillCodes = step3.SelectedSkillCodes.Distinct().ToList();
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

            try
            {
                if (isNew)
                {
                    await _auditRepository.AddAsync(new AuditLog
                    {
                        UserId = userId,
                        Action = "ONBOARDING_STARTED",
                        EntityName = "StudentLearningProfile",
                        EntityId = profile.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                string stepName = stepViewModel switch
                {
                    OnboardingStep1ViewModel => "Step: Goal",
                    OnboardingStep2ViewModel => "Step: Level",
                    OnboardingStep3ViewModel => "Step: Skills",
                    OnboardingStep4ViewModel => "Step: Study Time",
                    _ => "Step: Unknown"
                };

                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "ONBOARDING_STEP_SAVED",
                    EntityName = "StudentLearningProfile",
                    EntityId = profile.Id,
                    NewValue = stepName,
                    CreatedAt = DateTime.UtcNow
                });
                await _auditRepository.SaveChangesAsync();
            }
            catch { /* Ignore audit log failure */ }

            return true;
        }

        public async Task<bool> UpdateFullProfileAsync(int userId, UpdateLearningProfileViewModel model)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return false;

            if (!await _goalRepository.ExistsAsync(g => g.Id == model.MainGoalId && g.IsActive == true)) return false;
            if (model.CurrentLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == model.CurrentLevelId)) return false;
            if (model.TargetLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == model.TargetLevelId)) return false;

            if (!model.MainGoalId.HasValue) return false;
            var goal = await _goalRepository.GetByIdAsync(model.MainGoalId.Value);
            if (goal != null)
            {
                var code = goal.GoalCode;
                if (code == "IELTS" && model.TargetScore.HasValue && (model.TargetScore < 0 || model.TargetScore > 9)) return false;
                if (code == "TOEIC" && model.TargetScore.HasValue && (model.TargetScore < 0 || model.TargetScore > 990)) return false;
            }

            var activeSkills = (await _skillRepository.GetAllAsync()).Where(s => s.IsActive == true).Select(s => s.SkillCode).ToList();
            model.SelectedSkillCodes = model.SelectedSkillCodes.Distinct().ToList();
            if (model.SelectedSkillCodes.Any(code => !activeSkills.Contains(code))) return false;

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

            try
            {
                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "LEARNING_PROFILE_UPDATED",
                    EntityName = "StudentLearningProfile",
                    EntityId = profile.Id,
                    CreatedAt = DateTime.UtcNow
                });
                await _auditRepository.SaveChangesAsync();
            }
            catch { /* Ignore audit log failure */ }

            return true;
        }

        public async Task<bool> EditLearningProfileAsync(int userId, UpdateLearningProfileViewModel model)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return false;

            if (!await _goalRepository.ExistsAsync(g => g.Id == model.MainGoalId && g.IsActive == true)) return false;
            if (model.CurrentLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == model.CurrentLevelId)) return false;
            if (model.TargetLevelId.HasValue && !await _levelRepository.ExistsAsync(l => l.Id == model.TargetLevelId)) return false;

            if (!model.MainGoalId.HasValue) return false;
            var goal = await _goalRepository.GetByIdAsync(model.MainGoalId.Value);
            if (goal != null)
            {
                var code = goal.GoalCode;
                if (code == "IELTS" && model.TargetScore.HasValue && (model.TargetScore < 0 || model.TargetScore > 9)) return false;
                if (code == "TOEIC" && model.TargetScore.HasValue && (model.TargetScore < 0 || model.TargetScore > 990)) return false;
            }

            var activeSkills = (await _skillRepository.GetAllAsync()).Where(s => s.IsActive == true).Select(s => s.SkillCode).ToList();
            model.SelectedSkillCodes = model.SelectedSkillCodes.Distinct().ToList();
            if (model.SelectedSkillCodes.Any(code => !activeSkills.Contains(code))) return false;

            bool isMajorChange = profile.MainGoalId != model.MainGoalId;
            string oldGoalId = profile.MainGoalId?.ToString() ?? "None";
            string newGoalId = model.MainGoalId?.ToString() ?? "None";

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
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

                // Generate AiReplanningEvent if Major Change
                if (isMajorChange)
                {
                    var activePaths = await _learningPathRepository.GetAllAsync();
                    var activePath = activePaths.FirstOrDefault(p => p.StudentId == userId && (p.Status == "ACTIVE" || p.Status == "IN_PROGRESS"));

                    if (activePath != null)
                    {
                        await _replanningEventRepository.AddAsync(new AiReplanningEvent
                        {
                            StudentId = userId,
                            LearningPathId = activePath.Id,
                            TriggerType = "MANUAL_GOAL_UPDATE",
                            Reason = "Người dùng thay đổi mục tiêu học tập (Goal)",
                            Status = "PENDING",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _profileRepository.SaveChangesAsync();
                transactionScope.Complete();
            }
            catch
            {
                return false;
            }

            try
            {
                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "LEARNING_PROFILE_UPDATED",
                    EntityName = "StudentLearningProfile",
                    EntityId = profile.Id,
                    OldValue = isMajorChange ? $"Goal: {oldGoalId}" : "Minor update",
                    NewValue = isMajorChange ? $"Goal: {newGoalId}" : "Minor update",
                    CreatedAt = DateTime.UtcNow
                });
                await _auditRepository.SaveChangesAsync();
            }
            catch { /* Ignore audit log failure */ }

            return true;
        }

        public async Task<bool> MarkOnboardingCompletedAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return false;

            // Chống trùng lặp (Concurrency/Duplicate Submit)
            if (profile.OnboardingStatus == "COMPLETED") return true;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    profile.OnboardingStatus = "COMPLETED";
                    profile.UpdatedAt = DateTime.UtcNow;

                    _profileRepository.Update(profile);
                    await _profileRepository.SaveChangesAsync();

                    transactionScope.Complete();
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "ONBOARDING_COMPLETED",
                    EntityName = "StudentLearningProfile",
                    EntityId = profile.Id,
                    CreatedAt = DateTime.UtcNow
                });
                await _auditRepository.SaveChangesAsync();
            }
            catch { /* Ignore audit log failure */ }

            return true;
        }
        public async Task<bool> ValidateProfileActiveDataAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null) return false;

            // Validate Goal
            if (profile.MainGoal != null && profile.MainGoal.IsActive == false) return false;

            // Validate Skills
            var activeSkills = await GetActiveSkillsAsync();
            var activeSkillCodes = activeSkills.Select(s => s.SkillCode).ToList();

            foreach (var pref in profile.StudentSkillPreferences)
            {
                if (!activeSkillCodes.Contains(pref.SkillCode)) return false;
            }

            return true;
        }

        public async Task<bool> ResetOnboardingStatusAsync(int studentId, int adminId, string ipAddress)
        {
            var profile = await _profileRepository.GetByUserIdAsync(studentId);
            if (profile == null) return false;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    profile.OnboardingStatus = "NOT_STARTED";
                    profile.UpdatedAt = DateTime.UtcNow;

                    _profileRepository.Update(profile);
                    await _profileRepository.SaveChangesAsync();

                    transactionScope.Complete();
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = studentId,
                    Action = "ADMIN_RESET_LEARNING_PROFILE",
                    EntityName = "StudentLearningProfile",
                    EntityId = profile.Id,
                    OldValue = "COMPLETED",
                    NewValue = "NOT_STARTED",
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow
                });
                await _auditRepository.SaveChangesAsync();
            }
            catch { /* Ignore audit log failure */ }

            return true;
        }
    }
}
