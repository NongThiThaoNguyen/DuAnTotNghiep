using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.Enums;

namespace DuAnTotNghiep.Data.Seeders;

public class AILearnSeeder
{
    private readonly ApplicationDbContext _context;

    public AILearnSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // 1. Create Achievements and UserAchievements tables if they do not exist
        await CreateTablesIfNotExistAsync();

        // 2. Seed Skills & Levels if missing (just to ensure references are present)
        var skills = await _context.EnglishSkills.ToListAsync();
        var levels = await _context.EnglishProficiencyLevels.ToListAsync();
        if (!skills.Any() || !levels.Any())
        {
            return; // Wait for core seeder to run first
        }

        var grammarSkill = skills.FirstOrDefault(s => s.SkillCode == "GRAMMAR") ?? skills.First();
        var vocabSkill = skills.FirstOrDefault(s => s.SkillCode == "VOCABULARY") ?? skills.First();
        var listeningSkill = skills.FirstOrDefault(s => s.SkillCode == "LISTENING") ?? skills.First();
        var speakingSkill = skills.FirstOrDefault(s => s.SkillCode == "SPEAKING") ?? skills.First();
        var readingSkill = skills.FirstOrDefault(s => s.SkillCode == "READING") ?? skills.First();
        var intermediateLevel = levels.FirstOrDefault(l => l.Code == "INTERMEDIATE") ?? levels.First();
        var beginnerLevel = levels.FirstOrDefault(l => l.Code == "BEGINNER") ?? levels.First();
        var teacherUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "teacher@aistudyenglish.com");

        // 3. Seed 10 Courses (LearningTopics)
        var courses = new List<LearningTopic>();
        string[] courseTitles = new string[]
        {
            "IELTS 6.5 Overall Masterclass",
            "TOEIC 500+ Preparation Guide",
            "Tiếng Anh giao tiếp hàng ngày",
            "Ngữ pháp nền tảng vững chắc",
            "Luyện nghe chuyên sâu phản xạ",
            "Đọc hiểu nâng cao học thuật",
            "Phát âm chuẩn IPA tiếng Anh Mỹ",
            "Từ vựng IELTS theo 20 chủ đề",
            "Tiếng Anh giao tiếp trong công sở",
            "Viết luận tiếng Anh học thuật"
        };

        string[] courseCodes = new string[]
        {
            "COURSE_IELTS_65", "COURSE_TOEIC_500", "COURSE_COMM_DAILY",
            "COURSE_GRAM_FOUND", "COURSE_LIST_PRO", "COURSE_READ_ADV",
            "COURSE_PRON_IPA", "COURSE_VOCAB_IELTS", "COURSE_COMM_OFFICE",
            "COURSE_WRITE_ACAD"
        };

        for (int i = 0; i < 10; i++)
        {
            var code = courseCodes[i];
            var topic = await _context.LearningTopics.FirstOrDefaultAsync(t => t.TopicCode == code);
            if (topic == null)
            {
                topic = new LearningTopic
                {
                    TopicCode = code,
                    Title = courseTitles[i],
                    Description = $"Khóa học hoàn chỉnh về {courseTitles[i]} cung cấp đầy đủ bài học, ví dụ thực tế và quiz kiểm tra.",
                    SkillId = (i % 2 == 0) ? grammarSkill.Id : vocabSkill.Id,
                    LevelId = (i % 3 == 0) ? intermediateLevel.Id : beginnerLevel.Id,
                    DifficultyLevel = (i % 3 == 0) ? "INTERMEDIATE" : "BEGINNER",
                    Status = "ACTIVE",
                    OrderIndex = i + 1,
                    CreatedBy = teacherUser?.Id,
                    UpdatedBy = teacherUser?.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LearningTopics.Add(topic);
                await _context.SaveChangesAsync();
            }
            else if (teacherUser != null && topic.CreatedBy == null)
            {
                topic.CreatedBy = teacherUser.Id;
                topic.UpdatedBy = teacherUser.Id;
                topic.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            courses.Add(topic);
        }

        // 4. Seed 100 Lessons (OriginalLessons - 10 per Course)
        int lessonCount = await _context.OriginalLessons.CountAsync();
        if (lessonCount < 100)
        {
            int lessonsToCreate = 100 - lessonCount;
            int lessonIndex = 1;
            foreach (var course in courses)
            {
                var existingCount = await _context.OriginalLessons.CountAsync(l => l.TopicId == course.Id);
                for (int j = existingCount; j < 10; j++)
                {
                    var lesson = new OriginalLesson
                    {
                        TopicId = course.Id,
                        Title = $"Bài học {j + 1}: Chuyên đề nâng cao phần {j + 1} của {course.Title}",
                        Summary = $"Tóm tắt nội dung bài học số {j + 1} cho học viên.",
                        Content = $"<h3>Nội dung chính</h3><p>Đây là nội dung chi tiết bài học thứ {j + 1} trong khóa học {course.Title}. Học viên sẽ nắm được các cấu trúc câu cốt lõi và cách áp dụng thực tế.</p><h4>Ví dụ minh họa</h4><p><strong>Example:</strong> In many cultures, learning a second language is mandatory.<br><em>Dịch: Trong nhiều nền văn hóa, học ngôn ngữ thứ hai là bắt buộc.</em></p><h4>Tài nguyên đính kèm</h4><ul><li>Tài liệu PDF bài giảng: <a href='#'>Tải xuống PDF</a></li><li>Bản audio nghe: <a href='#'>Tải xuống MP3</a></li></ul>",
                        ContentType = "ARTICLE",
                        EstimatedMinutes = 15 + (j * 2),
                        SourceType = "SYSTEM",
                        ReviewStatus = "APPROVED",
                        IsAiGenerated = false,
                        CreatedBy = teacherUser?.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.OriginalLessons.Add(lesson);
                    lessonIndex++;
                }
            }
            await _context.SaveChangesAsync();
        }

        if (teacherUser != null)
        {
            var courseIds = courses.Select(c => c.Id).ToList();
            var ownerlessLessons = await _context.OriginalLessons
                .Where(l => courseIds.Contains(l.TopicId) && l.CreatedBy == null)
                .ToListAsync();
            foreach (var lesson in ownerlessLessons)
            {
                lesson.CreatedBy = teacherUser.Id;
                lesson.UpdatedAt = DateTime.UtcNow;
            }
            if (ownerlessLessons.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        // 5. Seed 30 Achievements
        if (!await _context.Achievements.AnyAsync())
        {
            var badgeDetails = new List<Achievement>();
            string[] badges = new string[]
            {
                "FIRST_STEP", "STREAK_3", "STREAK_7", "STREAK_30", "LESSON_5",
                "LESSON_10", "LESSON_50", "LESSON_100", "QUIZ_1", "QUIZ_10",
                "QUIZ_50", "QUIZ_MASTER", "VOCAB_KING", "GRAMMAR_HERO", "LISTENING_EAR",
                "SPEAKING_GOD", "READING_EYE", "WRITING_PEN", "XP_1000", "XP_5000",
                "XP_10000", "AI_CHAT_1", "AI_CHAT_10", "NIGHT_OWL", "EARLY_BIRD",
                "PERFECT_QUIZ", "LEVEL_5", "LEVEL_10", "FAST_LEARNER", "TOP_10"
            };

            string[] badgeNames = new string[]
            {
                "Khởi đầu mới", "3 ngày liên tiếp", "7 ngày liên tiếp", "Thần đồng chuyên cần", "Học viên năng nổ",
                "Vượt qua chính mình", "Chuyên cần vàng", "Bậc thầy lý thuyết", "Thử thách đầu tiên", "Trùm trắc nghiệm",
                "Kẻ chinh phục Quiz", "Quiz Master", "Vua Từ Vựng", "Anh Hùng Ngữ Pháp", "Đôi Tai Vàng",
                "Chiến Thần Nói", "Độc Giả Thông Thái", "Cây Bút Trẻ", "Tích Lũy 1000 XP", "Tích Lũy 5000 XP",
                "Huyền Thoại 10000 XP", "Trò chuyện AI", "Người bạn của Robot", "Cú đêm học tập", "Sớm tinh mơ",
                "Điểm tuyệt đối", "Đạt Level 5", "Đạt Level 10", "Học nhanh nhớ lâu", "Top 10 Cao Thủ"
            };

            string[] badgeDescs = new string[]
            {
                "Đăng nhập thành công lần đầu tiên.", "Học liên tục trong 3 ngày.", "Học liên tục trong 7 ngày.", "Học liên tục trong 30 ngày.", "Hoàn thành 5 bài học đầu tiên.",
                "Hoàn thành 10 bài học.", "Hoàn thành 50 bài học.", "Hoàn thành 100 bài học.", "Hoàn thành 1 bài trắc nghiệm.", "Hoàn thành 10 bài trắc nghiệm.",
                "Hoàn thành 50 bài trắc nghiệm.", "Đạt điểm tuyệt đối trong 5 bài Quiz liên tiếp.", "Học hết 100 từ vựng cốt lõi.", "Giải đúng 50 câu hỏi ngữ pháp khó.", "Đạt 100% điểm nghe.",
                "Thực hành giao tiếp với AI Tutor 10 lần.", "Đọc và trả lời đúng 20 bài đọc hiểu.", "Hoàn thành 5 bài viết luận học thuật.", "Đạt tổng tích lũy 1000 XP.", "Đạt tổng tích lũy 5000 XP.",
                "Đạt tổng tích lũy 10000 XP.", "Gửi tin nhắn đầu tiên cho AI Tutor.", "Trò chuyện với AI Tutor hơn 50 tin nhắn.", "Học tập vào khung giờ từ 12h đêm - 4h sáng.", "Học tập vào khung giờ từ 4h - 6h sáng.",
                "Đạt 10/10 điểm trong một bài trắc nghiệm.", "Đạt cấp độ 5 trên hệ thống.", "Đạt cấp độ 10 trên hệ thống.", "Hoàn thành bài học dưới thời gian dự kiến.", "Lọt vào bảng xếp hạng Top 10 cao thủ học thuật."
            };

            string[] icons = new string[]
            {
                "fa-flag", "fa-fire", "fa-calendar-check", "fa-crown", "fa-book-open",
                "fa-bookmark", "fa-award", "fa-graduation-cap", "fa-question-circle", "fa-tasks",
                "fa-trophy", "fa-star", "fa-font", "fa-spell-check", "fa-headphones",
                "fa-microphone", "fa-eye", "fa-pen-fancy", "fa-bolt", "fa-medal",
                "fa-gem", "fa-robot", "fa-comments", "fa-moon", "fa-sun",
                "fa-check-double", "fa-layer-group", "fa-chess-queen", "fa-running", "fa-users"
            };

            for (int k = 0; k < 30; k++)
            {
                var ach = new Achievement
                {
                    Code = badges[k],
                    Title = badgeNames[k],
                    Description = badgeDescs[k],
                    IconUrl = icons[k],
                    XpReward = 100 * (k % 5 + 1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Achievements.Add(ach);
            }
            await _context.SaveChangesAsync();
        }

        // 6. Link default achievements to users
        var student1 = await _context.Users.FirstOrDefaultAsync(u => u.Email == "student1@aistudyenglish.com");
        if (student1 != null)
        {
            var userAchs = await _context.UserAchievements.Where(ua => ua.UserId == student1.Id).ToListAsync();
            if (!userAchs.Any())
            {
                var allAchs = await _context.Achievements.ToListAsync();
                int idx = 0;
                foreach (var ach in allAchs)
                {
                    bool isUnlocked = idx < 8; // Unlock the first 8 achievements for student1
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = student1.Id,
                        AchievementId = ach.Id,
                        IsUnlocked = isUnlocked,
                        UnlockedAt = isUnlocked ? DateTime.UtcNow.AddDays(-idx) : null,
                        ProgressValue = isUnlocked ? 5 : (idx % 2 == 0 ? 3 : 0),
                        TargetValue = idx % 2 == 0 ? 10 : 5
                    });
                    idx++;
                }
                await _context.SaveChangesAsync();
            }
        }

        // 7. Seed 100 Quiz Questions
        int totalQuestions = await _context.QuestionBanks.CountAsync();
        if (totalQuestions < 100)
        {
            int index = 1;
            foreach (var course in courses)
            {
                // Create a Quiz for this course if it doesn't exist
                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.TopicId == course.Id);
                if (quiz == null)
                {
                    quiz = new Quiz
                    {
                        TopicId = course.Id,
                        SkillId = course.SkillId,
                        Title = $"Quiz: {course.Title}",
                        Description = $"Bài trắc nghiệm đánh giá kiến thức đã học trong {course.Title}.",
                        QuizType = "PRACTICE",
                        TimeLimitMinutes = 15,
                        PassingScore = 7.0m,
                        Status = "ACTIVE",
                        CreatedBy = teacherUser?.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Quizzes.Add(quiz);
                    await _context.SaveChangesAsync();
                }
                else if (teacherUser != null && quiz.CreatedBy == null)
                {
                    quiz.CreatedBy = teacherUser.Id;
                    await _context.SaveChangesAsync();
                }

                // Add 10 questions for this quiz
                var existingQs = await _context.QuizQuestions.CountAsync(qq => qq.QuizId == quiz.Id);
                for (int q = existingQs; q < 10; q++)
                {
                    var qb = new QuestionBank
                    {
                        TopicId = course.Id,
                        SkillId = course.SkillId,
                        QuestionType = "MCQ",
                        QuestionText = $"Đây là câu hỏi trắc nghiệm số {q + 1} của khóa học {course.Title}. Hãy chọn đáp án chính xác nhất?",
                        CorrectAnswer = "A",
                        Explanation = $"Giải thích chi tiết tại sao đáp án A là chính xác cho câu hỏi số {q + 1}. Cấu trúc ngữ pháp và từ vựng áp dụng ở đây rất thông dụng.",
                        DifficultyLevel = "INTERMEDIATE",
                        SourceType = "SYSTEM",
                        ReviewStatus = "APPROVED",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.QuestionBanks.Add(qb);
                    await _context.SaveChangesAsync();

                    var optA = new QuestionOption { QuestionId = qb.Id, OptionText = "Đáp án A (Chính xác)", IsCorrect = true, OrderIndex = 1 };
                    var optB = new QuestionOption { QuestionId = qb.Id, OptionText = "Đáp án B (Không chính xác)", IsCorrect = false, OrderIndex = 2 };
                    var optC = new QuestionOption { QuestionId = qb.Id, OptionText = "Đáp án C (Sai cấu trúc)", IsCorrect = false, OrderIndex = 3 };
                    var optD = new QuestionOption { QuestionId = qb.Id, OptionText = "Đáp án D (Thiếu từ)", IsCorrect = false, OrderIndex = 4 };

                    _context.QuestionOptions.AddRange(optA, optB, optC, optD);
                    await _context.SaveChangesAsync();

                    var qq = new QuizQuestion
                    {
                        QuizId = quiz.Id,
                        QuestionId = qb.Id,
                        Points = 1.0m,
                        OrderIndex = q + 1
                    };
                    _context.QuizQuestions.Add(qq);
                    await _context.SaveChangesAsync();

                    index++;
                }
            }
        }

        // 8. Seed 20 Chat Messages
        if (student1 != null)
        {
            var conv = await _context.AiTutorConversations.FirstOrDefaultAsync(c => c.StudentId == student1.Id);
            if (conv == null)
            {
                conv = new AiTutorConversation
                {
                    StudentId = student1.Id,
                    Title = "Conversation with AI Tutor",
                    Status = "ACTIVE",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.AiTutorConversations.Add(conv);
                await _context.SaveChangesAsync();
            }

            int msgCount = await _context.AiTutorMessages.CountAsync(m => m.ConversationId == conv.Id);
            if (msgCount < 20)
            {
                string[] studentMsgs = new string[]
                {
                    "Xin chào AI Tutor! Mình muốn học tiếng Anh giao tiếp.",
                    "Hôm nay mình muốn học cách giới thiệu bản thân.",
                    "Làm thế nào để nói về sở thích bằng tiếng Anh một cách tự nhiên?",
                    "Cho mình xin vài ví dụ về cấu trúc 'I am keen on'.",
                    "Thế còn 'I enjoy doing something' thì khác gì?",
                    "Giờ mình muốn học từ vựng về công việc.",
                    "Làm sao để nói 'Tôi đang tìm kiếm một cơ hội thử thách hơn'?",
                    "Có cấu trúc nào lịch sự hơn để viết trong email không?",
                    "Cảm ơn bạn! Hãy dịch giúp mình câu: 'Tôi mong nhận được phản hồi từ bạn'.",
                    "Rất hay! Mình sẽ ghi nhớ cấu trúc 'look forward to' này."
                };

                string[] aiMsgs = new string[]
                {
                    "Chào bạn! Rất vui được đồng hành cùng bạn trên con đường chinh phục tiếng Anh. Bạn muốn bắt đầu từ chủ đề nào?",
                    "Giới thiệu bản thân là chủ đề rất tuyệt. Bạn có thể bắt đầu với Tên, Tuổi, Công việc hiện tại và một câu ngắn về mục tiêu học tập nhé.",
                    "Để nói về sở thích tự nhiên, bạn hãy dùng các cụm như 'I'm into...', 'I enjoy...', hoặc 'I'm a big fan of...'. Tránh lặp đi lặp lại 'I like' nhé.",
                    "Cấu trúc 'keen on' dùng như sau: 'I am keen on reading books' hoặc 'She is keen on learning languages'. Sau 'keen on' là một V-ing hoặc danh từ.",
                    "Cả hai đều diễn tả sự yêu thích, nhưng 'keen on' mang sắc thái nhiệt huyết, đam mê hơn một chút so với 'enjoy' thông thường.",
                    "Chủ đề công việc rất thực tế! Một số từ khóa như: 'occupation', 'profession', 'colleague', 'deadline', 'work-life balance'.",
                    "Bạn có thể nói: 'I am currently seeking a more challenging opportunity.' Cụm 'seeking a challenging opportunity' nghe rất chuyên nghiệp.",
                    "Có chứ! Trong email công việc, thay vì 'I want...', bạn nên dùng 'I would appreciate it if you could...' hoặc 'Could you please...'.",
                    "Câu đó dịch chuẩn là: 'I look forward to hearing from you.' Hãy nhớ sau 'look forward to' là V-ing hoặc danh từ nhé.",
                    "Tuyệt vời! Bạn học rất nhanh. Hãy thử đặt một câu ví dụ với 'look forward to' cho mình xem nhé!"
                };

                for (int m = msgCount / 2; m < 10; m++)
                {
                    _context.AiTutorMessages.Add(new AiTutorMessage
                    {
                        ConversationId = conv.Id,
                        SenderType = "STUDENT",
                        MessageText = studentMsgs[m],
                        CreatedAt = DateTime.UtcNow.AddMinutes(-20 + m * 2)
                    });
                    _context.AiTutorMessages.Add(new AiTutorMessage
                    {
                        ConversationId = conv.Id,
                        SenderType = "AI",
                        MessageText = aiMsgs[m],
                        AiModel = "gemini-1.5-pro",
                        TokenUsage = 150,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-19 + m * 2)
                    });
                }
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task CreateTablesIfNotExistAsync()
    {
        // Execute Raw SQL to create tables if they do not exist
        string checkAchTableSql = "SELECT COUNT(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[achievements]') AND type in (N'U')";
        int achTableExists = 0;
        try
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = checkAchTableSql;
                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }
                achTableExists = (int)(command.ExecuteScalar() ?? 0);
            }
        }
        catch
        {
            // Fallback
        }

        if (achTableExists == 0)
        {
            string createAchievementsSql = @"
                CREATE TABLE [dbo].[achievements] (
                    [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [code] NVARCHAR(50) NOT NULL UNIQUE,
                    [title] NVARCHAR(255) NOT NULL,
                    [description] NVARCHAR(1000) NOT NULL,
                    [icon_url] NVARCHAR(255) NOT NULL,
                    [xp_reward] INT NOT NULL,
                    [is_active] BIT NOT NULL,
                    [created_at] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                );";

            string createUserAchievementsSql = @"
                CREATE TABLE [dbo].[user_achievements] (
                    [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [user_id] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[users]([id]) ON DELETE CASCADE,
                    [achievement_id] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[achievements]([id]) ON DELETE CASCADE,
                    [is_unlocked] BIT NOT NULL,
                    [unlocked_at] DATETIME2 NULL,
                    [progress_value] INT NOT NULL DEFAULT 0,
                    [target_value] INT NOT NULL DEFAULT 1
                );";

            await _context.Database.ExecuteSqlRawAsync(createAchievementsSql);
            await _context.Database.ExecuteSqlRawAsync(createUserAchievementsSql);
        }
    }
}
