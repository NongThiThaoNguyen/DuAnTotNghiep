using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DuAnTotNghiep.Data;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.Models.ViewModels.AILearn;
using DuAnTotNghiep.Services.Interfaces;

namespace DuAnTotNghiep.Services
{
    public class StudentLessonService : IStudentLessonService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGamificationService _gamificationService;

        public StudentLessonService(ApplicationDbContext context, IGamificationService gamificationService)
        {
            _context = context;
            _gamificationService = gamificationService;
        }

        public async Task<LessonViewModel?> GetLessonDetailAsync(int lessonId, int userId)
        {
            var lesson = await _context.OriginalLessons
                .Include(l => l.Topic)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null) return null;

            var siblings = await _context.OriginalLessons
                .Where(l => l.TopicId == lesson.TopicId)
                .OrderBy(l => l.Id)
                .ToListAsync();

            var completedLessonIds = await _context.StudyActivityLogs
                .Where(log => log.StudentId == userId && log.TopicId == lesson.TopicId && (log.ActivityType == "LESSON" || log.ActivityType == "ARTICLE"))
                .Select(log => log.LearningPathNodeId ?? 0)
                .Distinct()
                .ToListAsync();

            var lessonNav = siblings.Select((sib, index) => new LessonNavigationItemViewModel
            {
                Id = sib.Id,
                Title = sib.Title,
                OrderIndex = index + 1,
                IsCurrent = sib.Id == lessonId,
                IsCompleted = completedLessonIds.Contains(sib.Id)
            }).ToList();

            int currentIndex = siblings.FindIndex(s => s.Id == lessonId);
            int? prevId = currentIndex > 0 ? siblings[currentIndex - 1].Id : null;
            int? nextId = currentIndex < siblings.Count - 1 ? siblings[currentIndex + 1].Id : null;

            var isCompleted = completedLessonIds.Contains(lessonId);

            string? videoUrl = lesson.VideoUrl;
            if (lesson.ContentType == "VIDEO_LINK" && string.IsNullOrEmpty(videoUrl))
            {
                videoUrl = lesson.Content; // Fallback to old behavior
            }

            return new LessonViewModel
            {
                CourseId = lesson.TopicId,
                CourseTitle = lesson.Topic?.Title ?? "Khóa học",
                LessonId = lesson.Id,
                LessonTitle = lesson.Title,
                Summary = lesson.Summary ?? "Tóm tắt bài học tiếng Anh hữu ích dành cho bạn.",
                Content = lesson.Content ?? "<p>Nội dung đang được cập nhật...</p>",
                ContentType = lesson.ContentType,
                VideoUrl = videoUrl,
                EstimatedMinutes = lesson.EstimatedMinutes ?? 15,
                IsCompleted = isCompleted,
                GrammarExamples = GenerateGrammarExamples(lesson),
                Resources = GenerateResources(lesson),
                LessonsInCourse = lessonNav,
                PreviousLessonId = prevId,
                NextLessonId = nextId
            };
        }

        private List<LessonGrammarExampleViewModel> GenerateGrammarExamples(OriginalLesson lesson)
        {
            var list = new List<LessonGrammarExampleViewModel>();
            string title = lesson.Title ?? "";
            string topicTitle = lesson.Topic?.Title ?? "";

            if (title.Contains("Form-filling") || title.Contains("Số") || title.Contains("Tên"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc điền thông tin Cá nhân (Form-filling)",
                    Syntax = "Name + Spelling (A-Z) / Number + Dialing",
                    Example = "A: Could I take your full name? B: It's Sarah Jenkins, J-E-N-K-I-N-S.",
                    Translation = "A: Tôi xin tên đầy đủ của bạn? B: Là Sarah Jenkins, đánh vần J-E-N-K-I-N-S.",
                    Explanation = "Chú ý trọng âm phát âm con số 13/30 và cách phát âm các chữ cái dễ nhầm lẫn (A/E/I, G/J)."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc địa chỉ & Mã bưu điện",
                    Syntax = "House Number + Street Name + Postcode",
                    Example = "124 Oxford Street, London, Postcode: W1D 1BS.",
                    Translation = "Số 124 Đường Oxford, London, Mã bưu điện: W1D 1BS.",
                    Explanation = "Chữ số '0' thường được đọc là 'oh' hoặc 'zero' trong số điện thoại."
                });
            }
            else if (title.Contains("Skimming") || title.Contains("Reading") || title.Contains("Đọc hiểu"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc câu chủ đề (Topic Sentence)",
                    Syntax = "Subject + Main Verb + Key Idea",
                    Example = "The rapid expansion of AI has transformed modern industries.",
                    Translation = "Sự phát triển nhanh chóng của AI đã thay đổi các ngành công nghiệp hiện đại.",
                    Explanation = "Đọc câu đầu tiên của đoạn văn để xác định ngay ý chính (Main Idea)."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Mệnh đề đối lập (Contrast Clauses)",
                    Syntax = "While / Although + Clause 1, Clause 2",
                    Example = "While automation enhances efficiency, skeptics express concern over job loss.",
                    Translation = "Trong khi tự động hóa tăng hiệu suất, người hoài nghi lo ngại mất việc làm.",
                    Explanation = "Chú ý các từ nối chuyển ý như While, However, Consequently để xác định quan điểm."
                });
            }
            else if (title.Contains("Line Graph") || title.Contains("Task 1") || title.Contains("Biểu đồ"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc mô tả xu hướng tăng/giảm (Trends)",
                    Syntax = "S + experience / see + a/an + Adj + Noun + in + Data",
                    Example = "Hamburger consumption experienced a dramatic upward trend from 1970 to 1990.",
                    Translation = "Mức tiêu thụ hamburger đã trải qua một xu hướng tăng mạnh từ 1970 đến 1990.",
                    Explanation = "Có thể linh hoạt đổi sang dạng Động từ + Trạng từ: 'Hamburger consumption increased dramatically'."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc so sánh dữ liệu (Data Comparison)",
                    Syntax = "Compared to / In comparison with + Noun, S + Verb",
                    Example = "The figure for pizza stood at 50%, compared to only 20% for fish and chips.",
                    Translation = "Con số của pizza đạt 50%, so với chỉ 20% của cá và khoai tây chiên.",
                    Explanation = "Dùng cụm 'the figure for...' để tránh lặp lại danh từ chính nhiều lần."
                });
            }
            else if (title.Contains("Present Simple") || title.Contains("Hiện tại đơn"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Thì Hiện tại đơn diễn tả thói quen (Habits)",
                    Syntax = "S + Adverb of Frequency (always/usually) + V(s/es)",
                    Example = "She always drinks green tea in the morning before working.",
                    Translation = "Cô ấy luôn uống trà xanh vào buổi sáng trước khi làm việc.",
                    Explanation = "Trạng từ chỉ tần suất luôn đứng trước động từ thường và đứng sau To Be."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Thì Hiện tại đơn diễn tả sự thật hiển nhiên (Facts)",
                    Syntax = "S + V(s/es) + Object",
                    Example = "Water boils at 100 degrees Celsius under normal pressure.",
                    Translation = "Nước sôi ở 100 độ C dưới áp suất bình thường.",
                    Explanation = "Không sử dụng thì tiếp diễn cho các quy luật tự nhiên và sự thật khoa học."
                });
            }
            else if (title.Contains("Present Continuous") || title.Contains("Hiện tại tiếp diễn"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc hành động đang diễn ra (Action at present)",
                    Syntax = "S + am/is/are + V-ing + (at the moment / now)",
                    Example = "They are currently conducting a research project in the lab.",
                    Translation = "Họ hiện đang tiến hành một dự án nghiên cứu trong phòng thí nghiệm.",
                    Explanation = "Nhớ nhân đôi phụ âm cuối với động từ 1 âm tiết kết thúc bằng 1 nguyên âm + 1 phụ âm (run -> running)."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc phàn nàn với 'Always'",
                    Syntax = "S + am/is/are + always + V-ing",
                    Example = "He is always forgetting to turn off the lights when leaving.",
                    Translation = "Anh ấy toàn quên tắt đèn khi đi ra ngoài.",
                    Explanation = "Diễn tả thói quen xấu gây bực mình cho người nói."
                });
            }
            else if (title.Contains("Family") || title.Contains("Gia đình"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc miêu tả ngoại hình & tính cách giống người thân",
                    Syntax = "S + take after + Somebody (in + Aspect)",
                    Example = "I take after my father in both appearance and work ethic.",
                    Translation = "Tôi giống bố tôi cả về ngoại hình lẫn đạo đức nghề nghiệp.",
                    Explanation = "'Take after' là phrasal verb thông dụng nghĩa là có nét giống ai đó trong gia đình."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc nuôi dưỡng & lớn lên",
                    Syntax = "S + be + brought up / raised + in/by + Someone",
                    Example = "She was brought up in a loving and supportive nuclear family.",
                    Translation = "Cô ấy được nuôi dưỡng trong một gia đình nhỏ giàu tình thương và luôn hỗ trợ nhau.",
                    Explanation = "Phân biệt 'Nuclear family' (Gia đình 2 thế hệ) và 'Extended family' (Gia đình đa thế hệ)."
                });
            }
            else
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = $"1. Cấu trúc trọng tâm: {title}",
                    Syntax = "Subject + Key Verb + Object / Complement",
                    Example = $"Mastering {title} requires consistent daily practice.",
                    Translation = $"Thành thạo {title} đòi hỏi phải luyện tập đều đặn hàng ngày.",
                    Explanation = $"Áp dụng linh hoạt cấu trúc này khi thực hành bài tập chủ đề {topicTitle}."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc mở rộng phản xạ (Applied Sentence Pattern)",
                    Syntax = "It is + Adjective + for + Somebody + to + V-bare",
                    Example = "It is essential for learners to apply grammar rules in real communication.",
                    Translation = "Điều thiết yếu là người học phải áp dụng các quy tắc ngữ pháp vào giao tiếp thực tế.",
                    Explanation = "Sử dụng cấu trúc 'It is + Adj + for Sb to V' giúp câu văn trôi chảy và chuyên nghiệp hơn."
                });
            }

            return list;
        }

        private List<LessonResourceViewModel> GenerateResources(OriginalLesson lesson)
        {
            return new List<LessonResourceViewModel>
            {
                new LessonResourceViewModel
                {
                    Title = $"Tài liệu tóm tắt bài giảng - {lesson.Title}.pdf",
                    FileType = "PDF",
                    FileUrl = "#"
                },
                new LessonResourceViewModel
                {
                    Title = $"Audio luyện phát âm & ví dụ - {lesson.Title}.mp3",
                    FileType = "MP3",
                    FileUrl = "#"
                }
            };
        }

        public async Task<bool> MarkLessonCompletedAsync(int lessonId, int userId)
        {
            var lesson = await _context.OriginalLessons.FindAsync(lessonId);
            if (lesson == null) return false;

            var existingLog = await _context.StudyActivityLogs
                .FirstOrDefaultAsync(l => l.StudentId == userId && l.TopicId == lesson.TopicId && l.LearningPathNodeId == lessonId && l.ActivityType == "LESSON");

            if (existingLog == null)
            {
                var log = new StudyActivityLog
                {
                    StudentId = userId,
                    ActivityType = "LESSON",
                    TopicId = lesson.TopicId,
                    LearningPathNodeId = lessonId,
                    DurationMinutes = lesson.EstimatedMinutes ?? 15,
                    CreatedAt = DateTime.UtcNow
                };
                _context.StudyActivityLogs.Add(log);
                await _context.SaveChangesAsync();
            }

            // Gamification hook
            await _gamificationService.CheckAndGrantAchievementsAsync(userId);

            return true;
        }
    }
}
