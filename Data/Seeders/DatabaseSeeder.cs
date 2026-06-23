using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Repositories.Interfaces;

namespace DuAnTotNghiep.Data.Seeders
{
    public class DatabaseSeeder
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGenericRepository<LearningGoal> _goalRepository;
        private readonly IGenericRepository<EnglishProficiencyLevel> _levelRepository;
        private readonly IGenericRepository<EnglishSkill> _skillRepository;
        private readonly IGenericRepository<StudentLearningProfile> _profileRepository;

        public DatabaseSeeder(
            IRoleRepository roleRepository, 
            IUserRepository userRepository,
            IGenericRepository<LearningGoal> goalRepository,
            IGenericRepository<EnglishProficiencyLevel> levelRepository,
            IGenericRepository<EnglishSkill> skillRepository,
            IGenericRepository<StudentLearningProfile> profileRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _goalRepository = goalRepository;
            _levelRepository = levelRepository;
            _skillRepository = skillRepository;
            _profileRepository = profileRepository;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedGoalsAsync();
            await SeedLevelsAsync();
            await SeedSkillsAsync();
            await SeedDemoProfilesAsync();
        }

        private async Task SeedRolesAsync()
        {
            var rolesToSeed = new List<Role>
            {
                new Role { RoleCode = "ADMIN", RoleName = "Quản trị viên", Description = "Quản trị viên toàn hệ thống", CreatedAt = DateTime.UtcNow },
                new Role { RoleCode = "TEACHER", RoleName = "Giáo viên", Description = "Giáo viên giảng dạy", CreatedAt = DateTime.UtcNow },
                new Role { RoleCode = "STUDENT", RoleName = "Học sinh", Description = "Học sinh tham gia hệ thống", CreatedAt = DateTime.UtcNow }
            };

            bool changesMade = false;

            foreach (var role in rolesToSeed)
            {
                if (!await _roleRepository.ExistsAsync(r => r.RoleCode == role.RoleCode))
                {
                    await _roleRepository.AddAsync(role);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _roleRepository.SaveChangesAsync();
            }
        }

        private async Task SeedUsersAsync()
        {
            var adminRole = await _roleRepository.GetByCodeAsync("ADMIN");
            var teacherRole = await _roleRepository.GetByCodeAsync("TEACHER");
            var studentRole = await _roleRepository.GetByCodeAsync("STUDENT");

            if (adminRole == null || teacherRole == null || studentRole == null) return;

            var defaultPassword = PasswordHelper.HashPassword("Password@123");

            var usersToSeed = new List<User>
            {
                // Admin Account
                new User { Email = "admin@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "System Administrator", RoleId = adminRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                
                // Teacher Account
                new User { Email = "teacher@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "English Teacher", RoleId = teacherRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                
                // Student Accounts
                new User { Email = "student1@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Student One", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                new User { Email = "student2@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Student Two", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },
                
                // Locked Account (dùng để test Lockout / Invalidation)
                new User { Email = "lockeduser@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Locked User", RoleId = studentRole.Id, Status = "LOCKED", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0, LockoutUntil = DateTime.UtcNow.AddYears(100) },
                
                // Test OTP User
                new User { Email = "testotp@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Test OTP User", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 }
            };

            bool changesMade = false;

            foreach (var user in usersToSeed)
            {
                if (!await _userRepository.ExistsByEmailAsync(user.Email))
                {
                    await _userRepository.AddAsync(user);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _userRepository.SaveChangesAsync();
            }
        }

        private async Task SeedGoalsAsync()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { GoalCode = "COMMUNICATION", GoalName = "Cải thiện giao tiếp", Description = "Tập trung từ vựng, nghe, nói và phản xạ giao tiếp", IsActive = true },
                new LearningGoal { GoalCode = "IELTS", GoalName = "Luyện thi IELTS", Description = "Tập trung Reading, Listening, Writing, Speaking theo mốc điểm", IsActive = true },
                new LearningGoal { GoalCode = "SCHOOL", GoalName = "Học tốt Tiếng Anh trên lớp", Description = "Củng cố ngữ pháp, từ vựng, đọc hiểu và bài kiểm tra áp dụng", IsActive = true },
                new LearningGoal { GoalCode = "VOCABULARY", GoalName = "Mở rộng từ vựng", Description = "Tập trung ghi nhớ và sử dụng từ vựng theo chủ đề", IsActive = true },
                new LearningGoal { GoalCode = "GRAMMAR", GoalName = "Củng cố ngữ pháp", Description = "Tập trung cấu trúc câu và bài tập ngữ pháp", IsActive = true },
                new LearningGoal { GoalCode = "TOEIC", GoalName = "Luyện thi TOEIC", Description = "Mục tiêu đạt điểm cao trong kỳ thi TOEIC", IsActive = true }
            };

            bool changesMade = false;
            var existingGoals = await _goalRepository.GetAllAsync();

            foreach (var goal in goals)
            {
                var existing = existingGoals.FirstOrDefault(g => g.GoalCode == goal.GoalCode || g.GoalCode == "GENERAL_ENGLISH" && goal.GoalCode == "SCHOOL" || g.GoalCode == "BUSINESS" && goal.GoalCode == "VOCABULARY" || g.GoalCode == "STUDY_ABROAD" && goal.GoalCode == "GRAMMAR");
                
                if (existing == null)
                {
                    // Match by index or just add
                    existing = existingGoals.FirstOrDefault(g => g.GoalName.Contains("Cáº£i") && goal.GoalCode == "COMMUNICATION" 
                    || g.GoalName.Contains("Luyá»‡n") && goal.GoalCode == "IELTS"
                    || g.GoalName.Contains("Há»c") && goal.GoalCode == "SCHOOL"
                    || g.GoalName.Contains("Má»Ÿ") && goal.GoalCode == "VOCABULARY"
                    || g.GoalName.Contains("Cá»§ng") && goal.GoalCode == "GRAMMAR");
                }

                if (existing != null)
                {
                    // Update to fix corrupted text
                    existing.GoalCode = goal.GoalCode;
                    existing.GoalName = goal.GoalName;
                    existing.Description = goal.Description;
                    existing.IsActive = true;
                    _goalRepository.Update(existing);
                    changesMade = true;
                }
                else
                {
                    await _goalRepository.AddAsync(goal);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _goalRepository.SaveChangesAsync();
            }
        }

        private async Task SeedLevelsAsync()
        {
            var levels = new List<EnglishProficiencyLevel>
            {
                new EnglishProficiencyLevel { Code = "BEGINNER", Name = "Beginner", OrderIndex = 1, Description = "Người mới bắt đầu" },
                new EnglishProficiencyLevel { Code = "ELEMENTARY", Name = "Elementary", OrderIndex = 2, Description = "Sơ cấp" },
                new EnglishProficiencyLevel { Code = "PRE_INTERMEDIATE", Name = "Pre Intermediate", OrderIndex = 3, Description = "Tiền trung cấp" },
                new EnglishProficiencyLevel { Code = "INTERMEDIATE", Name = "Intermediate", OrderIndex = 4, Description = "Trung cấp" },
                new EnglishProficiencyLevel { Code = "UPPER_INTERMEDIATE", Name = "Upper Intermediate", OrderIndex = 5, Description = "Trung cấp trên" },
                new EnglishProficiencyLevel { Code = "ADVANCED", Name = "Advanced", OrderIndex = 6, Description = "Cao cấp" }
            };

            bool changesMade = false;
            foreach (var level in levels)
            {
                if (!await _levelRepository.ExistsAsync(l => l.Code == level.Code))
                {
                    await _levelRepository.AddAsync(level);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _levelRepository.SaveChangesAsync();
            }
        }

        private async Task SeedSkillsAsync()
        {
            var skills = new List<EnglishSkill>
            {
                new EnglishSkill { SkillCode = "LISTENING", SkillName = "Listening", OrderIndex = 1, Description = "Kỹ năng nghe", IsActive = true },
                new EnglishSkill { SkillCode = "SPEAKING", SkillName = "Speaking", OrderIndex = 2, Description = "Kỹ năng nói", IsActive = true },
                new EnglishSkill { SkillCode = "READING", SkillName = "Reading", OrderIndex = 3, Description = "Kỹ năng đọc", IsActive = true },
                new EnglishSkill { SkillCode = "WRITING", SkillName = "Writing", OrderIndex = 4, Description = "Kỹ năng viết", IsActive = true },
                new EnglishSkill { SkillCode = "GRAMMAR", SkillName = "Grammar", OrderIndex = 5, Description = "Ngữ pháp", IsActive = true },
                new EnglishSkill { SkillCode = "VOCABULARY", SkillName = "Vocabulary", OrderIndex = 6, Description = "Từ vựng", IsActive = true }
            };

            bool changesMade = false;
            foreach (var skill in skills)
            {
                if (!await _skillRepository.ExistsAsync(s => s.SkillCode == skill.SkillCode))
                {
                    await _skillRepository.AddAsync(skill);
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                await _skillRepository.SaveChangesAsync();
            }
        }

        private async Task SeedDemoProfilesAsync()
        {
            var student1 = await _userRepository.GetByEmailAsync("student1@aistudyenglish.com");
            var ieltsGoal = (await _goalRepository.GetAllAsync()).FirstOrDefault(g => g.GoalCode == "IELTS");
            var intermediateLevel = (await _levelRepository.GetAllAsync()).FirstOrDefault(l => l.Code == "INTERMEDIATE");

            if (student1 != null && ieltsGoal != null && intermediateLevel != null)
            {
                if (!await _profileRepository.ExistsAsync(p => p.UserId == student1.Id))
                {
                    await _profileRepository.AddAsync(new StudentLearningProfile
                    {
                        UserId = student1.Id,
                        MainGoalId = ieltsGoal.Id,
                        CurrentLevelId = intermediateLevel.Id,
                        TargetScore = 6.5m,
                        OnboardingStatus = "COMPLETED",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            var student2 = await _userRepository.GetByEmailAsync("student2@aistudyenglish.com");
            var commGoal = (await _goalRepository.GetAllAsync()).FirstOrDefault(g => g.GoalCode == "COMMUNICATION");
            var beginnerLevel = (await _levelRepository.GetAllAsync()).FirstOrDefault(l => l.Code == "BEGINNER");

            if (student2 != null && commGoal != null && beginnerLevel != null)
            {
                if (!await _profileRepository.ExistsAsync(p => p.UserId == student2.Id))
                {
                    await _profileRepository.AddAsync(new StudentLearningProfile
                    {
                        UserId = student2.Id,
                        MainGoalId = commGoal.Id,
                        CurrentLevelId = beginnerLevel.Id,
                        LearningNote = "Fluent Communication",
                        OnboardingStatus = "COMPLETED",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _profileRepository.SaveChangesAsync();
        }
    }
}
