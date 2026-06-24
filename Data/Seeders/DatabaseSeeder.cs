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
        private readonly ApplicationDbContext _context;

        public DatabaseSeeder(
            IRoleRepository roleRepository, 
            IUserRepository userRepository,
            IGenericRepository<LearningGoal> goalRepository,
            IGenericRepository<EnglishProficiencyLevel> levelRepository,
            IGenericRepository<EnglishSkill> skillRepository,
            IGenericRepository<StudentLearningProfile> profileRepository,
            ApplicationDbContext context)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _goalRepository = goalRepository;
            _levelRepository = levelRepository;
            _skillRepository = skillRepository;
            _profileRepository = profileRepository;
            _context = context;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
            await SeedGoalsAsync();
            await SeedLevelsAsync();
            await SeedSkillsAsync();
            await SeedPlacementTestsAsync();
            await SeedPlacementTestDemoAsync();
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
                new User { Email = "testotp@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Test OTP User", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 },

                // Demo User for Task 20
                new User { Email = "student.demo@aistudyenglish.com", PasswordHash = defaultPassword, FullName = "Student Demo", RoleId = studentRole.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow, FailedLoginCount = 0 }
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
                var existing = existingGoals.FirstOrDefault(g => g.GoalCode == goal.GoalCode);
                
                if (existing == null)
                {
                    existing = existingGoals.FirstOrDefault(g => 
                        (g.GoalCode == "SCHOOL_ENGLISH" && goal.GoalCode == "SCHOOL") ||
                        (g.GoalCode == "GENERAL_ENGLISH" && goal.GoalCode == "SCHOOL") ||
                        (g.GoalCode == "BUSINESS" && goal.GoalCode == "VOCABULARY") ||
                        (g.GoalCode == "STUDY_ABROAD" && goal.GoalCode == "GRAMMAR"));
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

            // Cleanup orphaned garbled goal
            var orphanedGoal = existingGoals.FirstOrDefault(g => g.GoalCode == "SCHOOL_ENGLISH" || g.GoalCode == "GENERAL_ENGLISH");
            if (orphanedGoal != null)
            {
                try
                {
                    _goalRepository.Delete(orphanedGoal);
                    await _goalRepository.SaveChangesAsync();
                }
                catch { } // Ignore if referenced by foreign keys
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
                new EnglishProficiencyLevel { Code = "ADVANCED", Name = "Advanced", OrderIndex = 6, Description = "Cao cấp" },
                new EnglishProficiencyLevel { Code = "PROFICIENT", Name = "Proficient", OrderIndex = 7, Description = "Sử dụng tiếng Anh linh hoạt, thành thạo" }
            };

            bool changesMade = false;
            var existingLevels = await _levelRepository.GetAllAsync();

            foreach (var level in levels)
            {
                var existing = existingLevels.FirstOrDefault(l => 
                    l.Code == level.Code ||
                    (l.Code == "A1" && level.Code == "BEGINNER") ||
                    (l.Code == "A2" && level.Code == "ELEMENTARY") ||
                    (l.Code == "B1" && level.Code == "PRE_INTERMEDIATE") ||
                    (l.Code == "B2" && level.Code == "INTERMEDIATE") ||
                    (l.Code == "C1" && level.Code == "UPPER_INTERMEDIATE") ||
                    (l.Code == "C2" && level.Code == "ADVANCED"));
                    
                if (existing != null)
                {
                    existing.Name = level.Name;
                    existing.Description = level.Description;
                    existing.OrderIndex = level.OrderIndex;
                    _levelRepository.Update(existing);
                    changesMade = true;
                }
                else
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
                new EnglishSkill { SkillCode = "LISTENING", SkillName = "Nghe hiểu", OrderIndex = 1, Description = "Kỹ năng nghe hiểu", IsActive = true },
                new EnglishSkill { SkillCode = "SPEAKING", SkillName = "Nói", OrderIndex = 2, Description = "Kỹ năng nói", IsActive = true },
                new EnglishSkill { SkillCode = "READING", SkillName = "Đọc hiểu", OrderIndex = 3, Description = "Kỹ năng đọc hiểu", IsActive = true },
                new EnglishSkill { SkillCode = "WRITING", SkillName = "Viết", OrderIndex = 4, Description = "Kỹ năng viết", IsActive = true },
                new EnglishSkill { SkillCode = "GRAMMAR", SkillName = "Ngữ pháp", OrderIndex = 5, Description = "Ngữ pháp", IsActive = true },
                new EnglishSkill { SkillCode = "VOCABULARY", SkillName = "Từ vựng", OrderIndex = 6, Description = "Từ vựng", IsActive = true },
                new EnglishSkill { SkillCode = "PRONUNCIATION", SkillName = "Phát âm", OrderIndex = 7, Description = "Phát âm", IsActive = true }
            };

            bool changesMade = false;
            var existingSkills = await _skillRepository.GetAllAsync();

            foreach (var skill in skills)
            {
                var existing = existingSkills.FirstOrDefault(s => s.SkillCode == skill.SkillCode);
                
                if (existing == null)
                {
                    existing = existingSkills.FirstOrDefault(s => 
                        (s.SkillCode == "LISTENING" && skill.SkillCode == "LISTENING") ||
                        (s.SkillCode == "SPEAKING" && skill.SkillCode == "SPEAKING") ||
                        (s.SkillCode == "READING" && skill.SkillCode == "READING") ||
                        (s.SkillCode == "WRITING" && skill.SkillCode == "WRITING") ||
                        (s.SkillCode == "GRAMMAR" && skill.SkillCode == "GRAMMAR") ||
                        (s.SkillCode == "VOCABULARY" && skill.SkillCode == "VOCABULARY") ||
                        (s.SkillCode == "PRONUNCIATION" && skill.SkillCode == "PRONUNCIATION")
                    );
                }

                if (existing != null)
                {
                    existing.SkillCode = skill.SkillCode;
                    existing.SkillName = skill.SkillName;
                    existing.Description = skill.Description;
                    existing.OrderIndex = skill.OrderIndex;
                    existing.IsActive = true;
                    _skillRepository.Update(existing);
                    changesMade = true;
                }
                else
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

            var studentDemo = await _userRepository.GetByEmailAsync("student.demo@aistudyenglish.com");
            if (studentDemo != null && ieltsGoal != null && beginnerLevel != null)
            {
                if (!await _profileRepository.ExistsAsync(p => p.UserId == studentDemo.Id))
                {
                    await _profileRepository.AddAsync(new StudentLearningProfile
                    {
                        UserId = studentDemo.Id,
                        MainGoalId = ieltsGoal.Id,
                        CurrentLevelId = beginnerLevel.Id,
                        LearningNote = "Demo Account for Placement Test",
                        OnboardingStatus = "COMPLETED",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _profileRepository.SaveChangesAsync();
        }

        private async Task SeedPlacementTestsAsync()
        {
            if (!_context.PlacementTests.Any())
            {
                var adminUser = await _userRepository.GetByEmailAsync("admin@aistudyenglish.com");
                var beginnerLevel = (await _levelRepository.GetAllAsync()).FirstOrDefault(l => l.Code == "BEGINNER");
                
                if (adminUser != null && beginnerLevel != null)
                {
                    var test = new PlacementTest
                    {
                        Title = "General English Placement Test",
                        Description = "Bài đánh giá năng lực tiếng Anh tổng quát.",
                        TimeLimitMinutes = 30,
                        TargetLevelId = beginnerLevel.Id,
                        CreatedBy = adminUser.Id,
                        Status = Enums.PlacementTestStatus.Published,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.PlacementTests.Add(test);
                    await _context.SaveChangesAsync();
                }
            }
        }
        private async Task SeedPlacementTestDemoAsync()
        {
            if (_context.PlacementTests.Any(t => t.Title == "English Placement Test Demo"))
                return;

            var adminUser = await _userRepository.GetByEmailAsync("admin@aistudyenglish.com");
            var a2Level = _context.EnglishProficiencyLevels.FirstOrDefault(l => l.Code == "ELEMENTARY");
            if (adminUser == null || a2Level == null) return;

            // 1. Create Placement Test
            var test = new PlacementTest
            {
                Title = "English Placement Test Demo",
                Description = "Bài kiểm tra đánh giá năng lực demo (A2 -> B1).",
                TimeLimitMinutes = 30,
                TargetLevelId = a2Level.Id,
                CreatedBy = adminUser.Id,
                Status = "PUBLISHED",
                TotalScore = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.PlacementTests.Add(test);
            await _context.SaveChangesAsync();

            // 2. Create Sections
            var skills = _context.EnglishSkills.ToList();
            var vocabSkill = skills.FirstOrDefault(s => s.SkillCode == "VOCABULARY");
            var grammarSkill = skills.FirstOrDefault(s => s.SkillCode == "GRAMMAR");
            var readingSkill = skills.FirstOrDefault(s => s.SkillCode == "READING");
            var listeningSkill = skills.FirstOrDefault(s => s.SkillCode == "LISTENING");

            var sections = new List<PlacementTestSection>();
            if (vocabSkill != null) sections.Add(new PlacementTestSection { PlacementTestId = test.Id, SkillId = vocabSkill.Id, SectionName = "Vocabulary", Instruction = "Choose the best word to complete the sentence.", OrderIndex = 1 });
            if (grammarSkill != null) sections.Add(new PlacementTestSection { PlacementTestId = test.Id, SkillId = grammarSkill.Id, SectionName = "Grammar", Instruction = "Choose the correct grammar structure.", OrderIndex = 2 });
            if (readingSkill != null) sections.Add(new PlacementTestSection { PlacementTestId = test.Id, SkillId = readingSkill.Id, SectionName = "Reading", Instruction = "Read the passage and answer the questions.", OrderIndex = 3 });
            if (listeningSkill != null) sections.Add(new PlacementTestSection { PlacementTestId = test.Id, SkillId = listeningSkill.Id, SectionName = "Listening", Instruction = "Listen and choose the correct answer.", OrderIndex = 4 });

            foreach (var sec in sections) _context.PlacementTestSections.Add(sec);
            await _context.SaveChangesAsync();

            // 3. Create 20 Questions
            var questions = new List<QuestionBank>();
            
            // Vocab Questions
            if (vocabSkill != null)
            {
                for (int i = 1; i <= 5; i++)
                {
                    questions.Add(CreateMCQ(vocabSkill.Id, a2Level.Id, adminUser.Id, $"Vocabulary Question {i}", $"Vocab Option {i}A", $"Vocab Option {i}B", $"Vocab Option {i}C", $"Vocab Option {i}D", 1));
                }
            }

            // Grammar Questions
            if (grammarSkill != null)
            {
                for (int i = 1; i <= 5; i++)
                {
                    questions.Add(CreateMCQ(grammarSkill.Id, a2Level.Id, adminUser.Id, $"Grammar Question {i}", $"Grammar Option {i}A", $"Grammar Option {i}B", $"Grammar Option {i}C", $"Grammar Option {i}D", 2));
                }
            }

            // Reading Questions
            if (readingSkill != null)
            {
                for (int i = 1; i <= 5; i++)
                {
                    questions.Add(CreateMCQ(readingSkill.Id, a2Level.Id, adminUser.Id, $"Reading Question {i} based on passage", $"Reading Option {i}A", $"Reading Option {i}B", $"Reading Option {i}C", $"Reading Option {i}D", 3));
                }
            }

            // Listening Questions
            if (listeningSkill != null)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var q = CreateMCQ(listeningSkill.Id, a2Level.Id, adminUser.Id, $"Listening Question {i}", $"Listen Option {i}A", $"Listen Option {i}B", $"Listen Option {i}C", $"Listen Option {i}D", 4);
                    q.AudioUrl = "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3";
                    questions.Add(q);
                }
            }

            foreach (var q in questions) _context.QuestionBanks.Add(q);
            await _context.SaveChangesAsync();

            // 4. Link Questions to Sections
            foreach (var section in sections)
            {
                int skillId = section.SkillId;
                var sectionQs = questions.Where(q => q.SkillId == skillId).ToList();
                int order = 1;
                foreach (var sq in sectionQs)
                {
                    _context.PlacementTestQuestions.Add(new PlacementTestQuestion
                    {
                        SectionId = section.Id,
                        QuestionId = sq.Id,
                        Points = 5,
                        OrderIndex = order++
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        private QuestionBank CreateMCQ(int skillId, int levelId, int adminId, string text, string opt1, string opt2, string opt3, string opt4, int correctIndex)
        {
            var options = new List<QuestionOption>
            {
                new QuestionOption { OptionText = opt1, IsCorrect = correctIndex == 1, OrderIndex = 1 },
                new QuestionOption { OptionText = opt2, IsCorrect = correctIndex == 2, OrderIndex = 2 },
                new QuestionOption { OptionText = opt3, IsCorrect = correctIndex == 3, OrderIndex = 3 },
                new QuestionOption { OptionText = opt4, IsCorrect = correctIndex == 4, OrderIndex = 4 }
            };

            return new QuestionBank
            {
                SkillId = skillId,
                LevelId = levelId,
                QuestionType = "MCQ",
                QuestionText = text,
                DifficultyLevel = "MEDIUM",
                SourceType = "SYSTEM",
                ReviewStatus = "APPROVED",
                CreatedBy = adminId,
                ApprovedBy = adminId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ApprovedAt = DateTime.UtcNow,
                CorrectAnswer = options[correctIndex - 1].OptionText,
                QuestionOptions = options
            };
        }
    }
}
