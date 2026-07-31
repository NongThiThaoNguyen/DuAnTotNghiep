using DuAnTotNghiep.Helpers;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Repositories.Interfaces;
using DuAnTotNghiep.Models.Enums;
using Microsoft.EntityFrameworkCore;

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
            await SeedTopicsAndObjectivesAsync();
            await SeedM8LearningPathAssetsAsync();
            await SeedLearningPathDemoAsync();
            await SeedReferenceSourcesAsync();
            await SeedOriginalLessonsAsync();
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
            var adminUser = await _userRepository.GetByEmailAsync("admin@aistudyenglish.com");
            var beginnerLevel = (await _levelRepository.GetAllAsync()).FirstOrDefault(l => l.Code == "BEGINNER");
            
            if (adminUser == null || beginnerLevel == null) return;

            var test = await _context.PlacementTests.FirstOrDefaultAsync(t => t.Title == "General English Placement Test");
            if (test == null)
            {
                test = new PlacementTest
                {
                    Title = "General English Placement Test",
                    Description = "Bài đánh giá năng lực tiếng Anh tổng quát.",
                    TimeLimitMinutes = 30,
                    TargetLevelId = beginnerLevel.Id,
                    CreatedBy = adminUser.Id,
                    Status = PlacementTestStatus.Published,
                    TotalScore = 100,
                    CreatedAt = DateTime.UtcNow
                };
                _context.PlacementTests.Add(test);
                await _context.SaveChangesAsync();
            }
            else
            {
                test.TotalScore = 100;
                test.Status = PlacementTestStatus.Published;
                _context.PlacementTests.Update(test);
                await _context.SaveChangesAsync();
            }

            // Check if sections already exist for this test
            bool hasSections = await _context.PlacementTestSections.AnyAsync(s => s.PlacementTestId == test.Id);
            if (!hasSections)
            {
                var skills = await _context.EnglishSkills.ToListAsync();
                var grammarSkill = skills.FirstOrDefault(s => s.SkillCode == "GRAMMAR");
                var vocabSkill = skills.FirstOrDefault(s => s.SkillCode == "VOCABULARY");
                var readingSkill = skills.FirstOrDefault(s => s.SkillCode == "READING");

                if (grammarSkill == null || vocabSkill == null || readingSkill == null)
                    return;

                // Create Sections
                var grammarSection = new PlacementTestSection
                {
                    PlacementTestId = test.Id,
                    SkillId = grammarSkill.Id,
                    SectionName = "Grammar",
                    Instruction = "Choose the correct grammar structure.",
                    OrderIndex = 1
                };
                var vocabSection = new PlacementTestSection
                {
                    PlacementTestId = test.Id,
                    SkillId = vocabSkill.Id,
                    SectionName = "Vocabulary",
                    Instruction = "Choose the best word to complete the sentence.",
                    OrderIndex = 2
                };
                var readingSection = new PlacementTestSection
                {
                    PlacementTestId = test.Id,
                    SkillId = readingSkill.Id,
                    SectionName = "Reading",
                    Instruction = "Read the passage and answer the questions.",
                    OrderIndex = 3
                };

                _context.PlacementTestSections.Add(grammarSection);
                _context.PlacementTestSections.Add(vocabSection);
                _context.PlacementTestSections.Add(readingSection);
                await _context.SaveChangesAsync();

                // Define placement questions
                var grammarQuestionsRaw = new (string QuestionText, string[] Options, int CorrectIndex)[]
                {
                    ("She ___ to school every day.", new[] {"go","goes","going","gone"}, 1),
                    ("They ___ watching TV right now.", new[] {"is","are","am","be"}, 1),
                    ("I ___ my homework yesterday.", new[] {"do","did","done","doing"}, 1),
                    ("There ___ a book on the table.", new[] {"is","are","be","been"}, 0),
                    ("He is ___ than his brother.", new[] {"tall","taller","tallest","more tall"}, 1),
                    ("___ you like coffee?", new[] {"Do","Does","Are","Is"}, 0),
                    ("She has ___ apple.", new[] {"a","an","the","-"}, 1),
                    ("We ___ to the beach last summer.", new[] {"go","went","gone","going"}, 1),
                    ("This is one ___ my favourite books.", new[] {"of","from","at","in"}, 0),
                    ("I have lived here ___ 2020.", new[] {"since","for","from","at"}, 0),
                    ("If it rains, I ___ stay home.", new[] {"will","would","can","must"}, 0),
                    ("She is the ___ student in class.", new[] {"good","better","best","most good"}, 2),
                    ("He can't come ___ he is sick.", new[] {"because","but","so","although"}, 0),
                    ("They ___ finished the project yet.", new[] {"haven't","hasn't","didn't","doesn't"}, 0),
                    ("My sister is ___ than me.", new[] {"young","younger","youngest","more young"}, 1),
                    ("I ___ never been to Japan.", new[] {"have","has","had","having"}, 0),
                    ("Please ___ the door.", new[] {"close","closes","closed","closing"}, 0),
                    ("She said she ___ tired.", new[] {"is","was","be","been"}, 1),
                    ("How ___ apples do you want?", new[] {"much","many","a lot","lot of"}, 1),
                    ("I look forward to ___ you soon.", new[] {"see","seeing","saw","seen"}, 1),
                    ("By the time we arrived, the movie ___ already started.", new[] {"has","had","have","was"}, 1),
                    ("The report ___ by the manager tomorrow.", new[] {"reviews","will review","will be reviewed","reviewed"}, 2),
                    ("If I ___ more time, I would travel more.", new[] {"have","had","has","having"}, 1),
                    ("The book ___ I bought yesterday is interesting.", new[] {"who","that","whom","whose"}, 1),
                    ("She's been working here ___ five years.", new[] {"since","for","from","at"}, 1),
                    ("Neither of the answers ___ correct.", new[] {"is","are","were","be"}, 0),
                    ("He suggested ___ to the doctor.", new[] {"go","going","to go","went"}, 1),
                    ("The bridge ___ in 1990.", new[] {"built","was built","builds","building"}, 1),
                    ("I wish I ___ speak French.", new[] {"can","could","will","would"}, 1),
                    ("Despite ___ hard, he failed the exam.", new[] {"study","studies","studying","studied"}, 2),
                    ("The more you practice, ___ you become.", new[] {"good","better","the better","best"}, 2),
                    ("She denied ___ the money.", new[] {"take","taking","took","to take"}, 1),
                    ("Not only ___ late, but he also forgot his homework.", new[] {"he was","was he","he is","is he"}, 1),
                    ("Had I known, I ___ have come earlier.", new[] {"will","would","can","must"}, 1),
                    ("The city ___ population has grown rapidly is now overcrowded.", new[] {"that","which","whose","who"}, 2),
                    ("He is used to ___ up early.", new[] {"get","gets","getting","got"}, 2),
                    ("It's high time we ___ a decision.", new[] {"make","made","making","makes"}, 1),
                    ("The number of students ___ increasing every year.", new[] {"is","are","were","have"}, 0),
                    ("I'd rather you ___ smoke here.", new[] {"don't","didn't","not","won't"}, 1),
                    ("She acted as if she ___ everything about it.", new[] {"knows","knew","know","known"}, 1),
                    ("Scarcely ___ the movie started when the lights went out.", new[] {"had","has","did","was"}, 0),
                    ("Few people ___ aware of the risks involved.", new[] {"is","are","was","been"}, 1),
                    ("This information ___ very useful for our research.", new[] {"is","are","were","been"}, 0),
                    ("Each of the students ___ a laptop.", new[] {"have","has","having","had"}, 1),
                    ("The data collected over five years ___ still being analysed.", new[] {"is","are","was","been"}, 0)
                };

                var vocabQuestionsRaw = new (string QuestionText, string[] Options, int CorrectIndex)[]
                {
                    ("\"Environment\" most closely means:", new[] {"the natural world","a type of food","a business plan","a school subject"}, 0),
                    ("A synonym of \"increase\" is:", new[] {"fall","rise","stay","stop"}, 1),
                    ("\"However\" is used to show:", new[] {"addition","contrast","a result","a reason"}, 1),
                    ("The opposite of \"decrease\" is:", new[] {"reduce","increase","lower","drop"}, 1),
                    ("\"Achieve\" means:", new[] {"to succeed in doing something","to forget something","to buy something","to lose something"}, 0),
                    ("\"Opportunity\" means:", new[] {"a problem","a chance","a rule","a mistake"}, 1),
                    ("A synonym of \"important\" is:", new[] {"significant","tiny","boring","random"}, 0),
                    ("\"Furthermore\" is used to:", new[] {"add more information","show doubt","end an idea","ask a question"}, 0),
                    ("\"Benefit\" means:", new[] {"an advantage","a punishment","a document","a delay"}, 0),
                    ("\"Recommend\" means:", new[] {"to forbid","to suggest","to ignore","to copy"}, 1),
                    ("\"According to the report\" means:", new[] {"based on the report","instead of the report","against the report","before the report"}, 0),
                    ("\"In addition\" means:", new[] {"also","however","therefore","unless"}, 0),
                    ("\"On the other hand\" is used to:", new[] {"show a different viewpoint","give an example","show agreement","end a story"}, 0),
                    ("\"Prefer\" means:", new[] {"to like one thing more than another","to dislike something","to forget something","to sell something"}, 0),
                    ("\"Describe\" means:", new[] {"to explain what something is like","to buy something","to hide something","to delay something"}, 0),
                    ("A synonym of \"difficult\" is:", new[] {"challenging","simple","cheap","fast"}, 0),
                    ("\"Compare\" means:", new[] {"to look at similarities and differences","to ignore something","to translate something","to repeat something"}, 0),
                    ("\"Appointment\" means:", new[] {"a scheduled meeting","a type of food","a holiday","a mistake"}, 0),
                    ("Which is the correct collocation?", new[] {"make a decision","do a decision","take a decision","have a decision"}, 0),
                    ("Which is the correct collocation?", new[] {"do a photo","take a photo","make a photo","have a photo"}, 1),
                    ("Which is the correct collocation?", new[] {"make homework","do homework","take homework","have homework"}, 1),
                    ("Which is the correct collocation?", new[] {"achieve a goal","make a goal","do a goal","take a goal"}, 0),
                    ("Which is the correct collocation?", new[] {"do a role","take a role","play a role","make a role"}, 2),
                    ("A synonym of \"huge\" is:", new[] {"enormous","tiny","narrow","short"}, 0),
                    ("A synonym of \"quick\" is:", new[] {"rapid","slow","late","calm"}, 0),
                    ("\"Global warming\" refers to:", new[] {"rising world temperatures","falling sea levels","new technology","population decline"}, 0),
                    ("\"Sustainable\" means:", new[] {"able to continue without causing harm","expensive","temporary","dangerous"}, 0),
                    ("\"Urban\" refers to:", new[] {"a city area","a farm","a forest","a river"}, 0),
                    ("\"Rural\" refers to:", new[] {"the countryside","a shopping mall","an airport","a factory"}, 0),
                    ("\"Consequently\" means:", new[] {"as a result","by contrast","for example","in summary"}, 0),
                    ("\"Despite\" is used to show:", new[] {"contrast","cause","time","addition"}, 0),
                    ("The antonym of \"increase\" is:", new[] {"decrease","expand","grow","rise"}, 0),
                    ("\"Significant\" means:", new[] {"important or notable","boring","cheap","short"}, 0),
                    ("\"Approximately\" means:", new[] {"exactly","about / roughly","never","always"}, 1),
                    ("\"Essential\" means:", new[] {"necessary","optional","forbidden","unusual"}, 0)
                };

                var readingQuestionsRaw = new (string QuestionText, string[] Options, int CorrectIndex)[]
                {
                    ("Mai works in a small coffee shop in the city centre. She starts work at 7 a.m. and finishes at 3 p.m. every day except Sunday.<br><br><b>What time does Mai finish work?</b>", new[] {"7 a.m.","3 p.m.","Sunday","All day"}, 1),
                    ("The weather forecast says it will rain tomorrow, so John decided to take his umbrella to work.<br><br><b>Why did John take his umbrella?</b>", new[] {"Because it is sunny","Because he lost his coat","Because it will rain","Because he works late"}, 2),
                    ("Many students choose to study abroad because they want to improve their English and experience a new culture.<br><br><b>What is one reason students study abroad?</b>", new[] {"To earn more money","To improve their English","To avoid exams","To stay near family"}, 1),
                    ("The library closes at 9 p.m. on weekdays, but on weekends it closes earlier, at 5 p.m.<br><br><b>When does the library close earlier?</b>", new[] {"Weekdays","Weekends","Every day","Never"}, 1),
                    ("Although Linh was tired after a long day at work, she still went to the gym to keep fit.<br><br><b>What did Linh do despite being tired?</b>", new[] {"She went home to sleep","She went to the gym","She cancelled her plans","She stayed at work"}, 1),
                    ("The company has grown quickly in the past two years, doubling its number of employees.<br><br><b>What happened to the number of employees?</b>", new[] {"It stayed the same","It halved","It doubled","It disappeared"}, 2),
                    ("Recycling helps reduce waste and protects the environment, but many people still don't recycle regularly.<br><br><b>What is one benefit of recycling mentioned?</b>", new[] {"It reduces waste","It creates more waste","It costs more money","It takes more time"}, 0),
                    ("The new bridge will connect the two cities, reducing travel time by half an hour.<br><br><b>What will the new bridge do?</b>", new[] {"Increase travel time","Reduce travel time","Close both cities","Replace the airport"}, 1),
                    ("Due to heavy traffic, Nam arrived at the meeting twenty minutes late.<br><br><b>Why was Nam late?</b>", new[] {"He overslept","Heavy traffic","He forgot the meeting","He was sick"}, 1),
                    ("Scientists warn that global temperatures could rise by two degrees by 2100 if nothing changes.<br><br><b>What might happen by 2100?</b>", new[] {"Temperatures could rise","Temperatures could fall","Nothing will change","Temperatures will stay the same"}, 0),
                    ("The museum offers free entry on the first Sunday of every month.<br><br><b>When is entry to the museum free?</b>", new[] {"Every Sunday","The first Sunday of the month","Every day","Only on holidays"}, 1),
                    ("Although the exam was difficult, most students passed because they had studied hard.<br><br><b>Why did most students pass?</b>", new[] {"The exam was easy","They studied hard","The teacher helped during the exam","They guessed the answers"}, 1),
                    ("The government has introduced a new policy to reduce plastic use in supermarkets.<br><br><b>What is the new policy about?</b>", new[] {"Increasing plastic use","Reducing plastic use","Closing supermarkets","Building new supermarkets"}, 1),
                    ("Because of the pandemic, many companies allowed employees to work from home.<br><br><b>What did companies allow?</b>", new[] {"Working from home","Longer holidays","Higher salaries","Shorter work hours"}, 0),
                    ("The hotel is located near the beach, making it popular with tourists in summer.<br><br><b>Why is the hotel popular in summer?</b>", new[] {"It is cheap","It is near the beach","It has a pool","It is in the city centre"}, 1),
                    ("Reading before bed can help you relax and fall asleep more easily.<br><br><b>What can reading before bed help with?</b>", new[] {"Waking up early","Falling asleep","Working faster","Eating healthily"}, 1),
                    ("The report shows that online shopping has increased significantly since 2020.", new[] {"In-store shopping","Online shopping","Travel bookings","Restaurant visits"}, 1),
                    ("Farmers in the region have struggled due to a lack of rain this year.<br><br><b>What problem have farmers faced?</b>", new[] {"Too much rain","Lack of rain","Low crop prices","New taxes"}, 1),
                    ("Public transport in the city has improved, with more buses now running every ten minutes.<br><br><b>What has improved?</b>", new[] {"Public transport","Air quality","School education","Housing prices"}, 0),
                    ("The university offers scholarships to students who achieve excellent academic results.<br><br><b>Who can get a scholarship?</b>", new[] {"All students","Students with excellent results","Only international students","Only first-year students"}, 1)
                };

                var grammarQs = new List<QuestionBank>();
                var vocabQs = new List<QuestionBank>();
                var readingQs = new List<QuestionBank>();

                foreach (var q in grammarQuestionsRaw)
                {
                    grammarQs.Add(CreateMCQ(grammarSkill.Id, beginnerLevel.Id, adminUser.Id, q.QuestionText, q.Options[0], q.Options[1], q.Options[2], q.Options[3], q.CorrectIndex + 1));
                }
                foreach (var q in vocabQuestionsRaw)
                {
                    vocabQs.Add(CreateMCQ(vocabSkill.Id, beginnerLevel.Id, adminUser.Id, q.QuestionText, q.Options[0], q.Options[1], q.Options[2], q.Options[3], q.CorrectIndex + 1));
                }
                foreach (var q in readingQuestionsRaw)
                {
                    readingQs.Add(CreateMCQ(readingSkill.Id, beginnerLevel.Id, adminUser.Id, q.QuestionText, q.Options[0], q.Options[1], q.Options[2], q.Options[3], q.CorrectIndex + 1));
                }

                var allQuestions = new List<QuestionBank>();
                allQuestions.AddRange(grammarQs);
                allQuestions.AddRange(vocabQs);
                allQuestions.AddRange(readingQs);

                foreach (var q in allQuestions)
                {
                    _context.QuestionBanks.Add(q);
                }
                await _context.SaveChangesAsync();

                // Link to Sections
                int grammarOrder = 1;
                foreach (var q in grammarQs)
                {
                    _context.PlacementTestQuestions.Add(new PlacementTestQuestion
                    {
                        SectionId = grammarSection.Id,
                        QuestionId = q.Id,
                        Points = 2, // 2 points per question
                        OrderIndex = grammarOrder++
                    });
                }

                int vocabOrder = 1;
                foreach (var q in vocabQs)
                {
                    _context.PlacementTestQuestions.Add(new PlacementTestQuestion
                    {
                        SectionId = vocabSection.Id,
                        QuestionId = q.Id,
                        Points = 2,
                        OrderIndex = vocabOrder++
                    });
                }

                int readingOrder = 1;
                foreach (var q in readingQs)
                {
                    _context.PlacementTestQuestions.Add(new PlacementTestQuestion
                    {
                        SectionId = readingSection.Id,
                        QuestionId = q.Id,
                        Points = 2,
                        OrderIndex = readingOrder++
                    });
                }

                await _context.SaveChangesAsync();
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

        private async Task SeedTopicsAndObjectivesAsync()
        {
            var grammarSkill = _context.EnglishSkills.FirstOrDefault(s => s.SkillCode == "GRAMMAR");
            var vocabSkill = _context.EnglishSkills.FirstOrDefault(s => s.SkillCode == "VOCABULARY");
            var beginnerLevel = _context.EnglishProficiencyLevels.FirstOrDefault(l => l.Code == "BEGINNER");
            var elementaryLevel = _context.EnglishProficiencyLevels.FirstOrDefault(l => l.Code == "ELEMENTARY");

            if (grammarSkill == null || vocabSkill == null || beginnerLevel == null || elementaryLevel == null)
                return;

            // 1. Seed Topic Grammar -> Tenses
            var tensesTopic = _context.LearningTopics.FirstOrDefault(t => t.TopicCode == "GRAM_TENSES");
            if (tensesTopic == null)
            {
                tensesTopic = new LearningTopic
                {
                    TopicCode = "GRAM_TENSES",
                    Title = "Tenses",
                    Description = "Các thì trong tiếng Anh",
                    SkillId = grammarSkill.Id,
                    LevelId = beginnerLevel.Id,
                    DifficultyLevel = "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(tensesTopic);
                await _context.SaveChangesAsync();
            }

            if (!_context.LearningObjectives.Any(o => o.TopicId == tensesTopic.Id))
            {
                _context.LearningObjectives.Add(new LearningObjective
                {
                    TopicId = tensesTopic.Id,
                    ObjectiveText = "Hiểu và phân biệt các thì cơ bản trong tiếng Anh",
                    CognitiveLevel = "UNDERSTAND",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // 2. Seed Topic Grammar -> Present Simple (con của Tenses)
            var presSimpleTopic = _context.LearningTopics.FirstOrDefault(t => t.TopicCode == "GRAM_PRES_SIMP");
            if (presSimpleTopic == null)
            {
                presSimpleTopic = new LearningTopic
                {
                    TopicCode = "GRAM_PRES_SIMP",
                    Title = "Present Simple",
                    Description = "Thì hiện tại đơn",
                    SkillId = grammarSkill.Id,
                    LevelId = beginnerLevel.Id,
                    ParentTopicId = tensesTopic.Id,
                    DifficultyLevel = "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(presSimpleTopic);
                await _context.SaveChangesAsync();
            }

            if (!_context.LearningObjectives.Any(o => o.TopicId == presSimpleTopic.Id))
            {
                _context.LearningObjectives.Add(new LearningObjective
                {
                    TopicId = presSimpleTopic.Id,
                    ObjectiveText = "Sử dụng thì hiện tại đơn để nói về thói quen",
                    CognitiveLevel = "APPLY",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // 3. Seed Topic Grammar -> Present Continuous (con của Tenses)
            var presContTopic = _context.LearningTopics.FirstOrDefault(t => t.TopicCode == "GRAM_PRES_CONT");
            if (presContTopic == null)
            {
                presContTopic = new LearningTopic
                {
                    TopicCode = "GRAM_PRES_CONT",
                    Title = "Present Continuous",
                    Description = "Thì hiện tại tiếp diễn",
                    SkillId = grammarSkill.Id,
                    LevelId = beginnerLevel.Id,
                    ParentTopicId = tensesTopic.Id,
                    DifficultyLevel = "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(presContTopic);
                await _context.SaveChangesAsync();
            }

            if (!_context.LearningObjectives.Any(o => o.TopicId == presContTopic.Id))
            {
                _context.LearningObjectives.Add(new LearningObjective
                {
                    TopicId = presContTopic.Id,
                    ObjectiveText = "Sử dụng thì hiện tại tiếp diễn cho hành động đang xảy ra",
                    CognitiveLevel = "APPLY",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // 4. Seed Topic Vocabulary -> Family
            var familyTopic = _context.LearningTopics.FirstOrDefault(t => t.TopicCode == "VOCAB_FAMILY");
            if (familyTopic == null)
            {
                familyTopic = new LearningTopic
                {
                    TopicCode = "VOCAB_FAMILY",
                    Title = "Family",
                    Description = "Chủ đề gia đình",
                    SkillId = vocabSkill.Id,
                    LevelId = beginnerLevel.Id,
                    DifficultyLevel = "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(familyTopic);
                await _context.SaveChangesAsync();
            }

            if (!_context.LearningObjectives.Any(o => o.TopicId == familyTopic.Id))
            {
                _context.LearningObjectives.Add(new LearningObjective
                {
                    TopicId = familyTopic.Id,
                    ObjectiveText = "Nhận biết và gọi tên các thành viên trong gia đình",
                    CognitiveLevel = "REMEMBER",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // 5. Seed Topic Vocabulary -> School
            var schoolTopic = _context.LearningTopics.FirstOrDefault(t => t.TopicCode == "VOCAB_SCHOOL");
            if (schoolTopic == null)
            {
                schoolTopic = new LearningTopic
                {
                    TopicCode = "VOCAB_SCHOOL",
                    Title = "School",
                    Description = "Chủ đề trường học",
                    SkillId = vocabSkill.Id,
                    LevelId = beginnerLevel.Id,
                    DifficultyLevel = "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(schoolTopic);
                await _context.SaveChangesAsync();
            }

            if (!_context.LearningObjectives.Any(o => o.TopicId == schoolTopic.Id))
            {
                _context.LearningObjectives.Add(new LearningObjective
                {
                    TopicId = schoolTopic.Id,
                    ObjectiveText = "Từ vựng về các đồ dùng và hoạt động ở trường học",
                    CognitiveLevel = "REMEMBER",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            // 6. Seed Topic Vocabulary -> Travel
            var travelTopic = _context.LearningTopics.FirstOrDefault(t => t.TopicCode == "VOCAB_TRAVEL");
            if (travelTopic == null)
            {
                travelTopic = new LearningTopic
                {
                    TopicCode = "VOCAB_TRAVEL",
                    Title = "Travel",
                    Description = "Chủ đề du lịch",
                    SkillId = vocabSkill.Id,
                    LevelId = elementaryLevel.Id,
                    DifficultyLevel = "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = 3,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(travelTopic);
                await _context.SaveChangesAsync();
            }

            if (!_context.LearningObjectives.Any(o => o.TopicId == travelTopic.Id))
            {
                _context.LearningObjectives.Add(new LearningObjective
                {
                    TopicId = travelTopic.Id,
                    ObjectiveText = "Từ vựng thông dụng khi đi du lịch và hỏi đường",
                    CognitiveLevel = "REMEMBER",
                    OrderIndex = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task SeedM8LearningPathAssetsAsync()
        {
            await SeedM8PromptTemplateAsync();
            await SeedM8PathTemplatesAsync();
        }

        private async Task SeedM8PromptTemplateAsync()
        {
            if (await _context.AiPromptTemplates.AnyAsync(t => t.PromptCode == "M8_LEARNING_PATH_V1")) return;

            _context.AiPromptTemplates.Add(new AiPromptTemplate
            {
                PromptCode = "M8_LEARNING_PATH_V1",
                PromptName = "M8 Learning Path Generator",
                ModuleCode = "LEARNING_PATH",
                VersionNo = 1,
                SystemPrompt = "Generate a personalized English learning path using approved topics, lessons, and quizzes only.",
                OutputSchema = "LearningPathOutputDto: pathTitle, summary, totalWeeks, phases[n].nodes[n]",
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task SeedM8PathTemplatesAsync()
        {
            var topics = await _context.LearningTopics.Where(t => t.Status == "ACTIVE").OrderBy(t => t.OrderIndex).Take(6).ToListAsync();
            if (topics.Count == 0) return;

            var goal = await _context.LearningGoals.OrderBy(g => g.Id).FirstOrDefaultAsync(g => g.IsActive);
            var startLevel = await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).FirstOrDefaultAsync(l => l.IsActive);
            var targetLevel = await _context.EnglishProficiencyLevels.OrderBy(l => l.OrderIndex).Skip(1).FirstOrDefaultAsync(l => l.IsActive);
            var adminId = await _context.Users.Where(u => u.Email == "admin@aistudyenglish.com").Select(u => (int?)u.Id).FirstOrDefaultAsync();

            await AddM8TemplateAsync("M8 Foundation Template", goal?.Id, startLevel?.Id, targetLevel?.Id, topics.Take(3).ToList(), adminId);
            await AddM8TemplateAsync("M8 Practice Template", goal?.Id, startLevel?.Id, targetLevel?.Id, topics.Skip(3).DefaultIfEmpty(topics[0]).Take(3).ToList(), adminId);
        }

        private async Task AddM8TemplateAsync(
            string name,
            int? goalId,
            int? startLevelId,
            int? targetLevelId,
            List<LearningTopic> topics,
            int? adminId)
        {
            if (await _context.LearningPathTemplates.AnyAsync(t => t.TemplateName == name)) return;

            var template = new LearningPathTemplate
            {
                TemplateName = name,
                GoalId = goalId,
                StartLevelId = startLevelId,
                TargetLevelId = targetLevelId,
                DurationWeeks = 4,
                Description = "Published fallback template for M8 AI learning path generation.",
                Status = "PUBLISHED",
                CreatedBy = adminId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var topic in topics.Select((topic, index) => new { topic, index }))
            {
                template.LearningPathTemplateNodes.Add(CreateM8TemplateNode(topic.topic, topic.index + 1));
            }

            _context.LearningPathTemplates.Add(template);
            await _context.SaveChangesAsync();
        }

        private static LearningPathTemplateNode CreateM8TemplateNode(LearningTopic topic, int orderIndex)
        {
            return new LearningPathTemplateNode
            {
                TopicId = topic.Id,
                SkillId = topic.SkillId,
                NodeTitle = topic.Title,
                NodeType = NodeType.Topic,
                EstimatedMinutes = topic.EstimatedMinutes ?? 20,
                OrderIndex = orderIndex,
                UnlockCondition = orderIndex == 1 ? "FIRST_NODE" : "PREVIOUS_NODE_COMPLETED"
            };
        }

        private async Task SeedReferenceSourcesAsync()
        {
            if (!await _context.ReferenceSources.AnyAsync())
            {
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@aistudyenglish.com");
                int? adminId = adminUser?.Id;

                var defaultRef = new ReferenceSource
                {
                    SourceName = "Default Internal Reference",
                    SourceUrl = "https://aistudyenglish.local",
                    SourceType = ReferenceSourceType.REFERENCE_ONLY,
                    LicenseNote = "Tài liệu tham khảo nội bộ của hệ thống.",
                    UsagePolicy = ReferenceUsagePolicy.RESTRICTED,
                    Status = ReferenceReviewStatus.APPROVED,
                    CreatedBy = adminId,
                    ApprovedBy = adminId,
                    CreatedAt = DateTime.UtcNow,
                    ApprovedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ReferenceSources.Add(defaultRef);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SeedLearningPathDemoAsync()
        {
            var student = await _userRepository.GetByEmailAsync("student1@aistudyenglish.com");
            if (student == null)
            {
                return;
            }

            var topics = await _context.LearningTopics
                .Where(t => t.Status == "ACTIVE")
                .OrderBy(t => t.OrderIndex)
                .ThenBy(t => t.Id)
                .Take(10)
                .ToListAsync();

            if (topics.Count == 0)
            {
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = DateTime.UtcNow;

            var hasCompletedPlacement = await _context.TestAttempts.AnyAsync(a =>
                a.StudentId == student.Id &&
                (a.Status == TestAttemptStatus.Submitted || a.Status == TestAttemptStatus.Graded));

            if (!hasCompletedPlacement)
            {
                var placementTest = await _context.PlacementTests
                    .Where(t => t.Status == PlacementTestStatus.Published)
                    .OrderBy(t => t.Id)
                    .FirstOrDefaultAsync();
                var estimatedLevel = await _context.EnglishProficiencyLevels
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.OrderIndex)
                    .ThenBy(l => l.Id)
                    .FirstOrDefaultAsync();

                if (placementTest != null)
                {
                    _context.TestAttempts.Add(new TestAttempt
                    {
                        PlacementTestId = placementTest.Id,
                        StudentId = student.Id,
                        StartedAt = now.AddDays(-3),
                        SubmittedAt = now.AddDays(-3).AddMinutes(30),
                        TotalScore = 72m,
                        EstimatedLevelId = estimatedLevel?.Id,
                        Status = TestAttemptStatus.Graded
                    });
                    await _context.SaveChangesAsync();
                }
            }

            var existingPath = await _context.StudentLearningPaths
                .Include(p => p.LearningPathNodes)
                .FirstOrDefaultAsync(p => p.StudentId == student.Id && (p.Title == "M9 Demo Learning Path" || p.Title == "Lộ trình học tập AI mẫu"));
            if (existingPath != null)
            {
                if (existingPath.LearningPathNodes != null && existingPath.LearningPathNodes.Any())
                {
                    _context.LearningPathNodes.RemoveRange(existingPath.LearningPathNodes);
                }
                _context.StudentLearningPaths.Remove(existingPath);
                await _context.SaveChangesAsync();
            }

            var path = new StudentLearningPath
            {
                StudentId = student.Id,
                Title = "M9 Demo Learning Path",
                Description = "Lộ trình học tập thông minh gợi ý bởi AI theo phong cách Duolingo.",
                StartDate = today.AddDays(-2),
                TargetEndDate = today.AddDays(21),
                AiPlanSummary = "Bắt đầu với các nền tảng ngữ pháp cơ bản, kiểm tra mức độ hiểu bài, sau đó ôn luyện xen kẽ với thực hành.",
                Status = "ACTIVE",
                GeneratedByAi = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.StudentLearningPaths.Add(path);
            await _context.SaveChangesAsync();

            var nodeSpecs = new[]
            {
                ("Khởi động Ngữ pháp", NodeType.Topic, ProgressStatus.Completed, "Foundation", -2, 12),
                ("Bài học: Thì Hiện tại đơn", NodeType.Lesson, ProgressStatus.Completed, "Foundation", -1, 18),
                ("Chủ đề: Thói quen hàng ngày", NodeType.Topic, ProgressStatus.Available, "Foundation", 0, 12),
                ("Thực hành: Câu về thói quen", NodeType.Practice, ProgressStatus.InProgress, "Practice", 0, 20),
                ("Ôn tập các phần còn yếu", NodeType.Review, ProgressStatus.NeedReview, "Practice", 1, 15),
                ("Tương tác với AI Tutor", NodeType.AiTutor, ProgressStatus.Locked, "Guided AI", 2, 10),
                ("Chủ đề: Từ vựng gia đình", NodeType.Topic, ProgressStatus.Locked, "Vocabulary", 3, 16),
                ("Bài học: Từ vựng trường học", NodeType.Lesson, ProgressStatus.Locked, "Vocabulary", 4, 18),
                ("Trắc nghiệm: Cơ bản du lịch", NodeType.Quiz, ProgressStatus.Locked, "Checkpoint", 5, 12),
                ("Ôn tập tổng kết", NodeType.Review, ProgressStatus.Locked, "Checkpoint", 6, 20)
            };

            var nodes = nodeSpecs.Select((spec, index) =>
            {
                var topic = topics[index % topics.Count];
                return new LearningPathNode
                {
                    LearningPathId = path.Id,
                    TopicId = topic.Id,
                    NodeTitle = spec.Item1,
                    NodeDescription = topic.Description,
                    NodeType = spec.Item2,
                    PathPhase = spec.Item4,
                    ScheduledDate = today.AddDays(spec.Item5),
                    EstimatedMinutes = spec.Item6,
                    OrderIndex = index + 1,
                    Status = spec.Item3,
                    AiReason = $"Được đề xuất vì bài học này bổ trợ kiến thức cho chủ đề {topic.Title}.",
                    CompletedAt = spec.Item3 == ProgressStatus.Completed ? now.AddDays(-Math.Max(1, 2 - index)) : null
                };
            });

            _context.LearningPathNodes.AddRange(nodes);
            await _context.SaveChangesAsync();
        }
        private async Task SeedOriginalLessonsAsync()
        {
            var teacherRole = await _roleRepository.GetByCodeAsync("TEACHER");
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == teacherRole.Id);
            
            var topics = await _context.LearningTopics
                .Include(t => t.OriginalLessons)
                .ToListAsync();

            bool changesMade = false;
            foreach (var topic in topics)
            {
                if (topic.OriginalLessons == null || !topic.OriginalLessons.Any())
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        var lesson = new OriginalLesson
                        {
                            TopicId = topic.Id,
                            Title = $"Bài học {i}: Hướng dẫn cơ bản về {topic.Title}",
                            Summary = $"Nội dung bài học {i} cung cấp kiến thức nền tảng giúp bạn hiểu rõ hơn về {topic.Title}.",
                            Content = $"<h3>Mục tiêu bài học</h3><p>Trong bài học này, chúng ta sẽ tìm hiểu về {topic.Title} qua các ví dụ thực tế.</p><h4>Ví dụ</h4><p>Hãy chú ý các tình huống sử dụng trong đời sống hàng ngày.</p>",
                            ContentType = "ARTICLE",
                            EstimatedMinutes = 15,
                            SourceType = "SYSTEM",
                            ReviewStatus = "APPROVED",
                            IsAiGenerated = false,
                            CreatedBy = teacher?.Id,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.OriginalLessons.Add(lesson);
                        changesMade = true;
                    }
                }
            }

            if (changesMade)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
