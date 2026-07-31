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

        // 4. Seed & Update Rich Realistic Lessons for Courses (OriginalLessons)
        await SeedOrUpdateRichLessonsAsync(courses, teacherUser?.Id);

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
                        DifficultyLevel = "MEDIUM",
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

        // 9. Seed Teacher Profile and Settings
        if (teacherUser != null)
        {
            if (!await _context.UserProfiles.AnyAsync(p => p.UserId == teacherUser.Id))
            {
                _context.UserProfiles.Add(new UserProfile
                {
                    UserId = teacherUser.Id,
                    DateOfBirth = new DateOnly(1985, 10, 20),
                    Gender = "Male",
                    Country = "Vietnam",
                    Bio = "Giảng viên tiếng Anh với 10 năm kinh nghiệm luyện thi IELTS và TOEIC.",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (!await _context.UserSettings.AnyAsync(s => s.UserId == teacherUser.Id))
            {
                _context.UserSettings.Add(new UserSetting
                {
                    UserId = teacherUser.Id,
                    Language = "vi-VN",
                    Timezone = "Asia/Ho_Chi_Minh",
                    Theme = "light"
                });
            }
            await _context.SaveChangesAsync();

            // 10. Seed Dashboard Data for Teacher (Schedules, Practice Tasks, Submissions, Quiz Attempts)
            var course = courses.FirstOrDefault();
            if (course != null && student1 != null)
            {
                if (!await _context.Schedules.AnyAsync(s => s.TeacherId == teacherUser.Id))
                {
                    var today = DateTime.Today;
                    _context.Schedules.AddRange(
                        new Schedule { TeacherId = teacherUser.Id, TopicId = course.Id, Title = "Lớp Giao Tiếp Cơ Bản", StartTime = today.AddHours(9), EndTime = today.AddHours(11), Classroom = "Phòng 101", CreatedAt = DateTime.UtcNow },
                        new Schedule { TeacherId = teacherUser.Id, TopicId = course.Id, Title = "Lớp IELTS Nâng Cao", StartTime = today.AddHours(14), EndTime = today.AddHours(16), Classroom = "Phòng 205", CreatedAt = DateTime.UtcNow },
                        new Schedule { TeacherId = teacherUser.Id, TopicId = course.Id, Title = "Chữa bài TOEIC", StartTime = today.AddDays(1).AddHours(9), EndTime = today.AddDays(1).AddHours(11), Classroom = "Online", CreatedAt = DateTime.UtcNow }
                    );
                    await _context.SaveChangesAsync();
                }

                var task = await _context.PracticeTasks.FirstOrDefaultAsync(t => t.TopicId == course.Id);
                if (task == null)
                {
                    task = new PracticeTask { TopicId = course.Id, SkillId = grammarSkill.Id, Title = "Bài tập: Viết email", Instruction = "Viết email ứng tuyển", TaskType = "WRITING", DifficultyLevel = "BEGINNER", CreatedBy = teacherUser.Id, Status = "ACTIVE", CreatedAt = DateTime.UtcNow };
                    _context.PracticeTasks.Add(task);
                    await _context.SaveChangesAsync();
                }

                if (!await _context.PracticeSubmissions.AnyAsync(s => s.PracticeTaskId == task.Id))
                {
                    _context.PracticeSubmissions.AddRange(
                        new PracticeSubmission { PracticeTaskId = task.Id, StudentId = student1.Id, SubmissionText = "Dear Sir...", Status = "SUBMITTED", SubmittedAt = DateTime.UtcNow.AddHours(-2) },
                        new PracticeSubmission { PracticeTaskId = task.Id, StudentId = student1.Id, SubmissionText = "To whom it may concern...", Status = "GRADED", Score = 8.5m, SubmittedAt = DateTime.UtcNow.AddDays(-1) }
                    );
                    await _context.SaveChangesAsync();
                }

                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.CreatedBy == teacherUser.Id);
                if (quiz != null && !await _context.QuizAttempts.AnyAsync(qa => qa.QuizId == quiz.Id))
                {
                    _context.QuizAttempts.AddRange(
                        new QuizAttempt { QuizId = quiz.Id, StudentId = student1.Id, Status = "COMPLETED", Score = 8.5m, StartedAt = DateTime.UtcNow.AddDays(-1), SubmittedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(15) },
                        new QuizAttempt { QuizId = quiz.Id, StudentId = student1.Id, Status = "COMPLETED", Score = 9.0m, StartedAt = DateTime.UtcNow.AddHours(-5), SubmittedAt = DateTime.UtcNow.AddHours(-5).AddMinutes(12) }
                    );
                    await _context.SaveChangesAsync();
                }

                // 11. Seed Chat Messages and Attendances
                if (!await _context.ChatMessages.AnyAsync(m => m.SenderId == teacherUser.Id || m.ReceiverId == teacherUser.Id))
                {
                    _context.ChatMessages.AddRange(
                        new ChatMessage { SenderId = student1.Id, ReceiverId = teacherUser.Id, MessageText = "Thưa thầy, em có câu hỏi về bài học hôm nay ạ.", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-3) },
                        new ChatMessage { SenderId = student1.Id, ReceiverId = teacherUser.Id, MessageText = "Phần ngữ pháp câu điều kiện loại 2 em chưa hiểu rõ.", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-2) },
                        new ChatMessage { SenderId = teacherUser.Id, ReceiverId = student1.Id, MessageText = "Chào em, lát nữa thầy sẽ giải đáp nhé.", IsRead = true, CreatedAt = DateTime.UtcNow.AddHours(-1) }
                    );
                    await _context.SaveChangesAsync();
                }

                if (!await _context.Attendances.AnyAsync(a => a.TopicId == course.Id))
                {
                    var todayDate = DateOnly.FromDateTime(DateTime.Today);
                    _context.Attendances.AddRange(
                        new Attendance { StudentId = student1.Id, TopicId = course.Id, AttendanceDate = todayDate.AddDays(-2), Status = "PRESENT", Remarks = "Tham gia đầy đủ", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                        new Attendance { StudentId = student1.Id, TopicId = course.Id, AttendanceDate = todayDate.AddDays(-1), Status = "LATE", Remarks = "Muộn 15 phút", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                    );
                    await _context.SaveChangesAsync();
                }
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

    private async Task SeedOrUpdateRichLessonsAsync(List<LearningTopic> courses, int? teacherUserId)
    {
        var templates = GetCourseLessonTemplates();
        var allTopics = await _context.LearningTopics.ToListAsync();

        foreach (var course in allTopics)
        {
            List<(string Title, string Summary, string Content, string ContentType, string? VideoUrl)> lessonData;

            if (!string.IsNullOrEmpty(course.TopicCode) && templates.ContainsKey(course.TopicCode))
            {
                lessonData = templates[course.TopicCode];
            }
            else
            {
                // Custom dynamic fallback per course title so every course lesson is distinct
                lessonData = new List<(string Title, string Summary, string Content, string ContentType, string? VideoUrl)>
                {
                    ($"Bài 1: Khái niệm & Kiến thức nền tảng về {course.Title}",
                     $"Hiểu rõ định nghĩa, vai trò và phạm vi ứng dụng của {course.Title} trong tiếng Anh.",
                     $"<h3>1. Định nghĩa & Tầm quan trọng</h3><p>Khóa học <strong>{course.Title}</strong> đóng vai trò thiết yếu giúp nâng cao trình độ tiếng Anh. Thí sinh cần nắm vững các điểm cốt lõi trước khi thực hành sâu.</p><h4>2. Ví dụ áp dụng</h4><p><strong>Example:</strong> Mastering {course.Title} will significantly improve your language proficiency.<br><em>Dịch: Thành thạo {course.Title} sẽ giúp nâng cao đáng kể trình độ ngôn ngữ của bạn.</em></p>",
                     "ARTICLE", null),

                    ($"Bài 2: Từ vựng chuyên sâu & Mẫu câu thông dụng cho {course.Title}",
                     $"Tổng hợp 15 từ vựng ăn điểm và mẫu câu diễn đạt chuyên nghiệp.",
                     $"<h3>1. Bảng từ vựng trọng tâm</h3><ul><li><strong>Core Terminology:</strong> Các thuật ngữ cốt lõi liên quan đến {course.Title}.</li><li><strong>Collocations:</strong> Cụm từ hay đi kèm chuẩn người bản xứ.</li><li><strong>Common Phrases:</strong> Các câu mẫu ứng dụng linh hoạt.</li></ul>",
                     "ARTICLE", null),

                    ($"Bài 3: Thực hành phản xạ & Phân tích ngữ cảnh thực tế của {course.Title}",
                     $"Luyện tập phản xạ nghe nói và đọc hiểu theo ngữ cảnh thực tế.",
                     $"<h3>1. Bài tập phản xạ</h3><p>Hãy phân tích các tình huống giao tiếp và ngữ cảnh thực tế dưới đây để nâng cao phản xạ tự nhiên.</p><h4>Tình huống thực tế</h4><p>Nêu quan điểm và phản hồi các câu hỏi liên quan đến {course.Title}.</p>",
                     "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                    ($"Bài 4: Phân tích bẫy thường gặp & Chiến thuật làm bài {course.Title}",
                     $"Nhận diện các lỗi sai phổ biến và bí quyết xử lý bài thi hiệu quả.",
                     $"<h3>1. Các lỗi sai thường gặp</h3><p>Học viên thường mắc lỗi dùng sai ngữ cảnh hoặc nhầm lẫn cấu trúc khi áp dụng {course.Title}. Hãy chú ý các nguyên tắc quan trọng để tránh mất điểm.</p>",
                     "ARTICLE", null),

                    ($"Bài 5: Tổng ôn kiến thức & Kiểm tra thực hành {course.Title}",
                     $"Tổng kết toàn bộ nội dung khóa học và thực hành chuẩn bị cho bài Quiz.",
                     $"<h3>1. Tổng kết khóa học</h3><p>Chúc mừng bạn đã hoàn thành các nội dung chính của khóa học {course.Title}. Hãy kiểm tra lại kiến thức và sẵn sàng thử sức với bài Quiz.</p>",
                     "ARTICLE", null)
                };
            }

            var existingLessons = await _context.OriginalLessons
                .Where(l => l.TopicId == course.Id)
                .OrderBy(l => l.Id)
                .ToListAsync();

            for (int i = 0; i < lessonData.Count; i++)
            {
                var data = lessonData[i];
                if (i < existingLessons.Count)
                {
                    // Update existing lesson to rich content
                    var lesson = existingLessons[i];
                    lesson.Title = data.Title;
                    lesson.Summary = data.Summary;
                    lesson.Content = data.Content;
                    lesson.ContentType = data.ContentType;
                    lesson.VideoUrl = data.VideoUrl;
                    lesson.EstimatedMinutes = 15 + (i * 3);
                    lesson.ReviewStatus = "APPROVED";
                    if (teacherUserId.HasValue) lesson.CreatedBy = teacherUserId;
                    lesson.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new rich lesson
                    var newLesson = new OriginalLesson
                    {
                        TopicId = course.Id,
                        Title = data.Title,
                        Summary = data.Summary,
                        Content = data.Content,
                        ContentType = data.ContentType,
                        VideoUrl = data.VideoUrl,
                        EstimatedMinutes = 15 + (i * 3),
                        SourceType = "SYSTEM",
                        ReviewStatus = "APPROVED",
                        IsAiGenerated = false,
                        CreatedBy = teacherUserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.OriginalLessons.Add(newLesson);
                }
            }

            // Remove excessive boilerplate lessons beyond the rich data count if any
            if (existingLessons.Count > lessonData.Count)
            {
                var lessonsToRemove = existingLessons.Skip(lessonData.Count).ToList();
                _context.OriginalLessons.RemoveRange(lessonsToRemove);
            }
        }

        await _context.SaveChangesAsync();
    }

    private static Dictionary<string, List<(string Title, string Summary, string Content, string ContentType, string? VideoUrl)>> GetCourseLessonTemplates()
    {
        return new Dictionary<string, List<(string Title, string Summary, string Content, string ContentType, string? VideoUrl)>>
        {
            ["GRAM_TENSES"] = new()
            {
                ("Bài 1: Tổng quan 12 thì trong tiếng Anh & Sơ đồ thời gian (Timeline)",
                 "Phân loại 12 thì theo mốc thời gian (Quá khứ, Hiện tại, Tương lai) và thể (Đơn, Tiếp diễn, Hoàn thành).",
                 "<h3>1. Hệ thống 12 thì trong tiếng Anh</h3><p>12 thì được cấu tạo từ 3 mốc thời gian (Past, Present, Future) nhân với 4 thể (Simple, Continuous, Perfect, Perfect Continuous).</p><h4>2. Sơ đồ thời gian (Timeline)</h4><ul><li><strong>Past:</strong> Đã xảy ra và kết thúc trong quá khứ.</li><li><strong>Present:</strong> Đang diễn ra hoặc là sự thật ở hiện tại.</li><li><strong>Future:</strong> Sẽ xảy ra trong tương lai.</li></ul>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: Nhóm các thì Đơn (Present, Past, Future Simple)",
                 "Phân biệt cách dùng, công thức và dấu hiệu nhận biết của 3 thì đơn cơ bản.",
                 "<h3>1. So sánh 3 thì đơn</h3><p><strong>Present Simple:</strong> S + V(s/es) -> Thói quen, sự thật hiển nhiên.<br><strong>Past Simple:</strong> S + V2/ed -> Đã kết thúc ở quá khứ.<br><strong>Future Simple:</strong> S + will + V-bare -> Quyết định bộc phát.</p>",
                 "ARTICLE", null),

                ("Bài 3: Nhóm các thì Tiếp diễn (Continuous Tenses)",
                 "Diễn tả hành động đang diễn ra tại mốc thời gian xác định.",
                 "<h3>1. Công thức chung nhóm Tiếp diễn</h3><p>Tất cả các thì tiếp diễn đều có dạng <code>Be + V-ing</code> (Present: am/is/are, Past: was/were, Future: will be).</p>",
                 "ARTICLE", null),

                ("Bài 4: Nhóm các thì Hoàn thành (Perfect Tenses)",
                 "Diễn tả hành động hoàn thành trước một mốc thời gian hoặc hành động khác.",
                 "<h3>1. Công thức chung nhóm Hoàn thành</h3><p>Tất cả các thì hoàn thành đều dùng <code>Have/Has/Had + V3/ed</code>.</p>",
                 "ARTICLE", null)
            },

            ["GRAM_PRES_SIMP"] = new()
            {
                ("Bài 1: Công thức & Quy tắc chia Động từ To Be và Động từ thường",
                 "Nắm vững cấu trúc khẳng định, phủ định, nghi vấn của thì Hiện tại đơn.",
                 "<h3>1. Cấu trúc Thì Hiện tại đơn</h3><p><strong>To Be:</strong> S + am/is/are (+ not) + N/Adj.<br><strong>Động từ thường:</strong> S + V(s/es) (Phủ định: S + do/does not + V-bare).</p><h4>2. Quy tắc thêm -s/-es</h4><p>Thêm -es khi động từ kết thúc bằng các đuôi: <em>-o, -s, -ch, -x, -sh, -z</em> (ví dụ: go -> goes, watch -> watches).</p>",
                 "ARTICLE", null),

                ("Bài 2: Trạng từ chỉ tần suất & Dấu hiệu nhận biết",
                 "Vị trí của trạng từ chỉ tần suất (always, usually, often, sometimes, never) trong câu.",
                 "<h3>1. Vị trí Trạng từ chỉ tần suất</h3><p>Đứng TRƯỚC động từ thường, nhưng đứng SAU động từ To Be.<br><em>Example: She <strong>always gets up</strong> early. / He <strong>is usually</strong> busy.</em></p>",
                 "ARTICLE", null),

                ("Bài 3: Diễn tả Sự thật hiển nhiên, Lịch trình & Thói quen",
                 "Ứng dụng thì hiện tại đơn trong các tình huống thực tế đời sống.",
                 "<h3>1. Các trường hợp sử dụng chính</h3><ul><li><strong>Thói quen:</strong> I drink coffee every morning.</li><li><strong>Sự thật hiển nhiên:</strong> Water boils at 100°C.</li><li><strong>Lịch trình cố định:</strong> The plane takes off at 6 PM.</li></ul>",
                 "ARTICLE", null)
            },

            ["GRAM_PRES_CONT"] = new()
            {
                ("Bài 1: Cấu trúc & Quy tắc thêm đuôi -ing vào Động từ",
                 "Cấu trúc S + am/is/are + V-ing và các quy tắc nhân đôi phụ âm.",
                 "<h3>1. Công thức Present Continuous</h3><p>Khẳng định: <code>S + am/is/are + V-ing</code><br>Phủ định: <code>S + am/is/are + not + V-ing</code><br>Nghi vấn: <code>Am/Is/Are + S + V-ing?</code></p><h4>2. Quy tắc nhân đôi phụ âm cuối</h4><p>Động từ 1 âm tiết kết thúc bằng 1 nguyên âm + 1 phụ âm -> Nhân đôi phụ âm (run -> running, sit -> sitting).</p>",
                 "ARTICLE", null),

                ("Bài 2: Diễn tả Kế hoạch dự định tương lai & Sự phàn nàn với 'Always'",
                 "Sử dụng thì tiếp diễn cho lịch trình cá nhân và hành động gây phiền hà lặp đi lặp lại.",
                 "<h3>1. Diễn tả kế hoạch tương lai gần</h3><p><em>'We are meeting the doctor tomorrow afternoon.' (Kế hoạch đã hẹn trước)</em></p><h4>2. Diễn tả sự phàn nàn với 'Always'</h4><p><em>'You are always forgetting your keys!' (Phàn nàn thói quen hay quên).</em></p>",
                 "ARTICLE", null),

                ("Bài 3: Danh sách Động từ chỉ Trạng thái (Stative Verbs) không chia V-ing",
                 "Phân biệt các động từ cảm xúc, nhận thức không dùng ở thì tiếp diễn.",
                 "<h3>1. Các Stative Verbs phổ biến</h3><p><em>like, love, hate, want, need, know, understand, believe, remember, belong.</em></p>",
                 "ARTICLE", null)
            },

            ["VOCAB_FAMILY"] = new()
            {
                ("Bài 1: Từ vựng các thế hệ trong Gia đình (Immediate & Extended Family)",
                 "Học từ vựng chỉ người thân trong gia đình nhỏ và đại gia đình.",
                 "<h3>1. Immediate Family (Gia đình nhỏ)</h3><p>Parents (bố mẹ), siblings (anh chị em), spouse (vợ/chồng), daughter/son (con gái/con trai).</p><h4>2. Extended Family (Đại gia đình)</h4><p>Grandparents (ông bà), aunt (cô/dì/bác gái), uncle (chú/bác/cậu), cousin (anh chị em họ), nephew/niece (cháu trai/cháu gái).</p>",
                 "ARTICLE", null),

                ("Bài 2: Thành ngữ & Collocations về tình cảm gia đình",
                 "Các cụm từ hay nói về mối quan hệ và truyền thống gia đình.",
                 "<h3>1. Idioms & Collocations</h3><ul><li><strong>Blood is thicker than water:</strong> Một giọt máu đào hơn ao nước đục.</li><li><strong>Take after someone:</strong> Giống ai đó trong gia đình.</li><li><strong>Run in the family:</strong> Di truyền trong gia đình.</li><li><strong>Bring up / Raise children:</strong> Nuôi nấng con cái.</li></ul>",
                 "ARTICLE", null),

                ("Bài 3: Luyện nói chủ đề Family trong giao tiếp",
                 "Mẫu câu miêu tả tính cách, ngoại hình và kỷ niệm gia đình.",
                 "<h3>1. Mẫu câu giao tiếp</h3><p><em>'I come from a nuclear family of four.' / 'I take after my father in personality.' / 'We always gather for dinner on weekends.'</em></p>",
                 "ARTICLE", null)
            },

            ["VOCAB_SCHOOL"] = new()
            {
                ("Bài 1: Từ vựng Môn học, Môi trường Lớp học & Dụng cụ",
                 "Từ vựng các môn học và thiết bị trường học.",
                 "<h3>1. Subjects (Môn học)</h3><p>Mathematics (Toán), Physics (Vật lý), Chemistry (Hóa học), Biology (Sinh học), Literature (Ngữ văn), History (Lịch sử).</p><h4>2. Classroom vocabulary</h4><p>Blackboard, projector, textbook, assignment, lecture hall, laboratory.</p>",
                 "ARTICLE", null),

                ("Bài 2: Từ vựng Hệ thống Giáo dục & Kỳ thi (Exams & Degrees)",
                 "Thuật ngữ điểm số, bằng cấp, học bổng và các kỳ thi.",
                 "<h3>1. Academic Terms</h3><p>GPA (Điểm trung bình), Tuition fees (Học phí), Scholarship (Học bổng), Diploma / Degree (Bằng cấp), Academic transcript (Bảng điểm).</p>",
                 "ARTICLE", null),

                ("Bài 3: Hội thoại giao tiếp với Giáo viên & Bạn bè ở trường",
                 "Mẫu câu xin phép nghỉ học, hỏi bài giảng và thảo luận bài tập nhóm.",
                 "<h3>1. Useful Dialogues</h3><p><em>'Could you please clarify the homework deadline?' / 'Let me share my ideas for our group presentation.'</em></p>",
                 "ARTICLE", null)
            },

            ["VOCAB_TRAVEL"] = new()
            {
                ("Bài 1: Từ vựng Phương tiện giao thông & Làm thủ tục tại Sân bay",
                 "Từ vựng thủ tục bay, vé máy bay, hành lý và bảng hiệu sân bay.",
                 "<h3>1. Airport Vocabulary</h3><p>Boarding pass (Thẻ lên máy bay), Passport control (Kiểm tra hộ chiếu), Baggage claim (Nơi nhận hành lý), Departure lounge (Phòng chờ xuất phát), Gate (Cổng lên xe/máy bay).</p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: Từ vựng Đặt phòng Khách sạn & Các loại hình Du lịch",
                 "Phân biệt các loại phòng, dịch vụ lưu trú và loại hình du lịch.",
                 "<h3>1. Accommodation Types</h3><p>Single room, Double room, Suite, Hostel, Resort. Services: Room service, Complimentary breakfast, Late check-out.</p>",
                 "ARTICLE", null),

                ("Bài 3: Giao tiếp Hỏi thông tin điểm đến & Xử lý sự cố khi Du lịch",
                 "Mẫu câu hỏi bản đồ, xin tư vấn tour và xử lý thất lạc hành lý.",
                 "<h3>1. Handling Emergencies</h3><p><em>'Excuse me, I seem to have lost my passport.' / 'Could you point me towards the nearest tourist information center?'</em></p>",
                 "ARTICLE", null)
            },
            ["COURSE_IELTS_65"] = new()
            {
                ("Bài 1: IELTS Listening Part 1 - Chiến thuật xử lý Form-filling",
                 "Phương pháp nhận biết bẫy phát âm số, tên riêng và điền từ chính xác trong Listening Part 1.",
                 "<h3>1. Tổng quan dạng bài Form-filling</h3><p>Trong IELTS Listening Part 1, thí sinh thường phải lắng nghe một cuộc điện thoại giao dịch (đặt phòng, đăng ký khóa học, báo mất đồ) và điền thông tin vào mẫu đơn.</p><h4>2. Các bẫy phát âm phổ biến</h4><ul><li><strong>Con số 13 vs 30:</strong> Chú ý trọng âm (thirTEEN nhấn âm 2, THIRty nhấn âm 1).</li><li><strong>Chữ cái dễ nhầm:</strong> A /eɪ/ vs E /iː/, G /dʒiː/ vs J /dʒeɪ/.</li><li><strong>Mã bưu điện / SĐT:</strong> 'O' đọc là 'zero' hoặc 'oh', số kép đọc là 'double'.</li></ul><h4>3. Bài tập minh họa & Ví dụ</h4><p><strong>Example Dialogue:</strong><br>A: Could I take your full name, please?<br>B: Yes, it's Sarah Jenkins. That's J-E-N-K-I-N-S.<br>A: And your contact number?<br>B: It's 07700 900345.<br><em>Dịch: Tên tôi là Sarah Jenkins (J-E-N-K-I-N-S), số điện thoại 07700 900345.</em></p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: IELTS Reading - Kỹ thuật Skimming & Scanning chuẩn xác",
                 "Chiến thuật đọc lướt và tìm từ khóa giúp tiết kiệm 50% thời gian làm bài Reading.",
                 "<h3>1. Skimming vs Scanning</h3><p>Skimming là kỹ thuật đọc lướt nhanh để nắm ý chính của đoạn văn (Main Idea), trong khi Scanning là đọc quét để tìm thông tin cụ thể (tên riêng, mốc năm, con số).</p><h4>2. Các bước thực hiện Skimming</h4><ul><li>Đọc câu đầu tiên (Topic Sentence) và câu kết luận của từng đoạn.</li><li>Chú ý các từ nối chuyển ý: <em>However, Consequently, On the other hand</em>.</li><li>Không dừng lại ở từ mới, tập trung vào mạch logic chung.</li></ul><h4>3. Ví dụ bài đọc Academic</h4><p><strong>Passage:</strong> The rapid expansion of artificial intelligence has transformed modern industries. While proponents argue that automation enhances efficiency, skeptics express concern over potential job displacement.<br><em>Dịch: Sự phát triển nhanh chóng của AI đã thay đổi các ngành công nghiệp. Trong khi người ủng hộ cho rằng tự động hóa tăng hiệu suất, người hoài nghi lo ngại mất việc làm.</em></p>",
                 "ARTICLE", null),

                ("Bài 3: IELTS Writing Task 1 - Cấu trúc mô tả biểu đồ đường (Line Graph)",
                 "Cách lập dàn ý 4 đoạn tiêu chuẩn, các cấu trúc mô tả xu hướng tăng/giảm và biến động.",
                 "<h3>1. Bố cục 4 đoạn chuẩn Task 1</h3><p><strong>Đoạn 1:</strong> Introduction (Paraphrase đề bài)<br><strong>Đoạn 2:</strong> Overview (Tóm tắt 2-3 điểm nổi bật nhất)<br><strong>Đoạn 3 & 4:</strong> Body 1 & Body 2 (Chi tiết số liệu theo nhóm).</p><h4>2. Từ vựng mô tả xu hướng</h4><ul><li><strong>Tăng:</strong> Increase, rise, soar, surge, climb steadily.</li><li><strong>Giảm:</strong> Decrease, decline, drop, plummet, fall sharply.</li><li><strong>Dao động:</strong> Fluctuate, experience a period of volatility.</li></ul><h4>3. Bài mẫu Band 7.5+</h4><p><em>'The line graph illustrates the consumption of three types of fast food in the UK between 1970 and 1990. Overall, hamburger and pizza experienced a dramatic upward trend, whereas fish and chips saw a significant decline...'</em></p>",
                 "ARTICLE", null),

                ("Bài 4: IELTS Speaking Part 2 - Kỹ thuật mở rộng ý tưởng Describe a Person",
                 "Cấu trúc PPF (Past-Present-Future) và dàn ý mindmap trả lời mượt mà trong 2 phút.",
                 "<h3>1. Dàn ý chuẩn cho Part 2</h3><p>Khi nhận thẻ đề bài (Cue Card) về người (thầy cô, người bạn, người nổi tiếng), áp dụng công thức 4 câu hỏi: Who - When/Where - What they do - Why you admire them.</p><h4>2. Cấu trúc PPF (Quá khứ - Hiện tại - Tương lai)</h4><ul><li><strong>Past:</strong> I first met him 5 years ago when I was in high school.</li><li><strong>Present:</strong> Currently, he is working as a senior software engineer.</li><li><strong>Future:</strong> In the future, I hope we can collaborate on a project together.</li></ul><h4>3. Từ vựng ăn điểm (Collocations & Idioms)</h4><p><em>Down-to-earth (khiêm tốn), a pillar of strength (trụ cột tinh thần), set a good example (nêu gương tốt).</em></p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 5: IELTS Writing Task 2 - Dạng bài Opinion Essay (Agree or Disagree)",
                 "Chiến thuật mở bài paraphrase, phát triển ý tưởng logic ở 2 thân bài và kết bài thuyết phục.",
                 "<h3>1. Cấu trúc bài luận Opinion Essay</h3><p>Dạng bài yêu cầu bạn đưa ra quan điểm cá nhân rõ ràng: Hoàn toàn đồng ý (Completely Agree), Hoàn toàn không đồng ý, hoặc Đồng ý một phần (Partially Agree).</p><h4>2. Dàn bài chi tiết</h4><ul><li><strong>Mở bài:</strong> Paraphrase lại đề bài + Thesis Statement rõ quan điểm.</li><li><strong>Thân bài 1:</strong> Phân tích lý do thứ nhất kèm ví dụ chứng minh.</li><li><strong>Thân bài 2:</strong> Phân tích lý do thứ hai kèm dẫn chứng thực tế.</li><li><strong>Kết bài:</strong> Khẳng định lại quan điểm và tóm tắt ý chính.</li></ul><h4>3. Các cụm từ nối ăn điểm Cohesion</h4><p><em>It is widely believed that... / From my perspective, / One compelling reason is that... / Consequently, ...</em></p>",
                 "ARTICLE", null)
            },

            ["COURSE_TOEIC_500"] = new()
            {
                ("Bài 1: TOEIC Part 1 - Bẫy hình ảnh mô tả người & vật thường gặp",
                 "Nhận diện thì tiếp diễn (Is being V3) và từ vựng trọng tâm mô tả tranh trong TOEIC Listening Part 1.",
                 "<h3>1. Phân biệt 'Is Being V3' vs 'Has Been V3'</h3><p>Trong Part 1, nếu nghe <em>'The car is being repaired'</em> thì bức tranh PHẢI có người thợ đang sửa xe. Nếu chỉ có chiếc xe đậu trong xưởng mà không có người làm việc, đáp án này là BẪY SAI.</p><h4>2. Từ vựng hành động phổ biến trong tranh</h4><ul><li><strong>Carrying:</strong> Bưng, bê, mang vác đồ đạc.</li><li><strong>Inspecting / Examining:</strong> Kiểm tra, xem xét thiết bị.</li><li><strong>Adjusting:</strong> Điều chỉnh kính, máy móc, vô lăng.</li><li><strong>Patronizing:</strong> Ghé thăm, sử dụng dịch vụ cửa hàng.</li></ul>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: TOEIC Part 2 - Mẹo phân biệt câu hỏi Wh-Questions & Yes/No",
                 "Nhận diện loại câu hỏi, mẹo loại trừ đáp án lặp từ và bẫy đồng âm.",
                 "<h3>1. Quy tắc vàng Part 2</h3><p>Với câu hỏi Wh- (Who, When, Where, Why, How), đáp án bắt đầu bằng <em>Yes/No</em> chắc chắn SAI 100%.</p><h4>2. Bẫy lặp từ (Same Word Trap)</h4><p>Nếu trong câu trả lời xuất hiện lại chính xác 1 từ trong câu hỏi, khả năng cao 80% đó là bẫy đồng âm nhiễu. Hãy chọn đáp án dùng từ đồng nghĩa (Synonyms).</p>",
                 "ARTICLE", null),

                ("Bài 3: TOEIC Part 5 - Quy tắc xác định Từ loại (Parts of Speech) trong 5 giây",
                 "Xác định Danh từ, Tính từ, Trạng từ bằng đuôi từ và vị trí ngữ pháp trong câu.",
                 "<h3>1. Vị trí Tính từ (Adjective) & Trạng từ (Adverb)</h3><ul><li><strong>Tính từ:</strong> Đứng trước Danh từ (a <em>successful</em> project) hoặc sau động từ Tobe / Linking Verbs (look, seem, remain).</li><li><strong>Trạng từ:</strong> Đứng trước/sau Động từ thường (work <em>efficiently</em>), đứng trước Tính từ (an <em>extremely</em> high rate).</li></ul><h4>2. Đuôi từ thường gặp</h4><p><strong>Nouns:</strong> -tion, -ment, -ance, -ence, -ity, -ship.<br><strong>Adjectives:</strong> -able, -ible, -ive, -ous, -ful, -al.<br><strong>Adverbs:</strong> -ly.</p>",
                 "ARTICLE", null),

                ("Bài 4: TOEIC Part 5 - Ngữ pháp Thì Động từ & Thể bị động công sở",
                 "Mẹo nhận biết thì qua dấu hiệu thời gian và thể bị động trong các văn bản thương mại.",
                 "<h3>1. Thể bị động trong văn bản TOEIC</h3><p>Cấu trúc: <code>S + Be + V3/ed + (by O)</code>. Khi sau khoảng trống KHÔNG có tân ngữ (Noun), 80% đáp án là Thể bị động.</p><h4>2. Từ nhận biết thì thông dụng</h4><p><em>Recently / Lately (Hiện tại hoàn thành), Currently / At present (Hiện tại tiếp diễn), Upcoming / Next week (Tương lai đơn).</em></p>",
                 "ARTICLE", null),

                ("Bài 5: TOEIC Part 7 - Đọc hiểu Email & Thư giao dịch thương mại",
                 "Cấu trúc thư điện tử công sở và mẹo tìm thông tin người gửi, mục đích thư.",
                 "<h3>1. Cấu trúc chuẩn của Business Email</h3><ul><li><strong>Header:</strong> From, To, Subject, Date.</li><li><strong>Opening:</strong> Dear Mr. Smith, / Dear Hiring Manager,</li><li><strong>Purpose:</strong> I am writing to inquire about... / I would like to confirm...</li><li><strong>Action/Request:</strong> Please reply by Friday...</li></ul><h4>2. Các câu hỏi đọc hiểu thường gặp</h4><p><em>What is the main purpose of the email? / What is Mr. Brown asked to do? / What is implied about the company?</em></p>",
                 "ARTICLE", null)
            },

            ["COURSE_COMM_DAILY"] = new()
            {
                ("Bài 1: Chào hỏi & Giới thiệu bản thân tự nhiên (Greetings & Small Talk)",
                 "Các mẫu câu chào hỏi tự nhiên của người bản xứ và cách duy trì cuộc trò chuyện ban đầu.",
                 "<h3>1. Các cách chào hỏi tự nhiên</h3><ul><li><strong>Informal:</strong> Hey! How's it going? / What's up? / How have you been?</li><li><strong>Formal:</strong> Good morning/afternoon. Nice to meet you.</li></ul><h4>2. Cách đáp lại lời chào</h4><p><em>'Pretty good, thanks! How about you?' / 'Can't complain! Busy as usual.'</em></p><h4>3. Các chủ đề Small Talk an toàn</h4><p>Thời tiết (Weather), kế hoạch cuối tuần (Weekend plans), giao thông (Traffic), công việc (Work).</p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: Gọi món & Thanh toán tại nhà hàng, quán café (Ordering Food & Drinks)",
                 "Mẫu câu đặt bàn, gọi món, yêu cầu chỉnh sửa món ăn và thanh toán hóa đơn.",
                 "<h3>1. Mẫu câu gọi đồ uống / món ăn</h3><ul><li>Could I get an Iced Americano with less ice, please?</li><li>I'd like to order the grilled chicken salad.</li><li>Is this dish vegetarian-friendly?</li></ul><h4>2. Yêu cầu thanh toán</h4><p><em>'Could we get the bill, please?' / 'Can I pay by credit card or cash?'</em></p>",
                 "ARTICLE", null),

                ("Bài 3: Hỏi đường & Chỉ hướng đi chi tiết (Asking & Giving Directions)",
                 "Từ vựng vị trí, ngã tư, vòng xoay và các mẫu câu hướng dẫn đường đi chuẩn xác.",
                 "<h3>1. Mẫu câu hỏi đường lịch sự</h3><p><em>'Excuse me, could you tell me how to get to the nearest metro station?' / 'Is there a bank around here?'</em></p><h4>2. Từ vựng chỉ hướng đi</h4><ul><li><strong>Go straight ahead:</strong> Đi thẳng về phía trước.</li><li><strong>Take the second left:</strong> Rẽ trái ở ngã rẽ thứ hai.</li><li><strong>At the roundabout:</strong> Tại vòng xoay / vòng xuyến.</li><li><strong>Cross the intersection:</strong> Băng qua ngã tư.</li></ul>",
                 "ARTICLE", null),

                ("Bài 4: Mua sắm & Trả giá hàng hóa (Shopping & Bargaining)",
                 "Mẫu câu hỏi giá, thử đồ, yêu cầu đổi trả và thương lượng giá cả khi mua sắm.",
                 "<h3>1. Hỏi thông tin sản phẩm & Thử đồ</h3><p><em>'Do you have this shirt in size M?' / 'Where is the fitting room?' / 'How much does this cost?'</em></p><h4>2. Mẫu câu trả giá lịch sự</h4><p><em>'Is there any discount if I buy two?' / 'Can you make it a bit cheaper?'</em></p>",
                 "ARTICLE", null),

                ("Bài 5: Đặt phòng khách sạn & Đặt vé du lịch (Booking & Reservations)",
                 "Các mẫu câu check-in, check-out khách sạn và giải quyết yêu cầu phòng ở.",
                 "<h3>1. Check-in khách sạn</h3><p><em>'Hi, I have a reservation under the name of John Smith for 3 nights.'</em></p><h4>2. Yêu cầu dịch vụ phòng</h4><p><em>'Could I get extra towels in room 302?' / 'What time is breakfast served?'</em></p>",
                 "ARTICLE", null)
            },

            ["COURSE_GRAM_FOUND"] = new()
            {
                ("Bài 1: Thì Hiện tại đơn & Hiện tại tiếp diễn (Present Simple vs Continuous)",
                 "Phân biệt thói quen lặp lại và hành động đang diễn ra tại thời điểm nói.",
                 "<h3>1. Công thức & Cách dùng Present Simple</h3><p><code>S + V(s/es)</code>. Dùng cho sự thật hiển nhiên, thói quen hằng ngày.</p><h4>2. Công thức & Cách dùng Present Continuous</h4><p><code>S + am/is/are + V-ing</code>. Dùng cho hành động đang diễn ra ngay lúc nói.</p><h4>3. State Verbs (Động từ chỉ trạng thái KHÔNG chia V-ing)</h4><p><em>know, believe, understand, love, hate, want, prefer, remember.</em></p>",
                 "ARTICLE", null),

                ("Bài 2: Thì Quá đơn & Quá cảnh hoàn thành (Past Simple vs Present Perfect)",
                 "Phân biệt mốc thời gian xác định trong quá khứ và hành động kéo dài đến hiện tại.",
                 "<h3>1. Past Simple (Quá đơn)</h3><p>Dùng cho sự việc đã kết thúc hoàn toàn trong quá khứ tại mốc thời gian rõ ràng (yesterday, last year, in 2010).</p><h4>2. Present Perfect (Hiện tại hoàn thành)</h4><p>Dùng cho hành động bắt đầu trong quá khứ kéo dài đến hiện tại, hoặc trải nghiệm vừa mới xảy ra (since, for, ever, never, already, yet).</p>",
                 "ARTICLE", null),

                ("Bài 3: Thì Tương lai đơn & Tương lai gần (Will vs Be Going To)",
                 "Phân biệt quyết định bộc phát tại thời điểm nói và kế hoạch đã dự định trước.",
                 "<h3>1. Will + V-bare</h3><p>Quyết định tức thì tại thời điểm nói, lời hứa, dự đoán không có căn cứ rõ ràng.</p><h4>2. Be going to + V-bare</h4><p>Kế hoạch đã được dự định/chuẩn bị từ trước, dự đoán có dấu hiệu trực quan rõ ràng.</p>",
                 "ARTICLE", null),

                ("Bài 4: Các loại Danh từ, Đại từ & Quy tắc Số nhiều / Số ít",
                 "Danh từ đếm được vs Không đếm được, sở hữu cách và đại từ phản thân.",
                 "<h3>1. Countable vs Uncountable Nouns</h3><p><strong>Countable:</strong> book -> books, apple -> apples.<br><strong>Uncountable:</strong> water, information, advice, money, furniture (không thêm 's').</p>",
                 "ARTICLE", null),

                ("Bài 5: Mệnh đề quan hệ (Relative Clauses - Who, Which, That, Whose)",
                 "Cách nối 2 câu đơn thành câu phức với đại từ quan hệ chuẩn xác.",
                 "<h3>1. Chức năng của các Đại từ quan hệ</h3><ul><li><strong>Who:</strong> Thay cho Người (đóng vai trò Chủ ngữ/Tân ngữ).</li><li><strong>Which:</strong> Thay cho Vật / Sự việc.</li><li><strong>That:</strong> Thay cho cả Người và Vật (dùng trong mệnh đề xác định).</li><li><strong>Whose:</strong> Diễn tả sở hữu (whose + Noun).</li></ul>",
                 "ARTICLE", null)
            },

            ["COURSE_LIST_PRO"] = new()
            {
                ("Bài 1: Nhận diện hiện tượng Nối âm (Linking Sounds) trong giao tiếp",
                 "Kỹ thuật nối phụ âm sang nguyên âm và âm nối tự nhiên của người bản xứ.",
                 "<h3>1. Phụ âm nối Nguyên âm (Consonant to Vowel)</h3><p>Khi một từ kết thúc bằng phụ âm và từ tiếp theo bắt đầu bằng nguyên âm, phụ âm sẽ nối liền sang nguyên âm đó.</p><p><strong>Ví dụ:</strong><br><em>Check it out</em> -> /tʃekɪtaʊt/<br><em>Turn off</em> -> /tɜːnɒf/<br><em>Hold on</em> -> /həʊldɒn/</p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: Kỹ thuật nuốt âm & âm câm (Assimilation & Elision)",
                 "Nhận biết âm bị nuốt hoặc bị biến đổi trong bài nghe tốc độ nhanh.",
                 "<h3>1. Hiện tượng nuốt âm (Elision)</h3><p>Âm /t/ và /d/ thường bị nuốt khi đứng giữa hai phụ âm khác.</p><p><em>Next door</em> -> /neks dɔːr/<br><em>Last night</em> -> /lɑːs naɪt/<br><em>You and me</em> -> /juː ən miː/</p>",
                 "ARTICLE", null),

                ("Bài 3: Nghe bắt từ khóa quan trọng (Keywords Extraction)",
                 "Kỹ năng lọc từ mang nội dung (Content Words) và bỏ qua từ chức năng (Function Words).",
                 "<h3>1. Content Words vs Function Words</h3><p><strong>Content Words (Từ mang nghĩa):</strong> Danh từ, Động từ chính, Tính từ, Trạng từ -> Được nhấn mạnh trong câu.<br><strong>Function Words (Từ ngữ pháp):</strong> Tobe, Giới từ, Trợ động từ, Mạo từ -> Đọc lướt nhẹ.</p>",
                 "ARTICLE", null),

                ("Bài 4: Phân biệt âm Anh - Anh (BrE) và Anh - Mỹ (AmE)",
                 "Các điểm khác biệt chính về âm /r/, âm /t/ (Flap T) và giọng đọc vùng miền.",
                 "<h3>1. Phụ âm /r/ trong AmE vs BrE</h3><p>Người Mỹ phát âm /r/ rất rõ (Rhotic), người Anh thường đọc âm câm /r/ trừ khi đứng trước nguyên âm.</p><h4>2. Âm Flap T trong tiếng Anh Mỹ</h4><p>Âm /t/ đứng giữa 2 nguyên âm đọc thành /d/ nhẹ (water -> /wɑːdər/, butter -> /bʌdər/).</p>",
                 "ARTICLE", null),

                ("Bài 5: Luyện nghe qua bài hát & Phim ảnh thực tế",
                 "Ứng dụng nghe phim Sitcom và nhạc Pop để tăng phản xạ tự nhiên.",
                 "<h3>1. Phương pháp Shadowing qua phim</h3><p>Nghe từng câu ngắn, dừng lại và nhại lại đúng nữ điệu, tốc độ và cảm xúc của nhân vật trong phim.</p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ")
            },

            ["COURSE_READ_ADV"] = new()
            {
                ("Bài 1: Phân tích cấu trúc đoạn văn Academic & Ý chính (Main Idea)",
                 "Nhận biết Topic Sentence, Supporting Sentences và Conclusion trong bài đọc.",
                 "<h3>1. Cấu trúc đoạn văn học thuật chuẩn</h3><p>Đoạn văn học thuật thường đi từ tổng quát (Topic Sentence) đến chi tiết (Examples & Data) và tóm tắt (Concluding Sentence).</p>",
                 "ARTICLE", null),

                ("Bài 2: Kỹ năng Đoán nghĩa từ mới dựa vào Ngữ cảnh (Context Clues)",
                 "Dùng định nghĩa, từ trái nghĩa và ví dụ xung quanh để đoán nghĩa từ vựng khó.",
                 "<h3>1. Các loại Context Clues</h3><ul><li><strong>Definition Clue:</strong> Từ mới được định nghĩa ngay sau dấu phẩy hoặc từ 'means', 'refers to'.</li><li><strong>Contrast Clue:</strong> Từ mới đứng cạnh các từ nối trái ngược 'unlike', 'whereas', 'however'.</li></ul>",
                 "ARTICLE", null),

                ("Bài 3: Phân tích các mối quan hệ Logic (Cause & Effect, Contrast)",
                 "Nhận diện các từ liên kết thể hiện mối quan hệ nguyên nhân - kết quả.",
                 "<h3>1. Từ nối Nguyên nhân & Kết quả</h3><p><em>Due to, As a result, Consequently, Leads to, Stems from.</em></p>",
                 "ARTICLE", null),

                ("Bài 4: Nhận diện thái độ & Quan điểm tác giả (Author's Tone)",
                 "Xác định văn phong khách quan, ủng hộ hay hoài nghi của tác giả bài viết.",
                 "<h3>1. Từ vựng chỉ thái độ tác giả</h3><p><em>Objective (Khách quan), Optimistic (Lạc quan), Skeptical (Hoài nghi), Critical (Phê bình), Neutral (Trung lập).</em></p>",
                 "ARTICLE", null),

                ("Bài 5: Đọc hiểu văn bản Khoa học & Công nghệ hiện đại",
                 "Phân tích các bài báo học thuật chủ đề Công nghệ sinh học và Trí tuệ nhân tạo.",
                 "<h3>1. Từ vựng chuyên ngành công nghệ</h3><p><em>Algorithm (Thuật toán), Breakthrough (Đột phá), Automation (Tự động hóa), Neural network (Mạng thần kinh nhân tạo).</em></p>",
                 "ARTICLE", null)
            },

            ["COURSE_PRON_IPA"] = new()
            {
                ("Bài 1: Tổng quan bảng phiên âm quốc tế IPA & Khẩu hình miệng",
                 "Hiểu 44 âm trong bảng IPA (20 nguyên âm và 24 phụ âm).",
                 "<h3>1. Cấu trúc bảng IPA</h3><p>Bảng IPA gồm 12 nguyên âm đơn (Monophthongs), 8 nguyên âm đôi (Diphthongs) và 24 phụ âm (Consonants).</p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 2: Phân biệt Nguyên âm ngắn & Nguyên âm dài (Short vs Long Vowels)",
                 "Luyện tập các cặp âm /iː/ vs /ɪ/, /uː/ vs /ʊ/, /ɑː/ vs /ʌ/.",
                 "<h3>1. Cặp âm /iː/ và /ɪ/</h3><p><strong>/iː/ (Dài):</strong> Môi kéo rộng sang 2 bên như đang mỉm cười (Sheep /ʃiːp/, Seat /siːt/).<br><strong>/ɪ/ (Ngắn):</strong> Phát âm nhanh, thả lỏng môi (Ship /ʃɪp/, Sit /sɪt/).</p>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 3: Cặp phụ âm dễ nhầm lẫn /θ/ và /ð/ (Think vs This)",
                 "Kỹ thuật đặt lưỡi giữa 2 răng để phát âm chuẩn âm 'th'.",
                 "<h3>1. Phát âm /θ/ (Unvoiced TH)</h3><p>Đặt đầu lưỡi giữa hai hàm răng, thổi hơi ra ngoài không làm rung dây thanh quản (Think, Thank, Thin, Three).</p><h4>2. Phát âm /ð/ (Voiced TH)</h4><p>Đặt đầu lưỡi giữa hai hàm răng, thổi hơi và làm RUNG dây thanh quản (This, That, These, Mother).</p>",
                 "ARTICLE", null),

                ("Bài 4: Trọng âm từ (Word Stress) & Quy tắc nhấn giọng",
                 "Nhận biết trọng âm danh từ 2 âm tiết (nhấn âm 1) và động từ 2 âm tiết (nhấn âm 2).",
                 "<h3>1. Quy tắc cơ bản</h3><ul><li><strong>Danh từ 2 âm tiết:</strong> Thường nhấn âm 1 (PREsent, EXport, TAble).</li><li><strong>Động từ 2 âm tiết:</strong> Thường nhấn âm 2 (preSENT, exPORT, deCIDE).</li></ul>",
                 "ARTICLE", null),

                ("Bài 5: Phát âm đuôi -ed và -s/es chuẩn người bản xứ",
                 "Quy tắc phát âm -ed thành /t/, /d/, /ɪd/ và -s/es thành /s/, /z/, /ɪz/.",
                 "<h3>1. Quy tắc phát âm đuôi -ed</h3><ul><li><strong>/ɪd/:</strong> Kết thúc bằng /t/ hoặc /d/ (wanted, needed).</li><li><strong>/t/:</strong> Kết thúc bằng phụ âm vô thanh /p, k, f, s, ʃ, tʃ/ (stopped, walked, washed).</li><li><strong>/d/:</strong> Các trường hợp còn lại (played, cleaned, loved).</li></ul>",
                 "ARTICLE", null)
            },

            ["COURSE_VOCAB_IELTS"] = new()
            {
                ("Bài 1: Topic Environment & Climate Change (Môi trường & Biến đổi khí hậu)",
                 "Bộ từ vựng và Collocations Band 7.0+ về biến đổi khí hậu và ô nhiễm.",
                 "<h3>1. Topic Vocabulary</h3><ul><li><strong>Global warming:</strong> Sự nóng lên toàn cầu.</li><li><strong>Greenhouse gas emissions:</strong> Khí thải nhà kính.</li><li><strong>Deforestation:</strong> Nạn phá rừng.</li><li><strong>Renewable energy:</strong> Năng lượng tái tạo (solar, wind power).</li><li><strong>Biodiversity loss:</strong> Sự mất đa dạng sinh học.</li></ul><h4>2. Cụm từ hay (Collocations)</h4><p><em>Exert a detrimental impact on (Gây ảnh hưởng xấu đến) / Implement sustainable policies (Thực thi chính sách bền vững).</em></p>",
                 "ARTICLE", null),

                ("Bài 2: Topic Education & Learning Technologies (Giáo dục & Công nghệ học)",
                 "Từ vựng chủ đề giáo dục hiện đại, học trực tuyến và kỹ năng tương lai.",
                 "<h3>1. Topic Vocabulary</h3><ul><li><strong>Distance learning / Online education:</strong> Học từ xa.</li><li><strong>Curriculum:</strong> Chương trình giảng dạy.</li><li><strong>Critical thinking skills:</strong> Tư duy phản biện.</li><li><strong>Tertiary education:</strong> Giáo dục đại học/cao đẳng.</li></ul>",
                 "ARTICLE", null),

                ("Bài 3: Topic Technology & Artificial Intelligence (Công nghệ & AI)",
                 "Từ vựng về tự động hóa, trí tuệ nhân tạo và tác động xã hội.",
                 "<h3>1. Topic Vocabulary</h3><ul><li><strong>Artificial Intelligence (AI):</strong> Trí tuệ nhân tạo.</li><li><strong>Automation:</strong> Tự động hóa.</li><li><strong>Cutting-edge technology:</strong> Công nghệ tiên tiến/hiện đại nhất.</li><li><strong>Technological breakthrough:</strong> Đột phá công nghệ.</li></ul>",
                 "ARTICLE", null),

                ("Bài 4: Topic Health, Fitness & Modern Lifestyle (Sức khỏe & Lối sống)",
                 "Từ vựng y tế, chế độ ăn uống lành mạnh và bệnh lý thời hiện đại.",
                 "<h3>1. Topic Vocabulary</h3><ul><li><strong>Sedentary lifestyle:</strong> Lối sống ít vận động.</li><li><strong>Balanced diet:</strong> Chế độ ăn uống cân bằng.</li><li><strong>Mental health awareness:</strong> Nhận thức sức khỏe tinh thần.</li><li><strong>Preventative medicine:</strong> Y học phòng ngừa.</li></ul>",
                 "ARTICLE", null),

                ("Bài 5: Topic Work, Employment & Career Path (Công việc & Sự nghiệp)",
                 "Từ vựng về thị trường lao động, cơ hội thăng tiến và cân bằng cuộc sống.",
                 "<h3>1. Topic Vocabulary</h3><ul><li><strong>Work-life balance:</strong> Cân bằng công việc và cuộc sống.</li><li><strong>Career prospects:</strong> Triển vọng nghề nghiệp.</li><li><strong>Job satisfaction:</strong> Sự hài lòng trong công việc.</li><li><strong>Remote working / Telecommuting:</strong> Làm việc từ xa.</li></ul>",
                 "ARTICLE", null)
            },

            ["COURSE_COMM_OFFICE"] = new()
            {
                ("Bài 1: Viết Email công việc chuyên nghiệp (Business Email Writing)",
                 "Mẫu câu mở đầu, trình bày yêu cầu và kết thúc Email đối ngoại chỉn chu.",
                 "<h3>1. Mẫu câu viết Email công sở</h3><p><strong>Mở đầu:</strong> I hope this email finds you well. / Thank you for reaching out to us.<br><strong>Nêu lý do:</strong> I am writing to inform you that... / As discussed in our meeting...<br><strong>Kết thúc:</strong> Please let me know if you have any questions. Best regards,</p>",
                 "ARTICLE", null),

                ("Bài 2: Tham gia cuộc họp & Đóng góp ý kiến (Active Meeting Participation)",
                 "Mẫu câu ngắt lời lịch sự, xin phát biểu và tổng kết ý kiến họp nhóm.",
                 "<h3>1. Mẫu câu xin phát biểu lịch sự</h3><p><em>'May I jump in here for a second?' / 'I'd like to add something to that point.'</em></p><h4>2. Đồng ý & Phản biện trong cuộc họp</h4><p><em>'I completely agree with Alex's proposal.' / 'That's a valid point, but have we considered the cost?'</em></p>",
                 "ARTICLE", null),

                ("Bài 3: Thuyết trình dự án trước đồng nghiệp & Đối tác (Business Presentation)",
                 "Cấu trúc bài thuyết trình 3 phần và các câu chuyển slide chuyên nghiệp.",
                 "<h3>1. Cấu trúc bài thuyết trình</h3><ul><li><strong>Introduction:</strong> Today I'd like to present our Q3 marketing strategy.</li><li><strong>Transitions:</strong> Moving on to the next slide... / Turning our attention to...</li><li><strong>Conclusion:</strong> To wrap up, here are the key takeaways...</li></ul>",
                 "VIDEO_LINK", "https://www.youtube.com/embed/dQw4w9WgXcQ"),

                ("Bài 4: Đàm phán thương lượng hợp đồng (Negotiation Skills)",
                 "Mẫu câu đưa ra đề xuất, thương lượng giá cả và đạt thỏa thuận chung.",
                 "<h3>1. Mẫu câu đàm phán</h3><p><em>'We are willing to accept your terms provided that...' / 'Could you meet us halfway on this price?' / 'We have a deal!'</em></p>",
                 "ARTICLE", null),

                ("Bài 5: Phỏng vấn xin việc bằng tiếng Anh (Job Interview Skills)",
                 "Trả lời câu hỏi phỏng vấn phổ biến bằng phương pháp STAR (Situation-Task-Action-Result).",
                 "<h3>1. Phương pháp STAR trả lời phỏng vấn</h3><p><strong>S (Situation):</strong> Bối cảnh tình huống.<br><strong>T (Task):</strong> Nhiệm vụ cần xử lý.<br><strong>A (Action):</strong> Hành động cụ thể bạn đã thực hiện.<br><strong>R (Result):</strong> Kết quả tích cực đạt được.</p>",
                 "ARTICLE", null)
            },

            ["COURSE_WRITE_ACAD"] = new()
            {
                ("Bài 1: Cấu trúc một đoạn văn học thuật chuẩn (Academic Paragraph Structure)",
                 "Cách viết Topic Sentence, Supporting Sentences và Concluding Sentence.",
                 "<h3>1. Công thức đoạn văn T.E.E.L</h3><ul><li><strong>T - Topic Sentence:</strong> Câu chủ đề nêu ý chính.</li><li><strong>E - Explanation:</strong> Giải thích làm rõ ý chính.</li><li><strong>E - Evidence / Example:</strong> Dẫn chứng hoặc ví dụ minh họa.</li><li><strong>L - Link:</strong> Câu kết liên kết sang ý tiếp theo.</li></ul>",
                 "ARTICLE", null),

                ("Bài 2: Viết câu Luận đề sắc bén (Thesis Statement)",
                 "Cách viết câu Thesis Statement thể hiện quan điểm cốt lõi của toàn bộ bài luận.",
                 "<h3>1. Tầm quan trọng của Thesis Statement</h3><p>Thesis statement thường nằm ở cuối đoạn mở bài, tóm tắt quan điểm luận điểm chính mà cả bài luận sẽ chứng minh.</p>",
                 "ARTICLE", null),

                ("Bài 3: Sử dụng Từ nối liên kết ý logic (Cohesion & Coherence Markers)",
                 "Danh sách từ nối bổ sung, đối lập, nguyên nhân và kết luận trong văn viết học thuật.",
                 "<h3>1. Các nhóm từ nối học thuật</h3><ul><li><strong>Bổ sung:</strong> Furthermore, Moreover, In addition, Additionally.</li><li><strong>Đối lập:</strong> However, Nevertheless, On the contrary, Conversely.</li><li><strong>Kết luận:</strong> In conclusion, To sum up, Overall, Consequently.</li></ul>",
                 "ARTICLE", null),

                ("Bài 4: Kỹ thuật Paraphrase & Trích dẫn nguồn tài liệu (Quoting & Citations)",
                 "Cách đổi từ đồng nghĩa, thay đổi cấu trúc câu và tránh lỗi đạo văn (Plagiarism).",
                 "<h3>1. 3 Kỹ thuật Paraphrase hàng đầu</h3><ul><li><strong>1. Dùng từ đồng nghĩa (Synonyms):</strong> Replace key words.</li><li><strong>2. Đổi thể (Active -> Passive):</strong> Change grammar structure.</li><li><strong>3. Đổi từ loại (Word Family):</strong> Change noun to verb/adjective.</li></ul>",
                 "ARTICLE", null),

                ("Bài 5: Phong cách viết khách quan Học thuật (Academic Tone & Formal Register)",
                 "Tránh viết tắt, tránh dùng đại từ nhân xưng xưng 'I/you' và từ ngữ suồng sã.",
                 "<h3>1. Nguyên tắc viết Academic</h3><ul><li>Không dùng viết tắt (dùng <em>cannot</em> thay vì <em>can't</em>).</li><li>Hạn chế dùng từ xưng hô thân mật (I, you, we).</li><li>Dùng các cấu trúc khách quan: <em>It is observed that... / Evidence suggests that...</em></li></ul>",
                 "ARTICLE", null)
            }
        };
    }
}

