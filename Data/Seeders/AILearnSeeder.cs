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

        // 11. Seed 3 Tenses Courses Sequence (Khóa A -> Khóa B -> Khóa C with Prerequisites)
        await SeedTenses3CoursesSequenceAsync(grammarSkill, beginnerLevel, intermediateLevel, teacherUser?.Id);
    }

    private async Task SeedTenses3CoursesSequenceAsync(
        EnglishSkill grammarSkill,
        EnglishProficiencyLevel beginnerLevel,
        EnglishProficiencyLevel intermediateLevel,
        int? teacherUserId)
    {
        // 1. Khóa A - Tenses Nền Tảng (A1)
        var courseA = await _context.LearningTopics.FirstOrDefaultAsync(t => t.TopicCode == "COURSE_TENSES_A");
        if (courseA == null)
        {
            courseA = new LearningTopic
            {
                TopicCode = "COURSE_TENSES_A",
                Title = "Khóa A: Thì Hiện Tại & Quá Khứ Cơ Bản",
                Description = "Khóa học nền tảng cung cấp toàn bộ kiến thức về Present Simple và Past Simple. Dành cho mọi học viên bắt đầu.",
                SkillId = grammarSkill.Id,
                LevelId = beginnerLevel.Id,
                DifficultyLevel = "BEGINNER",
                Status = "ACTIVE",
                OrderIndex = 101,
                CreatedBy = teacherUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LearningTopics.Add(courseA);
            await _context.SaveChangesAsync();
        }

        // 2. Khóa B - Tenses Nâng Cao (A2 - Gated by Competency >= 70%)
        var courseB = await _context.LearningTopics.FirstOrDefaultAsync(t => t.TopicCode == "COURSE_TENSES_B");
        if (courseB == null)
        {
            courseB = new LearningTopic
            {
                TopicCode = "COURSE_TENSES_B",
                Title = "Khóa B: Các Thì Tiếp Diễn & Hoàn Thành Nâng Cao",
                Description = "Khóa học mở rộng về Continuous & Perfect Tenses. Yêu cầu đạt tối thiểu 70% ở Khóa A.",
                SkillId = grammarSkill.Id,
                LevelId = intermediateLevel.Id,
                DifficultyLevel = "INTERMEDIATE",
                Status = "ACTIVE",
                OrderIndex = 102,
                CreatedBy = teacherUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LearningTopics.Add(courseB);
            await _context.SaveChangesAsync();
        }

        // Prerequisite: Course B requires Course A
        if (!await _context.TopicPrerequisites.AnyAsync(tp => tp.TopicId == courseB.Id && tp.PrerequisiteTopicId == courseA.Id))
        {
            _context.TopicPrerequisites.Add(new TopicPrerequisite
            {
                TopicId = courseB.Id,
                PrerequisiteTopicId = courseA.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // 3. Khóa C - Tenses Chuyên Sâu (B1)
        var courseC = await _context.LearningTopics.FirstOrDefaultAsync(t => t.TopicCode == "COURSE_TENSES_C");
        if (courseC == null)
        {
            courseC = new LearningTopic
            {
                TopicCode = "COURSE_TENSES_C",
                Title = "Khóa C: Ứng Dụng 12 Thì Trong Giao Tiếp Học Thuật",
                Description = "Làm chủ và kết hợp linh hoạt 12 thì tiếng Anh trong văn cảnh học thuật và thi cử. Yêu cầu hoàn thành Khóa B.",
                SkillId = grammarSkill.Id,
                LevelId = intermediateLevel.Id,
                DifficultyLevel = "ADVANCED",
                Status = "ACTIVE",
                OrderIndex = 103,
                CreatedBy = teacherUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LearningTopics.Add(courseC);
            await _context.SaveChangesAsync();
        }

        // Prerequisite: Course C requires Course B
        if (!await _context.TopicPrerequisites.AnyAsync(tp => tp.TopicId == courseC.Id && tp.PrerequisiteTopicId == courseB.Id))
        {
            _context.TopicPrerequisites.Add(new TopicPrerequisite
            {
                TopicId = courseC.Id,
                PrerequisiteTopicId = courseB.Id,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        await SeedTensesCourseLessonsAsync(courseA, courseB, courseC, teacherUserId);
    }

    private async Task SeedTensesCourseLessonsAsync(
        LearningTopic courseA,
        LearningTopic courseB,
        LearningTopic courseC,
        int? teacherUserId)
    {
        var lessonsByCourse = new Dictionary<int, (string Title, string Summary, string Content)[]>
        {
            [courseA.Id] = new[]
            {
                ("A1. Present Simple: Thói quen và sự thật", "Dùng Present Simple để nói về thói quen, lịch trình và sự thật hiển nhiên.", """
                <h3>Mục tiêu</h3><p>Sau bài học, bạn có thể mô tả lịch sinh hoạt, nói về sự thật và hỏi đáp về thói quen hằng ngày.</p>
                <h3>Công thức</h3><ul><li>Khẳng định: <strong>S + V/V(s/es) + O</strong>.</li><li>Phủ định: <strong>S + do/does not + V nguyên mẫu</strong>.</li><li>Nghi vấn: <strong>Do/Does + S + V nguyên mẫu?</strong></li></ul>
                <p>Với <em>he, she, it</em>, động từ thường thêm <strong>-s</strong> hoặc <strong>-es</strong>: work → works, watch → watches, study → studies.</p>
                <h3>Khi nào sử dụng?</h3><ol><li>Thói quen: <em>I review vocabulary every evening.</em></li><li>Sự thật: <em>Water boils at 100 degrees Celsius.</em></li><li>Lịch trình cố định: <em>The class starts at 8 a.m.</em></li></ol>
                <h3>Dấu hiệu nhận biết</h3><p><em>always, usually, often, sometimes, rarely, never, every day, on Mondays</em>.</p>
                <h3>Lỗi thường gặp</h3><p><strong>Sai:</strong> She go to school every day. <strong>Đúng:</strong> She goes to school every day.</p><p><strong>Sai:</strong> Does he likes music? <strong>Đúng:</strong> Does he like music?</p>
                <h3>Tự luyện</h3><ol><li>Viết 5 câu mô tả lịch học của bạn.</li><li>Đổi 2 câu sang dạng phủ định.</li><li>Đặt 3 câu hỏi dùng Do/Does và tự trả lời.</li></ol>
                """),
                ("A2. Past Simple: Hành động đã kết thúc", "Kể lại hành động đã xảy ra và kết thúc trong quá khứ.", """
                <h3>Mục tiêu</h3><p>Bạn có thể kể lại một ngày đã qua, một chuyến đi hoặc một sự kiện đã kết thúc.</p>
                <h3>Công thức</h3><ul><li>Khẳng định: <strong>S + V2/ed + O</strong>.</li><li>Phủ định: <strong>S + did not + V nguyên mẫu</strong>.</li><li>Nghi vấn: <strong>Did + S + V nguyên mẫu?</strong></li></ul>
                <p>Động từ có quy tắc thêm <strong>-ed</strong>: play → played, visit → visited. Động từ bất quy tắc cần học theo nhóm: go → went, buy → bought, see → saw, take → took.</p>
                <h3>Cách dùng</h3><p>Dùng Past Simple khi hành động đã hoàn tất tại một thời điểm xác định: <em>We visited Hue last summer.</em> / <em>She finished the report yesterday.</em></p>
                <h3>Dấu hiệu nhận biết</h3><p><em>yesterday, last night, last week, two days ago, in 2020, when I was a child</em>.</p>
                <h3>Phát âm đuôi -ed</h3><p>Đọc /t/ sau âm vô thanh như <em>watched</em>, /d/ sau âm hữu thanh như <em>played</em>, và /ɪd/ sau âm /t/ hoặc /d/ như <em>wanted</em>.</p>
                <h3>Lỗi thường gặp</h3><p><strong>Sai:</strong> She did not went home. <strong>Đúng:</strong> She did not go home.</p><p><strong>Sai:</strong> Did you visited Da Nang? <strong>Đúng:</strong> Did you visit Da Nang?</p>
                <h3>Tự luyện</h3><p>Viết một đoạn 80 từ về ngày hôm qua. Gạch chân các từ chỉ thời gian và kiểm tra mỗi động từ đã chia đúng chưa.</p>
                """),
                ("A3. Phân biệt Present Simple và Past Simple", "Chọn đúng thì khi kể thói quen hiện tại và sự kiện đã hoàn thành.", """
                <h3>Nguyên tắc chọn thì</h3><p>Hãy hỏi: hành động có lặp lại hoặc còn đúng ở hiện tại không? Nếu có, dùng Present Simple. Hành động đã xảy ra và kết thúc tại một thời điểm trong quá khứ thì dùng Past Simple.</p>
                <table class="table table-bordered"><thead><tr><th>Present Simple</th><th>Past Simple</th></tr></thead><tbody><tr><td>I work on Mondays.</td><td>I worked last Monday.</td></tr><tr><td>She lives in Hanoi.</td><td>She lived in Hanoi in 2020.</td></tr><tr><td>They play football every week.</td><td>They played football yesterday.</td></tr></tbody></table>
                <h3>Hội thoại mẫu</h3><p><strong>A:</strong> What do you usually do after class?<br><strong>B:</strong> I usually review my notes.<br><strong>A:</strong> What did you do yesterday?<br><strong>B:</strong> I reviewed the lesson and completed the exercises.</p>
                <h3>Chiến lược làm bài</h3><ol><li>Tìm trạng từ thời gian.</li><li>Xác định hành động là thói quen hay sự kiện đơn lẻ.</li><li>Kiểm tra chủ ngữ và dạng động từ.</li><li>Với câu hỏi có did/does, dùng động từ nguyên mẫu.</li></ol>
                <h3>Bài tổng hợp</h3><p>Viết 6 câu: 3 câu về thói quen hiện tại và 3 câu về một ngày trong quá khứ. Sau đó đổi một câu mỗi nhóm sang dạng phủ định.</p>
                """)
            },
            [courseB.Id] = new[]
            {
                ("B1. Present Continuous trong tình huống thực tế", "Mô tả hành động đang diễn ra và kế hoạch gần trong tương lai.", """
                <h3>Cấu trúc</h3><ul><li>Khẳng định: <strong>S + am/is/are + V-ing</strong>.</li><li>Phủ định: <strong>S + am/is/are not + V-ing</strong>.</li><li>Nghi vấn: <strong>Am/Is/Are + S + V-ing?</strong></li></ul>
                <h3>Cách dùng</h3><p>Dùng cho hành động đang xảy ra: <em>Look! The students are taking a test.</em>; hoạt động tạm thời: <em>I am staying with my aunt this week.</em>; kế hoạch đã sắp xếp: <em>We are meeting the teacher tomorrow.</em></p>
                <h3>Quy tắc thêm -ing</h3><p>make → making, run → running, lie → lying. Không tự động thêm -ing cho các động từ chỉ trạng thái như <em>know, believe, understand, need</em>.</p>
                <h3>Phân biệt nhanh</h3><p><em>I work from home</em> nói về thói quen; <em>I am working from home today</em> nói về tình huống tạm thời hôm nay.</p>
                <h3>Thực hành</h3><p>Viết 4 câu về những việc đang xảy ra quanh bạn và 2 câu về kế hoạch cuối tuần.</p>
                """),
                ("B2. Present Perfect: Kinh nghiệm và kết quả", "Nối một hành động trong quá khứ với hiện tại bằng already, yet, just, for và since.", """
                <h3>Công thức</h3><p><strong>S + have/has + V3</strong>. Phủ định dùng <strong>have/has not + V3</strong>, câu hỏi đảo <strong>Have/Has</strong> lên đầu.</p>
                <h3>Ba cách dùng chính</h3><ol><li>Kinh nghiệm: <em>Have you ever visited Singapore?</em></li><li>Kết quả hiện tại: <em>I have lost my key, so I cannot open the door.</em></li><li>Hành động bắt đầu trong quá khứ và còn tiếp diễn: <em>She has lived here since 2022.</em></li></ol>
                <h3>Từ đi kèm</h3><p><em>already</em> thường dùng trong câu khẳng định, <em>yet</em> trong phủ định/nghi vấn, <em>just</em> cho việc vừa xảy ra, <em>for</em> đi với khoảng thời gian, <em>since</em> đi với mốc bắt đầu.</p>
                <h3>Present Perfect và Past Simple</h3><p><em>I have seen that film</em> không nói thời điểm cụ thể. <em>I saw that film last Friday</em> có thời điểm đã kết thúc nên dùng Past Simple.</p>
                <h3>Thực hành</h3><p>Viết 3 câu về kinh nghiệm, 2 câu dùng for/since và 2 câu so sánh với Past Simple.</p>
                """),
                ("B3. Past Continuous và Past Perfect", "Mô tả bối cảnh đang diễn ra và hành động xảy ra trước một hành động khác trong quá khứ.", """
                <h3>Past Continuous</h3><p>Công thức <strong>was/were + V-ing</strong>, dùng để mô tả bối cảnh hoặc hành động đang diễn ra tại một thời điểm quá khứ: <em>I was reading when he called.</em></p>
                <h3>Past Perfect</h3><p>Công thức <strong>had + V3</strong>, dùng cho hành động xảy ra trước một hành động khác trong quá khứ: <em>They had left before we arrived.</em></p>
                <h3>When và while</h3><p><em>When</em> thường nối một hành động ngắn với một hành động đang diễn ra. <em>While</em> nhấn mạnh hai hành động cùng diễn ra: <em>While I was cooking, my brother was setting the table.</em></p>
                <h3>Dòng thời gian</h3><p><strong>Past Perfect</strong> → hành động sớm hơn; <strong>Past Simple</strong> → hành động sau; <strong>Past Continuous</strong> → bối cảnh đang diễn ra.</p>
                <h3>Thực hành</h3><p>Viết một câu dùng <em>when</em>, một câu dùng <em>while</em> và một câu dùng <em>before</em> để kể lại một sự cố trong ngày hôm qua.</p>
                """)
            },
            [courseC.Id] = new[]
            {
                ("C1. Hệ thống 12 thì trong văn cảnh học thuật", "Chọn thì theo quan hệ thời gian, tiến trình và kết quả trong văn viết học thuật.", """
                <h3>Khung tư duy 4 nhóm</h3><p>Trước khi chọn thì, xác định hành động thuộc nhóm <strong>Simple</strong> (sự thật/sự kiện), <strong>Continuous</strong> (đang diễn ra), <strong>Perfect</strong> (đã hoàn tất hoặc có kết quả) hay <strong>Perfect Continuous</strong> (nhấn mạnh thời lượng).</p>
                <h3>Văn phong học thuật</h3><p>Present Simple dùng cho nhận định và sự thật: <em>This study examines language anxiety.</em> Present Perfect dùng cho nghiên cứu trước đây có liên hệ hiện tại: <em>Researchers have identified a trend.</em> Past Simple dùng cho phương pháp hoặc kết quả của một nghiên cứu cụ thể: <em>The team collected data in 2024.</em></p>
                <h3>Phân tích đoạn văn</h3><p><em>Since the project began, the team has improved the process. Last year, it tested a new model, and it is now preparing a larger study.</em> Câu đầu dùng Present Perfect vì dự án bắt đầu trước đây và còn liên quan hiện tại; câu hai dùng Past Simple vì có mốc last year; mệnh đề cuối dùng Present Continuous vì đang chuẩn bị.</p>
                <h3>Bài tập</h3><p>Đọc một đoạn báo cáo ngắn, gạch chân động từ và ghi lý do tác giả chọn mỗi thì. Sau đó viết lại đoạn đó ở một mốc thời gian khác.</p>
                """),
                ("C2. Perfect Continuous và sắc thái thời lượng", "Dùng các thì hoàn thành tiếp diễn để nhấn mạnh thời lượng và nguyên nhân của kết quả.", """
                <h3>Cấu trúc</h3><ul><li>Present Perfect Continuous: <strong>have/has been + V-ing</strong>.</li><li>Past Perfect Continuous: <strong>had been + V-ing</strong>.</li><li>Future Perfect Continuous: <strong>will have been + V-ing</strong>.</li></ul>
                <h3>Sắc thái nghĩa</h3><p>Perfect nhấn mạnh kết quả: <em>The team has completed the survey.</em> Perfect Continuous nhấn mạnh quá trình hoặc thời lượng: <em>The team has been collecting data for six months.</em></p>
                <h3>Trong báo cáo</h3><p>Dùng cấu trúc này khi muốn giải thích nguyên nhân của một kết quả hiện tại: <em>The machine is hot because it has been running all morning.</em> Với Past Perfect Continuous: <em>The participants had been working for two hours before the break.</em></p>
                <h3>Lưu ý</h3><p>Không dùng dạng tiếp diễn tự nhiên với mọi động từ. Các động từ trạng thái như <em>own, know, understand, believe</em> thường dùng dạng Simple hoặc Perfect.</p>
                <h3>Thực hành</h3><p>Viết một đoạn 100 từ mô tả tiến độ dự án. Phải dùng ít nhất một câu Present Perfect, một câu Present Perfect Continuous và giải thích sự khác nhau.</p>
                """),
                ("C3. Tổng hợp thì trong bài viết và báo cáo", "Vận dụng linh hoạt 12 thì để mô tả quy trình, xu hướng, nghiên cứu và giả thuyết.", """
                <h3>Đoạn báo cáo mẫu</h3><p><em>Since the project began, the team has improved the process. Last year, it tested a new model, and it is now preparing a larger study. By the end of next year, the researchers will have collected enough evidence to compare the two approaches.</em></p>
                <h3>Cách kiểm tra bài viết</h3><ol><li>Khoanh các mốc thời gian và từ nối.</li><li>Vẽ dòng thời gian cho các hành động.</li><li>Kiểm tra sự hòa hợp chủ ngữ - động từ.</li><li>Kiểm tra V2/V3 và trợ động từ.</li><li>Đảm bảo việc đổi thì có lý do về thời gian hoặc ý nghĩa.</li></ol>
                <h3>Lỗi nâng cao</h3><p>Tránh dùng Present Perfect với thời điểm đã kết thúc như <em>yesterday</em>. Tránh dùng Past Simple khi muốn nhấn mạnh kết quả còn ảnh hưởng hiện tại. Không đổi thì tùy ý giữa các câu trong cùng một mốc thời gian.</p>
                <h3>Nhiệm vụ cuối khóa</h3><p>Viết báo cáo 150 từ về quá trình học tiếng Anh của bạn, gồm: thói quen hiện tại, một thay đổi trong quá khứ, thành quả đến hiện tại và kế hoạch sắp tới. Đánh dấu ít nhất 8 động từ và giải thích lựa chọn thì.</p>
                """)
            }
        };

        foreach (var entry in lessonsByCourse)
        {
            var existingLessons = await _context.OriginalLessons
                .Where(lesson => lesson.TopicId == entry.Key)
                .OrderBy(lesson => lesson.Id)
                .ToListAsync();

            for (var index = 0; index < entry.Value.Length; index++)
            {
                var data = entry.Value[index];
                var lesson = index < existingLessons.Count ? existingLessons[index] : new OriginalLesson
                {
                    TopicId = entry.Key,
                    SourceType = "SYSTEM",
                    IsAiGenerated = false,
                    CreatedAt = DateTime.UtcNow
                };

                lesson.Title = data.Title;
                lesson.Summary = data.Summary;
                lesson.Content = data.Content;
                lesson.ContentType = "ARTICLE";
                lesson.EstimatedMinutes = 20 + index * 5;
                lesson.ReviewStatus = "APPROVED";
                lesson.CreatedBy = teacherUserId;
                lesson.UpdatedAt = DateTime.UtcNow;

                if (index >= existingLessons.Count)
                {
                    _context.OriginalLessons.Add(lesson);
                }
            }
        }

        await _context.SaveChangesAsync();
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

        // Ensure Exam Mode columns exist on quizzes, quiz_attempts, test_attempts
        try
        {
            string ensureExamColumnsSql = @"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[quizzes]') AND name = 'is_exam_mode')
                    ALTER TABLE [dbo].[quizzes] ADD [is_exam_mode] BIT NOT NULL CONSTRAINT [DF_quizzes_is_exam_mode] DEFAULT 0;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[quizzes]') AND name = 'max_violations')
                    ALTER TABLE [dbo].[quizzes] ADD [max_violations] INT NOT NULL CONSTRAINT [DF_quizzes_max_violations] DEFAULT 3;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[quiz_attempts]') AND name = 'fullscreen_exit_count')
                    ALTER TABLE [dbo].[quiz_attempts] ADD [fullscreen_exit_count] INT NOT NULL CONSTRAINT [DF_quiz_attempts_fullscreen_exit] DEFAULT 0;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[quiz_attempts]') AND name = 'tab_switch_count')
                    ALTER TABLE [dbo].[quiz_attempts] ADD [tab_switch_count] INT NOT NULL CONSTRAINT [DF_quiz_attempts_tab_switch] DEFAULT 0;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[quiz_attempts]') AND name = 'violation_log')
                    ALTER TABLE [dbo].[quiz_attempts] ADD [violation_log] NVARCHAR(MAX) NULL;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[test_attempts]') AND name = 'fullscreen_exit_count')
                    ALTER TABLE [dbo].[test_attempts] ADD [fullscreen_exit_count] INT NOT NULL CONSTRAINT [DF_test_attempts_fullscreen_exit] DEFAULT 0;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[test_attempts]') AND name = 'tab_switch_count')
                    ALTER TABLE [dbo].[test_attempts] ADD [tab_switch_count] INT NOT NULL CONSTRAINT [DF_test_attempts_tab_switch] DEFAULT 0;

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[test_attempts]') AND name = 'violation_log')
                    ALTER TABLE [dbo].[test_attempts] ADD [violation_log] NVARCHAR(MAX) NULL;
            ";
            await _context.Database.ExecuteSqlRawAsync(ensureExamColumnsSql);
        }
        catch
        {
            // Table alteration fallback if permissions restricted
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
                var lessonIdsToRemove = lessonsToRemove.Select(lesson => lesson.Id).ToList();
                var referencingNodes = await _context.LearningPathNodes
                    .Where(node => node.LessonId.HasValue && lessonIdsToRemove.Contains(node.LessonId.Value))
                    .ToListAsync();
                foreach (var node in referencingNodes)
                {
                    node.LessonId = null;
                }
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
                ("Bài học 1: Tổng quan về 12 Thì",
                 "Hiểu hệ thống 12 thì tiếng Anh và cách xác định thời gian của hành động.",
                 """
                 <h3>1. Hệ thống 12 thì</h3><p>12 thì được tạo từ 3 mốc thời gian: Quá khứ, Hiện tại, Tương lai và 4 dạng: Đơn, Tiếp diễn, Hoàn thành, Hoàn thành tiếp diễn.</p><div class="table-responsive my-3"><table class="table table-bordered align-middle"><thead class="table-light"><tr><th>Dạng</th><th>Quá khứ</th><th>Hiện tại</th><th>Tương lai</th></tr></thead><tbody><tr><td><strong>Đơn</strong></td><td>Past Simple</td><td>Present Simple</td><td>Future Simple</td></tr><tr><td><strong>Tiếp diễn</strong></td><td>Past Continuous</td><td>Present Continuous</td><td>Future Continuous</td></tr><tr><td><strong>Hoàn thành</strong></td><td>Past Perfect</td><td>Present Perfect</td><td>Future Perfect</td></tr><tr><td><strong>Hoàn thành tiếp diễn</strong></td><td>Past Perfect Continuous</td><td>Present Perfect Continuous</td><td>Future Perfect Continuous</td></tr></tbody></table></div><h3>2. Mẹo nhận biết nhanh</h3><ul><li><strong>Simple:</strong> nói về sự thật, thói quen hoặc hành động đơn lẻ.</li><li><strong>Continuous:</strong> nhấn mạnh hành động đang diễn ra.</li><li><strong>Perfect:</strong> nhấn mạnh sự hoàn thành hoặc kết quả.</li><li><strong>Perfect Continuous:</strong> nhấn mạnh khoảng thời gian hành động diễn ra.</li></ul><h3>3. Ví dụ</h3><div class="c26-grammar-formula mb-3"><p><strong>1. I study English every day.</strong><br><em>Dịch: Tôi học tiếng Anh mỗi ngày.</em><br><small>Giải thích: Thói quen → Present Simple.</small></p><p><strong>2. I am studying English now.</strong><br><em>Dịch: Tôi đang học tiếng Anh bây giờ.</em><br><small>Giải thích: Hành động đang diễn ra → Present Continuous.</small></p><p><strong>3. I have studied English for two years.</strong><br><em>Dịch: Tôi đã học tiếng Anh được hai năm.</em><br><small>Giải thích: Hành động bắt đầu trong quá khứ và còn liên quan hiện tại → Present Perfect.</small></p></div><h3>4. Bài tập thực hành</h3><div class="quiz-container bg-white p-4 rounded shadow-sm border mt-3 mb-4"><form id="practiceQuizForm"><div class="mb-4"><h5 class="mb-3">1. Chọn thì phù hợp: She ___ to school every day. (go)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1a" value="correct"><label class="form-check-label" for="q1a">goes</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1b" value="wrong"><label class="form-check-label" for="q1b">is going</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1c" value="wrong"><label class="form-check-label" for="q1c">has gone</label></div></div><div class="mb-4"><h5 class="mb-3">2. Chọn thì phù hợp: They ___ dinner now. (have)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2a" value="wrong"><label class="form-check-label" for="q2a">have</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2b" value="correct"><label class="form-check-label" for="q2b">are having</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2c" value="wrong"><label class="form-check-label" for="q2c">had</label></div></div><div class="mb-4"><h5 class="mb-3">3. Chọn thì phù hợp: I ___ this book already. (read)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3a" value="wrong"><label class="form-check-label" for="q3a">read</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong"><label class="form-check-label" for="q3b">am reading</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3c" value="correct"><label class="form-check-label" for="q3c">have read</label></div></div><button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()"><i class="fa-solid fa-paper-plane me-2"></i>Nộp bài</button><div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div></form></div><script>function checkQuiz() { let score = 0; const total = 3; const form = document.getElementById("practiceQuizForm"); if (form.q1 && form.q1.value === "correct") score++; if (form.q2 && form.q2.value === "correct") score++; if (form.q3 && form.q3.value === "correct") score++; const resultDiv = document.getElementById("quizResult"); resultDiv.style.display = "block"; if (score === total) { resultDiv.className = "mt-3 fw-bold fs-5 text-success"; resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu."; } else { resultDiv.className = "mt-3 fw-bold fs-5 text-warning"; resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy thử lại nhé!"; } }</script><div class="alert alert-info shadow-sm mt-4"><h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Tóm tắt bài học</h5><ul class="mb-0"><li>Xác định thời gian trước: quá khứ, hiện tại hay tương lai.</li><li>Sau đó xác định dạng: đơn, tiếp diễn, hoàn thành hoặc hoàn thành tiếp diễn.</li><li>Không cần học thuộc 12 thì cùng lúc; hãy học theo từng nhóm.</li></ul></div>
                 """,
                 "ARTICLE", null),

                ("Bài học 2: Phân biệt Present Simple và Present Continuous",
                 "Phân biệt hành động thường xuyên với hành động đang xảy ra tại thời điểm nói.",
                 """
                 <h3>1. Present Simple</h3><p>Dùng cho thói quen, lịch trình và sự thật hiển nhiên.</p><div class="c26-grammar-formula">Công thức: S + V(s/es) + O</div><p><strong>Ví dụ:</strong></p><ul><li><code>Tom plays football every Sunday.</code> (Thói quen lặp lại.)</li><li><code>Water boils at 100°C.</code> (Sự thật hiển nhiên.)</li></ul><p><strong>Dấu hiệu nhận biết:</strong> <em>every day, usually, often, always, sometimes, never</em>.</p><h3>2. Present Continuous</h3><p>Dùng cho hành động đang diễn ra tại thời điểm nói hoặc quanh thời điểm hiện tại.</p><div class="c26-grammar-formula">Công thức: S + am/is/are + V-ing + O</div><p><strong>Ví dụ:</strong></p><ul><li><code>Tom is playing football now.</code> (Hành động đang xảy ra.)</li><li><code>I am studying for my exam this week.</code> (Hoạt động tạm thời quanh hiện tại.)</li></ul><p><strong>Dấu hiệu nhận biết:</strong> <em>now, right now, at the moment, today, this week</em>.</p><h3>3. So sánh nhanh</h3><div class="table-responsive my-3"><table class="table table-bordered align-middle"><thead class="table-light"><tr><th>Present Simple</th><th>Present Continuous</th></tr></thead><tbody><tr><td>I work every day.</td><td>I am working now.</td></tr><tr><td>Thói quen</td><td>Đang diễn ra</td></tr><tr><td>usually, often, every...</td><td>now, at the moment</td></tr></tbody></table></div><h3>4. Bài tập thực hành</h3><div class="quiz-container bg-white p-4 rounded shadow-sm border mt-3 mb-4"><form id="practiceQuizForm"><div class="mb-4"><h5 class="mb-3">1. My brother ___ football every Saturday. (play)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1a" value="correct"><label class="form-check-label" for="q1a">plays</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1b" value="wrong"><label class="form-check-label" for="q1b">is playing</label></div></div><div class="mb-4"><h5 class="mb-3">2. Look! The baby ___. (sleep)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2a" value="wrong"><label class="form-check-label" for="q2a">sleeps</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2b" value="correct"><label class="form-check-label" for="q2b">is sleeping</label></div></div><div class="mb-4"><h5 class="mb-3">3. We usually ___ dinner at 7 p.m. (have)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3a" value="correct"><label class="form-check-label" for="q3a">have</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong"><label class="form-check-label" for="q3b">are having</label></div></div><div class="mb-4"><h5 class="mb-3">4. She ___ a book at the moment. (read)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4a" value="wrong"><label class="form-check-label" for="q4a">reads</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4b" value="correct"><label class="form-check-label" for="q4b">is reading</label></div></div><button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()"><i class="fa-solid fa-paper-plane me-2"></i>Nộp bài</button><div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div></form></div><script>function checkQuiz() { let score = 0; const total = 4; const form = document.getElementById("practiceQuizForm"); if (form.q1 && form.q1.value === "correct") score++; if (form.q2 && form.q2.value === "correct") score++; if (form.q3 && form.q3.value === "correct") score++; if (form.q4 && form.q4.value === "correct") score++; const resultDiv = document.getElementById("quizResult"); resultDiv.style.display = "block"; if (score === total) { resultDiv.className = "mt-3 fw-bold fs-5 text-success"; resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu."; } else { resultDiv.className = "mt-3 fw-bold fs-5 text-warning"; resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy xem lại bài nhé!"; } }</script><div class="alert alert-info shadow-sm mt-4"><h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Tóm tắt bài học</h5><ul class="mb-0"><li>Thói quen/lặp lại → Present Simple.</li><li>Đang xảy ra → Present Continuous.</li><li>Luôn nhìn vào ngữ cảnh, không chỉ nhìn một từ khóa.</li></ul></div>
                 """,
                 "ARTICLE", null),

                ("Bài học 3: Thực hành và Bài tập tổng hợp về Tenses",
                 "Áp dụng kiến thức về các thì vào câu thực tế và tự sửa lỗi.",
                 """
                 <h3>1. Chọn thì theo ngữ cảnh</h3><p>Đọc toàn bộ câu trước khi chọn thì. Xác định thời điểm, tính chất của hành động và các từ chỉ thời gian.</p><h3>2. Sửa lỗi thường gặp</h3><div class="c26-grammar-formula mb-3"><p><strong class="text-danger">❌ Sai:</strong> She go to school every day.<br><strong class="text-success">✅ Đúng:</strong> She goes to school every day.<br><small>Giải thích: Chủ ngữ She ở Present Simple cần động từ thêm s/es.</small></p><p><strong class="text-danger">❌ Sai:</strong> They is studying now.<br><strong class="text-success">✅ Đúng:</strong> They are studying now.<br><small>Giải thích: They đi với are trong Present Continuous.</small></p><p><strong class="text-danger">❌ Sai:</strong> I am go to school every day.<br><strong class="text-success">✅ Đúng:</strong> I go to school every day.<br><small>Giải thích: Thói quen dùng Present Simple, không dùng am + V nguyên mẫu.</small></p></div><h3>3. Hội thoại thực tế</h3><div class="bg-light p-3 rounded border mb-3"><p><strong>A:</strong> What do you usually do after school?</p><p><strong>B:</strong> I usually go home and do my homework.</p><p><strong>A:</strong> What are you doing now?</p><p><strong>B:</strong> I am studying English.</p><p class="text-muted mt-2 mb-0"><em>Dịch: A hỏi về thói quen dùng Present Simple; khi hỏi việc đang diễn ra dùng Present Continuous.</em></p></div><h3>4. Bài tập thực hành</h3><div class="quiz-container bg-white p-4 rounded shadow-sm border mt-3 mb-4"><form id="practiceQuizForm"><div class="mb-4"><h5 class="mb-3">1. He ___ coffee every morning. (drink)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1a" value="correct"><label class="form-check-label" for="q1a">drinks</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1b" value="wrong"><label class="form-check-label" for="q1b">is drinking</label></div></div><div class="mb-4"><h5 class="mb-3">2. Listen! Someone ___. (sing)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2a" value="wrong"><label class="form-check-label" for="q2a">sings</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2b" value="correct"><label class="form-check-label" for="q2b">is singing</label></div></div><div class="mb-4"><h5 class="mb-3">3. They ___ to school by bus every day. (go)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3a" value="correct"><label class="form-check-label" for="q3a">go</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong"><label class="form-check-label" for="q3b">goes</label></div></div><div class="mb-4"><h5 class="mb-3">4. I ___ my homework right now. (do)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4a" value="wrong"><label class="form-check-label" for="q4a">do</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4b" value="correct"><label class="form-check-label" for="q4b">am doing</label></div></div><div class="mb-4"><h5 class="mb-3">5. Sửa lỗi: She are watching TV now.</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q5" id="q5a" value="correct"><label class="form-check-label" for="q5a">She is watching TV now.</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q5" id="q5b" value="wrong"><label class="form-check-label" for="q5b">She watching TV now.</label></div></div><button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()"><i class="fa-solid fa-paper-plane me-2"></i>Nộp bài</button><div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div></form></div><script>function checkQuiz() { let score = 0; const total = 5; const form = document.getElementById("practiceQuizForm"); if (form.q1 && form.q1.value === "correct") score++; if (form.q2 && form.q2.value === "correct") score++; if (form.q3 && form.q3.value === "correct") score++; if (form.q4 && form.q4.value === "correct") score++; if (form.q5 && form.q5.value === "correct") score++; const resultDiv = document.getElementById("quizResult"); resultDiv.style.display = "block"; if (score === total) { resultDiv.className = "mt-3 fw-bold fs-5 text-success"; resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu."; } else { resultDiv.className = "mt-3 fw-bold fs-5 text-warning"; resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy cố gắng lên!"; } }</script><div class="alert alert-info shadow-sm mt-4"><h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Tóm tắt bài học</h5><ul class="mb-0"><li>Đọc ngữ cảnh trước khi chọn thì.</li><li>Kiểm tra chủ ngữ và trợ động từ.</li><li>Sau khi chọn thì, kiểm tra lại dạng của động từ.</li></ul></div>
                 """,
                 "ARTICLE", null)
            },

            ["GRAM_PRES_SIMP"] = new()
            {
                ("Bài học 1: Tổng quan về 12 Thì",
                 "Hiểu hệ thống 12 thì tiếng Anh và cách xác định thời gian của hành động.",
                 """
                 <h3>1. Hệ thống 12 thì</h3><p>12 thì được tạo từ 3 mốc thời gian: Quá khứ, Hiện tại, Tương lai và 4 dạng: Đơn, Tiếp diễn, Hoàn thành, Hoàn thành tiếp diễn.</p><div class="table-responsive my-3"><table class="table table-bordered align-middle"><thead class="table-light"><tr><th>Dạng</th><th>Quá khứ</th><th>Hiện tại</th><th>Tương lai</th></tr></thead><tbody><tr><td><strong>Đơn</strong></td><td>Past Simple</td><td>Present Simple</td><td>Future Simple</td></tr><tr><td><strong>Tiếp diễn</strong></td><td>Past Continuous</td><td>Present Continuous</td><td>Future Continuous</td></tr><tr><td><strong>Hoàn thành</strong></td><td>Past Perfect</td><td>Present Perfect</td><td>Future Perfect</td></tr><tr><td><strong>Hoàn thành tiếp diễn</strong></td><td>Past Perfect Continuous</td><td>Present Perfect Continuous</td><td>Future Perfect Continuous</td></tr></tbody></table></div><h3>2. Mẹo nhận biết nhanh</h3><ul><li><strong>Simple:</strong> nói về sự thật, thói quen hoặc hành động đơn lẻ.</li><li><strong>Continuous:</strong> nhấn mạnh hành động đang diễn ra.</li><li><strong>Perfect:</strong> nhấn mạnh sự hoàn thành hoặc kết quả.</li><li><strong>Perfect Continuous:</strong> nhấn mạnh khoảng thời gian hành động diễn ra.</li></ul><h3>3. Ví dụ</h3><div class="c26-grammar-formula mb-3"><p><strong>1. I study English every day.</strong><br><em>Dịch: Tôi học tiếng Anh mỗi ngày.</em><br><small>Giải thích: Thói quen → Present Simple.</small></p><p><strong>2. I am studying English now.</strong><br><em>Dịch: Tôi đang học tiếng Anh bây giờ.</em><br><small>Giải thích: Hành động đang diễn ra → Present Continuous.</small></p><p><strong>3. I have studied English for two years.</strong><br><em>Dịch: Tôi đã học tiếng Anh được hai năm.</em><br><small>Giải thích: Hành động bắt đầu trong quá khứ và còn liên quan hiện tại → Present Perfect.</small></p></div><h3>4. Bài tập thực hành</h3><div class="quiz-container bg-white p-4 rounded shadow-sm border mt-3 mb-4"><form id="practiceQuizForm"><div class="mb-4"><h5 class="mb-3">1. Chọn thì phù hợp: She ___ to school every day. (go)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1a" value="correct"><label class="form-check-label" for="q1a">goes</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1b" value="wrong"><label class="form-check-label" for="q1b">is going</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1c" value="wrong"><label class="form-check-label" for="q1c">has gone</label></div></div><div class="mb-4"><h5 class="mb-3">2. Chọn thì phù hợp: They ___ dinner now. (have)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2a" value="wrong"><label class="form-check-label" for="q2a">have</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2b" value="correct"><label class="form-check-label" for="q2b">are having</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2c" value="wrong"><label class="form-check-label" for="q2c">had</label></div></div><div class="mb-4"><h5 class="mb-3">3. Chọn thì phù hợp: I ___ this book already. (read)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3a" value="wrong"><label class="form-check-label" for="q3a">read</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong"><label class="form-check-label" for="q3b">am reading</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3c" value="correct"><label class="form-check-label" for="q3c">have read</label></div></div><button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()"><i class="fa-solid fa-paper-plane me-2"></i>Nộp bài</button><div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div></form></div><script>function checkQuiz() { let score = 0; const total = 3; const form = document.getElementById("practiceQuizForm"); if (form.q1 && form.q1.value === "correct") score++; if (form.q2 && form.q2.value === "correct") score++; if (form.q3 && form.q3.value === "correct") score++; const resultDiv = document.getElementById("quizResult"); resultDiv.style.display = "block"; if (score === total) { resultDiv.className = "mt-3 fw-bold fs-5 text-success"; resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu."; } else { resultDiv.className = "mt-3 fw-bold fs-5 text-warning"; resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy thử lại nhé!"; } }</script><div class="alert alert-info shadow-sm mt-4"><h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Tóm tắt bài học</h5><ul class="mb-0"><li>Xác định thời gian trước: quá khứ, hiện tại hay tương lai.</li><li>Sau đó xác định dạng: đơn, tiếp diễn, hoàn thành hoặc hoàn thành tiếp diễn.</li><li>Không cần học thuộc 12 thì cùng lúc; hãy học theo từng nhóm.</li></ul></div>
                 """,
                 "ARTICLE", null),

                ("Bài học 2: Phân biệt Present Simple và Present Continuous",
                 "Phân biệt hành động thường xuyên với hành động đang xảy ra tại thời điểm nói.",
                 """
                 <h3>1. Present Simple</h3><p>Dùng cho thói quen, lịch trình và sự thật hiển nhiên.</p><div class="c26-grammar-formula">Công thức: S + V(s/es) + O</div><p><strong>Ví dụ:</strong></p><ul><li><code>Tom plays football every Sunday.</code> (Thói quen lặp lại.)</li><li><code>Water boils at 100°C.</code> (Sự thật hiển nhiên.)</li></ul><p><strong>Dấu hiệu nhận biết:</strong> <em>every day, usually, often, always, sometimes, never</em>.</p><h3>2. Present Continuous</h3><p>Dùng cho hành động đang diễn ra tại thời điểm nói hoặc quanh thời điểm hiện tại.</p><div class="c26-grammar-formula">Công thức: S + am/is/are + V-ing + O</div><p><strong>Ví dụ:</strong></p><ul><li><code>Tom is playing football now.</code> (Hành động đang xảy ra.)</li><li><code>I am studying for my exam this week.</code> (Hoạt động tạm thời quanh hiện tại.)</li></ul><p><strong>Dấu hiệu nhận biết:</strong> <em>now, right now, at the moment, today, this week</em>.</p><h3>3. So sánh nhanh</h3><div class="table-responsive my-3"><table class="table table-bordered align-middle"><thead class="table-light"><tr><th>Present Simple</th><th>Present Continuous</th></tr></thead><tbody><tr><td>I work every day.</td><td>I am working now.</td></tr><tr><td>Thói quen</td><td>Đang diễn ra</td></tr><tr><td>usually, often, every...</td><td>now, at the moment</td></tr></tbody></table></div><h3>4. Bài tập thực hành</h3><div class="quiz-container bg-white p-4 rounded shadow-sm border mt-3 mb-4"><form id="practiceQuizForm"><div class="mb-4"><h5 class="mb-3">1. My brother ___ football every Saturday. (play)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1a" value="correct"><label class="form-check-label" for="q1a">plays</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1b" value="wrong"><label class="form-check-label" for="q1b">is playing</label></div></div><div class="mb-4"><h5 class="mb-3">2. Look! The baby ___. (sleep)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2a" value="wrong"><label class="form-check-label" for="q2a">sleeps</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2b" value="correct"><label class="form-check-label" for="q2b">is sleeping</label></div></div><div class="mb-4"><h5 class="mb-3">3. We usually ___ dinner at 7 p.m. (have)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3a" value="correct"><label class="form-check-label" for="q3a">have</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong"><label class="form-check-label" for="q3b">are having</label></div></div><div class="mb-4"><h5 class="mb-3">4. She ___ a book at the moment. (read)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4a" value="wrong"><label class="form-check-label" for="q4a">reads</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4b" value="correct"><label class="form-check-label" for="q4b">is reading</label></div></div><button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()"><i class="fa-solid fa-paper-plane me-2"></i>Nộp bài</button><div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div></form></div><script>function checkQuiz() { let score = 0; const total = 4; const form = document.getElementById("practiceQuizForm"); if (form.q1 && form.q1.value === "correct") score++; if (form.q2 && form.q2.value === "correct") score++; if (form.q3 && form.q3.value === "correct") score++; if (form.q4 && form.q4.value === "correct") score++; const resultDiv = document.getElementById("quizResult"); resultDiv.style.display = "block"; if (score === total) { resultDiv.className = "mt-3 fw-bold fs-5 text-success"; resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu."; } else { resultDiv.className = "mt-3 fw-bold fs-5 text-warning"; resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy xem lại bài nhé!"; } }</script><div class="alert alert-info shadow-sm mt-4"><h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Tóm tắt bài học</h5><ul class="mb-0"><li>Thói quen/lặp lại → Present Simple.</li><li>Đang xảy ra → Present Continuous.</li><li>Luôn nhìn vào ngữ cảnh, không chỉ nhìn một từ khóa.</li></ul></div>
                 """,
                 "ARTICLE", null),

                ("Bài học 3: Thực hành và Bài tập tổng hợp về Tenses",
                 "Áp dụng kiến thức về các thì vào câu thực tế và tự sửa lỗi.",
                 """
                 <h3>1. Chọn thì theo ngữ cảnh</h3><p>Đọc toàn bộ câu trước khi chọn thì. Xác định thời điểm, tính chất của hành động và các từ chỉ thời gian.</p><h3>2. Sửa lỗi thường gặp</h3><div class="c26-grammar-formula mb-3"><p><strong class="text-danger">❌ Sai:</strong> She go to school every day.<br><strong class="text-success">✅ Đúng:</strong> She goes to school every day.<br><small>Giải thích: Chủ ngữ She ở Present Simple cần động từ thêm s/es.</small></p><p><strong class="text-danger">❌ Sai:</strong> They is studying now.<br><strong class="text-success">✅ Đúng:</strong> They are studying now.<br><small>Giải thích: They đi với are trong Present Continuous.</small></p><p><strong class="text-danger">❌ Sai:</strong> I am go to school every day.<br><strong class="text-success">✅ Đúng:</strong> I go to school every day.<br><small>Giải thích: Thói quen dùng Present Simple, không dùng am + V nguyên mẫu.</small></p></div><h3>3. Hội thoại thực tế</h3><div class="bg-light p-3 rounded border mb-3"><p><strong>A:</strong> What do you usually do after school?</p><p><strong>B:</strong> I usually go home and do my homework.</p><p><strong>A:</strong> What are you doing now?</p><p><strong>B:</strong> I am studying English.</p><p class="text-muted mt-2 mb-0"><em>Dịch: A hỏi về thói quen dùng Present Simple; khi hỏi việc đang diễn ra dùng Present Continuous.</em></p></div><h3>4. Bài tập thực hành</h3><div class="quiz-container bg-white p-4 rounded shadow-sm border mt-3 mb-4"><form id="practiceQuizForm"><div class="mb-4"><h5 class="mb-3">1. He ___ coffee every morning. (drink)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1a" value="correct"><label class="form-check-label" for="q1a">drinks</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q1" id="q1b" value="wrong"><label class="form-check-label" for="q1b">is drinking</label></div></div><div class="mb-4"><h5 class="mb-3">2. Listen! Someone ___. (sing)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2a" value="wrong"><label class="form-check-label" for="q2a">sings</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q2" id="q2b" value="correct"><label class="form-check-label" for="q2b">is singing</label></div></div><div class="mb-4"><h5 class="mb-3">3. They ___ to school by bus every day. (go)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3a" value="correct"><label class="form-check-label" for="q3a">go</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q3" id="q3b" value="wrong"><label class="form-check-label" for="q3b">goes</label></div></div><div class="mb-4"><h5 class="mb-3">4. I ___ my homework right now. (do)</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4a" value="wrong"><label class="form-check-label" for="q4a">do</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q4" id="q4b" value="correct"><label class="form-check-label" for="q4b">am doing</label></div></div><div class="mb-4"><h5 class="mb-3">5. Sửa lỗi: She are watching TV now.</h5><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q5" id="q5a" value="correct"><label class="form-check-label" for="q5a">She is watching TV now.</label></div><div class="form-check mb-2"><input class="form-check-input" type="radio" name="q5" id="q5b" value="wrong"><label class="form-check-label" for="q5b">She watching TV now.</label></div></div><button type="button" class="btn btn-primary px-4 py-2" onclick="checkQuiz()"><i class="fa-solid fa-paper-plane me-2"></i>Nộp bài</button><div id="quizResult" class="mt-3 fw-bold fs-5" style="display: none;"></div></form></div><script>function checkQuiz() { let score = 0; const total = 5; const form = document.getElementById("practiceQuizForm"); if (form.q1 && form.q1.value === "correct") score++; if (form.q2 && form.q2.value === "correct") score++; if (form.q3 && form.q3.value === "correct") score++; if (form.q4 && form.q4.value === "correct") score++; if (form.q5 && form.q5.value === "correct") score++; const resultDiv = document.getElementById("quizResult"); resultDiv.style.display = "block"; if (score === total) { resultDiv.className = "mt-3 fw-bold fs-5 text-success"; resultDiv.innerHTML = "🎉 Tuyệt vời! Bạn đã đúng " + score + "/" + total + " câu."; } else { resultDiv.className = "mt-3 fw-bold fs-5 text-warning"; resultDiv.innerHTML = "Bạn trả lời đúng " + score + "/" + total + " câu. Hãy cố gắng lên!"; } }</script><div class="alert alert-info shadow-sm mt-4"><h5><i class="fa-solid fa-lightbulb text-warning me-2"></i>Tóm tắt bài học</h5><ul class="mb-0"><li>Đọc ngữ cảnh trước khi chọn thì.</li><li>Kiểm tra chủ ngữ và trợ động từ.</li><li>Sau khi chọn thì, kiểm tra lại dạng của động từ.</li></ul></div>
                 """,
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
                ("Bài 1: Tổng quan cấu trúc Academic Essay",
                 "Khái niệm bài luận học thuật, phân biệt văn phong Academic Formal vs Informal, quy chuẩn độ dài 250-300 từ và 4 dạng essay phổ biến.",
                 "<h3>1. Mục tiêu bài học</h3><p>Nắm vững khái niệm bài luận học thuật (Academic Essay), phân biệt sự khác nhau giữa văn phong học thuật (Academic Formal) và văn phong tự do (Informal). Hiểu rõ cấu trúc 3 phần tiêu chuẩn và 4 dạng bài luận cốt lõi.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Academic Essay là một văn bản học thuật thể hiện quan điểm, lập luận có căn cứ về một chủ đề xã hội hay khoa học. Bài luận chuẩn đòi hỏi 3 yếu tố nền tảng:</p><ul><li><strong>Khách quan (Objectivity):</strong> Không dùng từ suồng sã, viết tắt (don't -> do not) hay cảm xúc cá nhân thái quá.</li><li><strong>Cấu trúc 3 phần (3-Part Structure):</strong> Mở bài (Introduction), Thân bài (Body Paragraphs), Kết bài (Conclusion). Độ dài chuẩn từ 250 - 300 từ.</li><li><strong>4 Dạng bài chính:</strong> Opinion (Đồng ý/Không đồng ý), Discussion (Thảo luận 2 quan điểm), Cause & Solution (Nguyên nhân & Giải pháp), Advantage & Disadvantage (Mặt tốt & Mặt xấu).</li></ul><h3>3. Công thức & Cấu trúc Essay tổng quan</h3><div class=\"c26-grammar-formula\">Master Essay Architecture (250-300 words):<br>- Paragraph 1: Introduction (40-50 words) -> Background Paraphrase + Thesis Statement.<br>- Paragraph 2: Body 1 (90-100 words) -> Main Point 1 + Explanation + Example.<br>- Paragraph 3: Body 2 (90-100 words) -> Main Point 2 + Explanation + Example.<br>- Paragraph 4: Conclusion (30-40 words) -> Restate Thesis + Summary + Final Thought.</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>Informal Writing (Chưa chuẩn):</strong><br><code>I think medical robots are super cool because doctors get tired easily.</code></p><p><strong>Academic Formal Writing (Chuẩn học thuật):</strong><br><code>Advancements in medical robotics have significantly enhanced surgical precision, thereby reducing human error caused by practitioner fatigue.</code></p><p><em>Dịch: Những tiến bộ trong robot y tế đã làm tăng đáng kể độ chính xác của phẫu thuật, từ đó giảm thiểu lỗi do con người gây ra bởi sự mệt mỏi của bác sĩ.</em></p><p><strong>Giải thích:</strong> Câu học thuật thay thế từ xưng hô \"I think\" và từ suồng sã \"super cool\" bằng cụm từ học thuật trang trọng (<em>advancements in medical robotics, surgical precision, practitioner fatigue</em>) và từ nối nguyên nhân (<em>thereby</em>).</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập 1:</strong> Chọn câu văn mang phong cách Academic Formal:<br>A. \"A lot of people get sick because food from fast food joints is bad for you.\"<br>B. \"Frequent consumption of highly processed fast food is associated with elevated health risks among urban residents.\"</p><p><strong>Bài tập 2:</strong> Ghép đề bài với dạng bài tương ứng:<br>1. \"What are the reasons for traffic congestion, and how can cities fix it?\"<br>2. \"Some people think homework should be banned. Do you agree or disagree?\"</p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Bài tập 1:</strong> Chọn <strong>B</strong>. Vì B dùng từ vựng AWL (<em>frequent consumption, highly processed, elevated health risks, urban residents</em>).</p><p><strong>Bài tập 2:</strong> 1 -> Cause & Solution; 2 -> Opinion Essay.</p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Bài luận học thuật gồm 4 đoạn chính (250-300 từ): 1 Intro + 2 Bodies + 1 Conclusion.</li><li>Không viết tắt, không dùng ngôn ngữ giao tiếp hằng ngày.</li><li>Xác định đúng 1 trong 4 dạng essay trước khi đặt bút viết.</li></ul>",
                 "ARTICLE", null),

                ("Bài 2: Phân tích đề và xác định yêu cầu",
                 "Phương pháp phân tích đề 3T (Topic, Micro-topic, Task Command) trong 2 phút và kỹ thuật gạch chân keyword tránh lạc đề.",
                 "<h3>1. Mục tiêu bài học</h3><p>Làm chủ phương pháp phân tích đề bài 3T (Topic - Micro-topic - Task Command) trong 2 phút. Nhận diện các bẫy lạc đề (Off-topic) và xác định chính xác số lượng câu hỏi cần trả lời.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Khi đọc đề bài luận, người viết cần nhanh chóng bóc tách 3 thành phần cốt lõi bằng kỹ thuật gạch chân từ khóa (Keywords):</p><ul><li><strong>T1 - General Topic (Chủ đề lớn):</strong> Lĩnh vực tổng quát (e.g., Health, Technology, Environment, Education).</li><li><strong>T2 - Micro-topic / Specific Focus (Khía cạnh bị thu hẹp):</strong> Đối tượng cụ thể (e.g., Fast food advertising targeted at children, Plastic packaging in supermarkets).</li><li><strong>T3 - Task Command (Yêu cầu hành động):</strong> Loại câu hỏi cụ thể (Agree/Disagree, Discuss both views, Causes & Solutions). Nếu đề bài có 2 câu hỏi, bạn PHẢI trả lời đầy đủ cả 2 trong 2 đoạn thân bài.</li></ul><h3>3. Công thức Phân tích Đề bài 3T</h3><div class=\"c26-grammar-formula\">3T Analysis Checklist:<br>1. General Topic: Lĩnh vực là gì?<br>2. Micro-topic: Giới hạn không gian / đối tượng / điều kiện nào?<br>3. Task Instruction: Trả lời quan điểm cá nhân hay bàn luận 2 chiều? Có mấy câu hỏi?</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>Prompt Sample:</strong><br><code>Childhood obesity has increased dramatically in many developed countries. Some people claim that advertising fast food to children should be banned. To what extent do you agree or disagree?</code></p><p><em>Dịch: Béo phì ở trẻ em đã tăng vọt tại nhiều quốc gia phát triển. Một số người cho rằng nên cấm quảng cáo đồ ăn nhanh hướng đến trẻ em. Bạn đồng ý hay không đồng ý ở mức độ nào?</em></p><p><strong>Phân tích 3T Chi tiết:</strong><br>- <strong>Topic:</strong> Health / Children.<br>- <strong>Micro-topic:</strong> Banning fast food advertisements aimed at children.<br>- <strong>Task Command:</strong> Agree or Disagree -> Phải đưa ra lập trường rõ ràng (Đồng ý cấm hay Không nên cấm).</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Hãy gạch chân và xác định T1 (Topic), T2 (Micro-topic), T3 (Task) cho đề bài sau:<br><code>\"Rapid urbanization has led to overcrowding in major cities. What problems does this cause, and what solutions can municipal governments implement?\"</code></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Đáp án:</strong><br>- <strong>T1 (Topic):</strong> Urbanization / City life.<br>- <strong>T2 (Micro-topic):</strong> Overcrowding in major cities (Đô thị hóa gây quá tải đô thị).<br>- <strong>T3 (Task):</strong> 2 câu hỏi -> Thân bài 1 (Problems of overcrowding) & Thân bài 2 (Solutions by governments).</p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Luôn dành 2 phút phân tích đề trước khi viết.</li><li>Không viết lan man sang chủ đề rộng (Topic) mà phải xoáy sâu vào khía cạnh thu hẹp (Micro-topic).</li><li>Nắm rõ câu hỏi chỉ có 1 yêu cầu hay có 2 yêu cầu song song.</li></ul>",
                 "ARTICLE", null),

                ("Bài 3: Thesis Statement",
                 "Cách viết câu luận đề (Thesis Statement) sắc bén ở cuối đoạn mở bài, phân biệt Weak Thesis vs Strong Thesis.",
                 "<h3>1. Mục tiêu bài học</h3><p>Hiểu vai trò của câu luận đề (Thesis Statement) trong việc định hướng bài luận. Biết cách viết câu Thesis sắc bén, rõ ràng cho từng dạng essay và phân biệt được Weak Thesis vs Strong Thesis.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Thesis Statement là câu quan trọng nhất bài luận, đặt ở câu cuối của đoạn Mở bài. Một câu Thesis đạt chuẩn B2 phải thỏa mãn 3 điều kiện:</p><ul><li><strong>Rõ ràng (Clear stance):</strong> Thể hiện lập trường dứt khoát, không dùng từ chung chung mơ hồ (\"there are pros and cons\").</li><li><strong>Định hướng (Preview points):</strong> Nêu vắn tắt 2 lý do/khía cạnh sẽ chứng minh ở 2 đoạn thân bài.</li><li><strong>Phân biệt Weak vs Strong Thesis:</strong><br>- <em>Weak Thesis:</em> \"Air travel causes environmental problems.\" (Quá chung chung, không có lập trường).<br>- <em>Strong Thesis:</em> \"Governments should levy higher environmental taxes on airline companies to curb carbon emissions and fund green infrastructure.\" (Cụ thể, sắc bén).</li></ul><h3>3. Công thức Thesis Statement Theo Dạng Bài</h3><div class=\"c26-grammar-formula\">1. Opinion Essay:<br>While some advocate [Counter-view], I firmly believe that [Main Stance] because [Reason 1] and [Reason 2].<br><br>2. Discussion Essay:<br>Although [View A] has certain merits, I maintain that [View B] is more beneficial for [Target].<br><br>3. Cause & Solution Essay:<br>This essay will analyze how [Primary Cause] drives this trend and propose [Key Solutions] to mitigate the issue.</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>English Example:</strong><br><code>Although international air travel boosts global tourism, I firmly contend that governments should impose environmental taxes on aviation companies to counteract excessive carbon emissions and fund renewable energy research.</code></p><p><em>Dịch: Mặc dù du lịch hàng không quốc tế thúc đẩy du lịch toàn cầu, tôi vững tin rằng các chính phủ nên đánh thuế môi trường đối với các công ty hàng không để chống lại lượng khí thải carbon quá mức và tài tài trợ cho nghiên cứu năng lượng tái tạo.</em></p><p><strong>Giải thích:</strong> Câu Thesis vừa công nhận khía cạnh đối lập (boosts global tourism), vừa nêu rõ quan điểm đồng ý đánh thuế với 2 mục đích cụ thể ở thân bài (counteract emissions, fund renewable research).</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Sửa câu Weak Thesis sau thành câu Strong Thesis sắc bén:<br><em>\"Weak: Plastic packaging is bad for the environment and we should stop it.\"</em></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Gợi ý đáp án:</strong> <em>\"Strong: Supermarkets should replace single-use plastic packaging with biodegradable materials to reduce ocean pollution and protect marine ecosystems.\"</em><br>(Giải thích: Đã thay từ chung chung \"bad/stop\" bằng hành động cụ thể \"replace single-use plastic with biodegradable materials\" và nêu 2 lý do rõ ràng).</p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Thesis Statement luôn nằm ở câu cuối cùng của mở bài.</li><li>Tuyệt đối tránh câu mập mờ \"This essay will discuss advantages and disadvantages\".</li><li>Luôn nêu rõ lập trường và tóm tắt 2 hướng triển khai của thân bài.</li></ul>",
                 "ARTICLE", null),

                ("Bài 4: Introduction",
                 "Quy trình viết Mở bài 2 câu hoàn chỉnh (Background Paraphrase + Thesis Statement) bằng 3 kỹ thuật Paraphrase.",
                 "<h3>1. Mục tiêu bài học</h3><p>Nắm vững quy trình viết đoạn Mở bài (Introduction) chuẩn mực 2 câu (40-50 từ). Làm chủ 3 kỹ thuật Paraphrase đề bài để tạo ấn tượng học thuật chuyên nghiệp.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Một đoạn Mở bài học thuật hoàn hảo gồm đúng 2 câu:</p><ul><li><strong>Câu 1 - Background Paraphrase (Viết lại đề bài):</strong> Diễn đạt lại đề bài bằng câu từ mới mà không chép nguyên văn.</li><li><strong>Câu 2 - Thesis Statement (Câu luận đề):</strong> Nêu quan điểm và định hướng nội dung bài viết.</li></ul><p><strong>3 Kỹ thuật Paraphrase Đề bài:</strong></p><ul><li><em>1. Dùng từ đồng nghĩa (Synonyms):</em> e.g., \"artificial intelligence\" -> \"automated intelligent systems\", \"school children\" -> \"primary students\".</li><li><em>2. Biến đổi từ loại (Word Family):</em> e.g., \"develop rapidly\" (Verb+Adv) -> \"rapid development\" (Adj+Noun).</li><li><em>3. Dùng thể bị động hoặc Chủ ngữ giả (Dummy Subject):</em> e.g., \"It is widely believed that...\" / \"Foreign languages are believed to be...\".</li></ul><h3>3. Công thức Mở bài 2 Câu Standards</h3><div class=\"c26-grammar-formula\">Sentence 1 (Background): It is widely observed that [Paraphrased Micro-topic].<br>Sentence 2 (Thesis): In my opinion, [Clear Stance] because [Reason 1] and [Reason 2].</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>Prompt Original:</strong> <code>Some people think that artificial intelligence will replace human teachers in the future.</code></p><p><strong>Paraphrased Introduction (2 Câu):</strong><br><code>It is increasingly asserted that automated intelligent systems might eventually supersede human educators in classrooms. While AI can process vast academic data efficiently, I firmly believe that human teachers remain indispensable due to their capacity for emotional empathy and moral mentorship.</code></p><p><em>Dịch: Đang có quan điểm ngày càng phổ biến rằng các hệ thống trí tuệ tự động cuối cùng có thể thay thế các nhà giáo dục con người trong lớp học. Mặc dù AI có thể xử lý dữ liệu học thuật bao la một cách hiệu quả, tôi vững tin rằng giáo viên con người vẫn không thể thay thế nhờ khả năng thấu hiểu cảm xúc và định hướng đạo đức.</em></p><p><strong>Giải thích:</strong> Câu 1 paraphrase cực kỳ mượt mượt (replace -> supersede, human teachers -> human educators), Câu 2 Thesis khẳng định giáo viên không thể thay thế nhờ 2 lý do (emotional empathy, moral mentorship).</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Viết đoạn Mở bài 2 câu hoàn chỉnh cho đề bài sau:<br><code>\"Some people believe that reading paper books is better for learning than using electronic books (e-books).\"</code></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Gợi ý đáp án:</strong><br><em>\"It is often argued that traditional printed books offer greater educational benefits than digital e-readers. While e-books provide undeniable convenience, I firmly maintain that paper books are superior because they reduce digital eye strain and enhance deep reading comprehension.\"</em></p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Mở bài chuẩn chỉ gồm 2 câu (40-50 từ).</li><li>Câu 1 Paraphrase đề bài, Câu 2 Thesis nêu lập trường.</li><li>Không sao chép từ 3 từ liên tiếp từ đề bài.</li></ul>",
                 "ARTICLE", null),

                ("Bài 5: Body Paragraph + Topic Sentence",
                 "Cách xây dựng cấu trúc đoạn thân bài và kỹ thuật viết câu mở đoạn (Topic Sentence) trực diện, bao quát.",
                 "<h3>1. Mục tiêu bài học</h3><p>Nắm vững cấu trúc tổng thể một đoạn thân bài (Body Paragraph). Biết cách viết câu mở đoạn (Topic Sentence) chuẩn xác, đóng vai trò là \"kim chỉ nam\" cho toàn bộ ý tưởng trong đoạn.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Mỗi đoạn thân bài có độ dài 90-100 từ và tuân thủ nguyên tắc vàng: <strong>1 Đoạn thân bài = 1 Ý tưởng chủ đạo (Single Central Topic)</strong>.</p><p>Cấu trúc đoạn thân bài gồm 3 phần chính:</p><ul><li><strong>1. Topic Sentence (Câu chủ đề):</strong> Đứng ở câu đầu tiên của đoạn, nêu trực tiếp ý chính sẽ chứng minh.</li><li><strong>2. Supporting Sentences (Các câu triển khai):</strong> Giải thích lý do và đưa ví dụ minh họa.</li><li><strong>3. Concluding / Link Sentence (Câu chốt):</strong> Tóm tắt lại tác động hoặc liên kết với Thesis.</li></ul><p><strong>Yêu cầu của câu Topic Sentence tốt:</strong> Không viết câu quá chi tiết (chứa ví dụ) và không viết câu quá chung chung. Phải chứa <em>Topic + Controlling Idea (Ý tưởng kiểm soát)</em>.</p><h3>3. Công thức Viết Topic Sentence</h3><div class=\"c26-grammar-formula\">Formula 1: First and foremost, [Action/Trend] offers substantial benefits in terms of [Controlling Idea].<br>Formula 2: On the one hand, a primary factor driving [Issue] is [Key Cause].</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>Topic Sentence Bad (Quá chi tiết):</strong><br><code>First, governments should build more subways in Hanoi because subways carry 5000 people per hour.</code> (Sai vì đưa số liệu chi tiết vào Topic Sentence).</p><p><strong>Topic Sentence Good (Chuẩn học thuật):</strong><br><code>First and foremost, expanding public mass transit systems significantly alleviates urban traffic congestion.</code></p><p><em>Dịch: Đầu tiên và quan trọng nhất, việc mở rộng các hệ thống giao thông công cộng khối lượng lớn giúp giảm đáng kể sự ùn tắc giao thông đô thị.</em></p><p><strong>Giải thích:</strong> Câu tốt nêu rõ Topic (expanding public transit) và Controlling Idea (alleviates urban congestion), tạo tiền đề cho các câu sau giải thích lý do và đưa ví dụ.</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Viết 2 câu Topic Sentence cho 2 đoạn thân bài của đề bài <em>\"Benefits of Renewable Energy\"</em> (Đoạn 1: Giảm ô nhiễm; Đoạn 2: Đảm bảo an ninh năng lượng lâu dài).</p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Gợi ý đáp án:</strong><br>- Body 1 Topic Sentence: <em>\"On the one hand, transitioning to renewable energy sources plays a pivotal role in mitigating environmental pollution.\"</em><br>- Body 2 Topic Sentence: <em>\"Furthermore, clean energy adoption enhances long-term national energy security.\"</em></p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Topic Sentence luôn là câu đầu tiên của đoạn thân bài.</li><li>1 đoạn thân bài chỉ tập trung chứng minh 1 ý chính duy nhất.</li><li>Không đưa số liệu hoặc ví dụ cụ thể vào câu Topic Sentence.</li></ul>",
                 "ARTICLE", null),

                ("Bài 6: Supporting Ideas + Examples + Explanation",
                 "Kỹ thuật phát triển lập luận sâu bằng chuỗi \"Why & How\" và cách đưa Ví dụ minh họa thực tế thuyết phục.",
                 "<h3>1. Mục tiêu bài học</h3><p>Làm chủ kỹ thuật triển khai ý tưởng sau câu Topic Sentence. Phân biệt giữa đoạn văn bị lỗi liệt kê ý tưởng (Idea Listing) và đoạn văn phát triển lập luận sâu sắc bằng Explanation & Example.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Sau khi có câu Topic Sentence, người viết cần phát triển đoạn bằng 2 thành phần chủ chốt:</p><ul><li><strong>1. Explanation (Giải thích):</strong> Trả lời câu hỏi \"Why is this true?\" và \"How does this work?\". Xây dựng chuỗi nguyên nhân - kết quả (Cause-Effect Chain) thay vì liệt kê 3-4 ý nhỏ hời hợt.</li><li><strong>2. Specific Example (Ví dụ cụ thể):</strong> Đưa ra bằng chứng minh họa thực tế (Tên quốc gia, tổ chức, số liệu nghiên cứu, case study). Tránh đưa ví dụ chung chung như \"some people I know\".</li></ul><h3>3. Công thức Triển khai Đoạn Thân bài T.E.E.L</h3><div class=\"c26-grammar-formula\">- Topic Sentence: [Main Point]<br>- Explanation: This is because [Cause], which means that [Mechanism], ultimately resulting in [Effect].<br>- Example: A clear illustration of this is [Specific Case / Data].</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>Paragraph with Idea Listing (Lỗi liệt kê hời hợt):</strong><br><code>Plastic bags are bad. They pollute oceans, kill fish, look ugly, and take 500 years to decompose. Also people litter them everywhere.</code> (Dở vì chỉ liệt kê ý mà không giải thích cơ chế).</p><p><strong>Deeply Developed Paragraph (Triển khai sâu sắc):</strong><br><code>Banning single-use plastic packaging substantially reduces marine environmental degradation. This is primarily because non-biodegradable synthetic polymers persist in aquatic ecosystems for centuries, breaking down into toxic microplastics that enter the marine food chain. A prominent example is the Pacific Trash Vortex, where accumulated plastic waste threatens over 700 marine species.</code></p><p><em>Dịch: Việc cấm bao bì nhựa dùng một lần làm giảm đáng kể sự suy thoái môi trường biển. Điều này chủ yếu là do các polymer tổng hợp không phân hủy sinh học tồn tại trong hệ sinh thái dưới nước qua nhiều thế kỷ, phân hủy thành các mảnh vi nhựa độc hại đi vào chuỗi thức ăn biển. Một ví dụ nổi bật là Xoáy rác Thái Bình Dương, nơi rác thải nhựa tích tụ đe dọa hơn 700 loài sinh vật biển.</em></p><p><strong>Giải thích:</strong> Đoạn văn cực kỳ sâu sắc vì giải thích cơ chế vi nhựa (microplastics -> food chain) và đưa dẫn chứng thực tế (Pacific Trash Vortex, 700 species).</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Viết câu Explanation và Example cho câu Topic Sentence sau:<br><em>\"Topic Sentence: Space exploration encourages technological innovations that benefit daily human life.\"</em></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Gợi ý đáp án:</strong><br>- Explanation: <em>\"This is because scientists developing space equipment must create lightweight, durable materials, which are subsequently adapted for commercial applications.\"</em><br>- Example: <em>\"For instance, memory foam originally developed by NASA to cushion astronauts during spaceflights is now widely utilized in modern medical mattresses.\"</em></p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Thà giải thích 1 ý tưởng đến cùng còn hơn liệt kê 3 ý tưởng qua loa.</li><li>Luôn dùng chuỗi \"Cause -> Mechanism -> Effect\" để giải thích.</li><li>Ví dụ phải cụ thể (tên tổ chức, sản phẩm, con số) để tăng độ tin cậy.</li></ul>",
                 "ARTICLE", null),

                ("Bài 7: Linking Words + Cohesion",
                 "Hệ thống từ nối học thuật theo nhóm, quy tắc dấu câu và kỹ thuật dùng đại từ liên kết (Referencing Pronouns).",
                 "<h3>1. Mục tiêu bài học</h3><p>Làm chủ các nhóm từ nối (Transition Words) và kỹ thuật tham chiếu bằng đại từ (Referencing Devices) giúp bài viết đạt điểm tối đa tiêu chí Coherence & Cohesion mà không bị lỗi lạm dụng từ nối.</p><h3>2. Lý thuyết ngắn gọn</h3><p>Mạch logic của bài luận được giữ vững nhờ 2 công cụ chính:</p><ul><li><strong>1. Linking Words (Từ nối theo nhóm):</strong><br>- <em>Addition (Bổ sung):</em> Furthermore, Moreover, In addition.<br>- <em>Contrast (Đối lập):</em> However, Nevertheless, Conversely, On the other hand.<br>- <em>Cause & Effect (Hệ quả):</em> Consequently, Therefore, As a result, Thus.<br>- <em>Illustration (Ví dụ):</em> For instance, A pertinent example is...</li><li><strong>2. Referencing Pronouns (Đại từ thay thế):</strong> Dùng <em>This trend, These measures, Such practices, This phenomenon</em> để nối câu tự nhiên mà không cần dùng quá nhiều từ nối đứng đầu câu.</li></ul><p><strong>Quy tắc dấu câu:</strong> <code>Linking Word, + Clause.</code> hoặc <code>Clause; linking word, clause.</code></p><h3>3. Công thức Nối Câu Tự Nhiên bằng Referencing</h3><div class=\"c26-grammar-formula\">Pattern 1: Sentence A (Problem/Action). This + Noun (This trend / This policy) + Verb + Effect.<br>Pattern 2: Clause A; consequently, Clause B.</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>English Example:</strong><br><code>Financial institutions are increasingly implementing AI-driven fraud detection algorithms. This technological integration allows banks to analyze millions of transactions in real time; consequently, fraudulent activities are identified before major financial losses occur.</code></p><p><em>Dịch: Các định chế tài chính đang ngày càng triển khai các thuật toán phát hiện gian lận dựa trên AI. Sự tích hợp công nghệ này cho phép các ngân hàng phân tích hàng triệu giao dịch theo thời gian thực; do đó, các hoạt động gian lận được phát hiện trước khi tổn thất tài chính lớn xảy ra.</em></p><p><strong>Giải thích:</strong> Cụm <em>\"This technological integration\"</em> thay thế mượt mà cho câu 1, kết hợp dấu chấm phẩy và từ nối <em>\"; consequently,\"</em> tạo tính gắn kết học thuật cao.</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Điền từ nối hoặc cụm đại từ thích hợp vào chỗ trống:<br><em>\"Many coastal cities face rising sea levels due to climate change. __________ threatens urban infrastructure; __________, city planners are constructing seawalls.\"</em></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Đáp án:</strong><br>- Chỗ trống 1: <strong>This environmental phenomenon</strong> (hoặc <em>This trend</em>).<br>- Chỗ trống 2: <strong>consequently</strong> (hoặc <em>therefore / as a result</em>).</p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Không lạm dụng đặt từ nối đứng đầu ở 100% các câu văn.</li><li>Sử dụng linh hoạt đại từ chỉ định (This policy, These issues) để nối câu.</li><li>Nhớ đặt dấu phẩy sau các trạng từ nối đứng đầu câu.</li></ul>",
                 "ARTICLE", null),

                ("Bài 8: Academic Vocabulary + Grammar",
                 "Nâng cấp từ vựng AWL B1/B2, kỹ thuật Hedging (nói giảm nói tránh) và sử dụng thể bị động/mệnh đề rút gọn.",
                 "<h3>1. Mục tiêu bài học</h3><p>Chuyển đổi từ vựng giao tiếp thông thường sang bộ từ vựng Academic Word List (AWL). Thành thạo kỹ thuật <strong>Hedging (Khách quan hóa)</strong> và ngữ pháp nâng cao (Bị động, Mệnh đề quan hệ rút gọn).</p><h3>2. Lý thuyết ngắn gọn</h3><p>Văn phong bài luận học thuật đòi hỏi tính trang trọng và khách quan:</p><ul><li><strong>1. Academic Word List (AWL):</strong><br>- Thay <em>solve</em> -> <em>tackle / resolve</em><br>- Thay <em>big</em> -> <em>substantial / considerable / profound</em><br>- Thay <em>show</em> -> <em>demonstrate / illustrate / reveal</em><br>- Thay <em>help</em> -> <em>facilitate / foster / encourage</em></li><li><strong>2. Kỹ thuật Hedging (Nói giảm nói tránh):</strong> Trong văn phong khoa học, tránh khẳng định tuyệt đối (always, 100% true). Dùng các từ như: <em>tend to, appear to, is likely to, evidence suggests that...</em></li><li><strong>3. Ngữ pháp nâng cao:</strong> Thể bị động (It is recognized that...) và Mệnh đề quan hệ rút gọn (reducing active V-ing / passive V3).</li></ul><h3>3. Công thức Nâng Cấp Câu Học Thuật</h3><div class=\"c26-grammar-formula\">Informal: I think X always causes Y.<br>Academic B2: Evidence suggests that X tends to contribute significantly to Y.<br>Reduced Clause: Policies aimed at reducing emissions... (thay vì Policies which aim at...)</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>Informal Style (Chưa đạt):</strong><br><code>I think tourists always make historical places dirty and break old buildings.</code></p><p><strong>Academic Formal B2 (Chuẩn mực):</strong><br><code>It is observed that unsustainable tourist inflows tend to accelerate the degradation of historic monuments through environmental littering and physical wear.</code></p><p><em>Dịch: Người ta quan sát thấy rằng dòng du khách không bền vững có xu hướng làm tăng nhanh sự suy thoái của các di tích lịch sử thông qua việc xả rác môi trường và sự mài mòn vật lý.</em></p><p><strong>Giải thích:</strong> Câu nâng cấp thay \"I think\" bằng \"It is observed that\", thay \"always make dirty\" bằng \"tend to accelerate the degradation through littering\", từ vựng AWL cực kỳ đẳng cấp.</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Nâng cấp câu suồng sã sau thành câu Academic Formal B2 có sử dụng Hedging:<br><em>\"Using smartphones too much always makes students get bad exam results.\"</em></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Gợi ý đáp án:</strong> <em>\"Evidence indicates that excessive smartphone usage is likely to impede academic performance among secondary students.\"</em><br>(Từ nâng cấp: <em>using too much -> excessive usage, always makes -> is likely to, get bad exam results -> impede academic performance</em>).</p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Sử dụng từ vựng AWL thay cho phrasal verbs và từ suồng sã.</li><li>Luôn dùng kỹ thuật Hedging (tends to, is likely to) để lập luận mang tính khoa học.</li><li>Rút gọn mệnh đề quan hệ để câu văn súc tích và mượt mà hơn.</li></ul>",
                 "ARTICLE", null),

                ("Bài 9: Conclusion + Proofreading",
                 "Cách viết Kết bài 2 câu chuẩn mực, quy tắc KHÔNG đưa ý mới và quy trình Proofreading 4 bước soát lỗi.",
                 "<h3>1. Mục tiêu bài học</h3><p>Viết đoạn Kết bài (Conclusion) 2 câu (30-40 từ) tóm tắt lại bài luận ấn tượng. Làm chủ quy trình 4 bước Proofreading để loại bỏ sạch lỗi sai ngữ pháp, chính tả trước khi nộp bài.</p><h3>2. Lý thuyết ngắn gọn</h3><p><strong>1. Đoạn Kết bài (Conclusion):</strong><br>- Độ dài: 2 câu (30-40 từ), bắt đầu bằng cụm <em>\"In conclusion,\"</em>.<br>- <em>Câu 1: Restate Thesis Statement</em> (Khẳng định lại lập trường bằng cách Paraphrase).<br>- <em>Câu 2: Summary / Final Outlook</em> (Tóm tắt lại 2 điểm chính hoặc đưa ra khuyến nghị tương lai).<br>- <strong>Quy tắc vàng:</strong> Tuyệt đối KHÔNG đưa ý tưởng mới chưa được giải thích ở thân bài vào kết bài.</p><p><strong>2. Quy trình Proofreading 4 bước (5 phút cuối):</strong><br>- <em>Bước 1:</em> Spelling & Capitalization (Chính tả & Viết hoa đầu câu/tên riêng).<br>- <em>Bước 2:</em> Subject-Verb Agreement (Sự hòa hợp chủ - vị: danh từ số nhiều đi với động từ số nhiều).<br>- <em>Bước 3:</em> Tense & Word Forms (Dùng đúng thì và đúng từ loại Noun/Verb/Adj/Adv).<br>- <em>Bước 4:</em> Punctuation & Coherence (Dấu phẩy sau từ nối, dấu chấm câu).</p><h3>3. Công thức Kết bài 2 Câu Standard</h3><div class=\"c26-grammar-formula\">Sentence 1: In conclusion, while [Counter-view], I reiterate that [Restated Thesis].<br>Sentence 2: Educational institutions and governments should therefore collaborate to [Final Recommendation].</div><h3>4. Ví dụ tiếng Anh + Dịch + Giải thích</h3><p><strong>English Example (Conclusion):</strong><br><code>In conclusion, although prolonged working hours may offer temporary productivity gains, I reiterate that excessive workload severely compromises employee health and long-term organizational efficiency. Employers should therefore prioritize flexible scheduling to foster a sustainable workplace environment.</code></p><p><em>Dịch: Tóm lại, mặc dù giờ làm việc kéo dài có thể mang lại mức tăng năng suất tạm thời, tôi khẳng định lại rằng khối lượng công việc quá mức làm ảnh hưởng nghiêm trọng đến sức khỏe nhân viên và hiệu quả lâu dài của tổ chức. Do đó, người sử dụng lao động nên ưu tiên lịch trình linh hoạt để nuôi dưỡng môi trường làm việc bền vững.</em></p><p><strong>Giải thích:</strong> Kết bài bắt đầu bằng <em>In conclusion</em>, nhượng bộ ý đối lập (temporary gains), khẳng định lại Thesis (compromises health), và câu 2 đưa ra khuyến nghị thiết thực.</p><h3>5. Bài tập thực hành</h3><p><strong>Bài tập:</strong> Tìm và sửa 3 lỗi sai trong đoạn văn sau:<br><em>\"In conclusion electric cars is very good for environment. Consequently people should buy it to reduce pollution.\"</em></p><h3>6. Đáp án & Hướng dẫn giải</h3><p><strong>Đáp án:</strong><br>- Lỗi 1: Thiếu dấu phẩy sau từ nối -> <em>In conclusion,</em><br>- Lỗi 2: Sai hòa hợp chủ vị -> <em>electric cars ARE</em> (không dùng is).<br>- Lỗi 3: Sai đại từ thay thế -> <em>buy THEM</em> (thay cho danh từ số nhiều electric cars).<br>-> Câu chuẩn: <em>\"In conclusion, electric vehicles offer substantial environmental advantages. Consequently, governments should encourage consumers to adopt them.\"</em></p><h3>7. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Kết bài gồm 2 câu: Câu 1 khẳng định lại Thesis, Câu 2 đưa khuyến nghị.</li><li>Không đưa bất kỳ lý do hoặc ý tưởng mới nào vào đoạn kết bài.</li><li>Luôn dành 3-5 phút cuối để rà soát lỗi chính tả và chia động từ.</li></ul>",
                 "ARTICLE", null),

                ("Bài 10: Viết một bài Essay hoàn chỉnh từ A–Z",
                 "Quy trình 40 phút hoàn thành bài Essay tiêu chuẩn 250-300 từ từ phân tích đề, lập dàn ý đến bài viết B2 hoàn chỉnh.",
                 "<h3>1. Mục tiêu bài học</h3><p>Tổng hợp toàn bộ kỹ năng từ Bài 1 đến Bài 9 để tự tay hoàn thành 1 bài luận Academic Essay hoàn chỉnh (250-300 từ) trong thời gian 40 phút thi cử chuẩn mực.</p><h3>2. Lý thuyết ngắn gọn</h3><p><strong>Bảng Phân Bổ Thời Gian 40 Phút Thi Viết Essay:</strong></p><ul><li><strong>Phút 0 - 3 (Phân tích đề 3T):</strong> Xác định Topic, Micro-topic, Task Command.</li><li><strong>Phút 3 - 8 (Lập dàn ý 4 đoạn):</strong> Viết nháp Topic Sentence và 2 ý hỗ trợ cho Body 1 & Body 2.</li><li><strong>Phút 8 - 35 (Viết bài hoàn chỉnh):</strong> Viết Mở bài (2 câu), Thân bài 1 (T.E.E.L), Thân bài 2 (T.E.E.L), Kết bài (2 câu).</li><li><strong>Phút 35 - 40 (Proofread):</strong> Rà soát 4 bước sửa lỗi chính tả, chia động từ và từ nối.</li></ul><h3>3. Master Essay Outline Template</h3><div class=\"c26-grammar-formula\">Master Essay Structure (250-300 words):<br>- Paragraph 1: Intro (Background Paraphrase + Thesis Statement)<br>- Paragraph 2: Body 1 (Topic Sentence 1 + Explanation + Specific Example + Link)<br>- Paragraph 3: Body 2 (Topic Sentence 2 + Explanation + Specific Example + Link)<br>- Paragraph 4: Conclusion (Restate Thesis + Final Recommendation)</div><h3>4. Bài luận mẫu B2 hoàn chỉnh (Full Master Sample Essay)</h3><p><strong>Prompt:</strong> <code>Some people believe that traditional local crafts are no longer relevant in modern industrial societies. To what extent do you agree or disagree?</code></p><p><strong>Full Master Sample Essay (270 từ):</strong><br><code>It is increasingly argued that traditional artisanal crafts have lost their significance in contemporary industrialized economies. While mass-produced goods offer superior affordability, I firmly contend that heritage handicrafts remain vital due to their cultural preservation value and economic contribution to niche tourism.</code><br><br><code>First and foremost, traditional crafts serve as indispensable tangible repositories of cultural identity. Unlike automated factory items, handmade products encapsulate centuries of ancestral techniques and regional storytelling. For instance, traditional silk weaving in Vietnam not only preserves indigenous artistic heritage but also passes historical craftsmanship to younger generations. Consequently, safeguarding these artisanal practices sustains national cultural diversity.</code><br><br><code>Furthermore, local craft industries foster sustainable economic development through cultural tourism. Global travelers actively seek authentic cultural experiences, creating high demand for unique handmade souvenirs. A pertinent example is Kyoto, Japan, where traditional pottery and textile workshops generate substantial revenue while providing stable employment for local artisans. Therefore, traditional crafts play a crucial role in modern economic diversification.</code><br><br><code>In conclusion, although industrial manufacturing provides cheap consumer goods, I reiterate that traditional handicrafts remain essential for cultural preservation and tourism growth. Governments should therefore implement financial subsidies to support local craft communities in the digital era.</code></p><p><em>Dịch bài luận mẫu:</em><br>Đang có quan điểm ngày càng phổ biến rằng các nghề thủ công truyền thống đã mất đi tầm quan trọng trong các nền kinh tế công nghiệp hóa hiện đại. Mặc dù hàng hóa sản xuất hàng loạt mang lại giá cả phải chăng hơn, tôi vững tin rằng các sản phẩm thủ công di sản vẫn đóng vai trò thiết yếu nhờ giá trị bảo tồn văn hóa và đóng góp kinh tế cho du lịch đặc thù... (Xem bài dịch toàn văn trong tài liệu bài giảng).</p><h3>5. Bài tập thực hành tự luyện</h3><p><strong>Đề bài thực hành hoàn chỉnh:</strong><br><code>\"Some people think that governments should prioritize funding for scientific research over funding for the arts. To what extent do you agree or disagree?\"</code><br>Hãy thực hiện đầy đủ 4 bước (Phân tích 3T -> Lập dàn ý 4 đoạn -> Viết bài 250-300 từ -> Proofread) để hoàn thành bài essay của bạn.</p><h3>6. Phần ghi nhớ cuối bài (Key Takeaways)</h3><ul><li>Phân bổ 40 phút nghiêm ngặt: 5 phút chuẩn bị, 30 phút viết, 5 phút soát lỗi.</li><li>Đảm bảo bài essay có bố cục 4 đoạn rõ ràng và đạt từ 250 - 300 từ.</li><li>Chúc mừng bạn đã làm chủ toàn bộ kỹ năng viết luận tiếng Anh học thuật!</li></ul>",
                 "ARTICLE", null)
            }
        };
    }
}


