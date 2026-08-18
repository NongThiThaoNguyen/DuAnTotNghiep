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
                if (title.Contains("Tổng quan") || title.Contains("Bài 1"))
                {
                    list.Add(new LessonGrammarExampleViewModel
                    {
                        Title = "1. Diễn tả thói quen hằng ngày (Daily Routines)",
                        Syntax = "S + Adverb of Frequency (usually/always) + V(s/es)",
                        Example = "I usually drink warm water every morning after waking up.",
                        Translation = "Tôi thường uống nước ấm mỗi sáng sau khi thức dậy.",
                        Explanation = "Trạng từ 'usually' đứng trước động từ thường 'drink' và đi kèm 'every morning' để diễn tả thói quen."
                    });
                    list.Add(new LessonGrammarExampleViewModel
                    {
                        Title = "2. Diễn tả sự thật hiển nhiên (General Truths)",
                        Syntax = "Subject (Nature/Science) + V(s/es) + Complement",
                        Example = "The sun rises in the East and sets in the West.",
                        Translation = "Mặt trời mọc ở hướng Đông và lặn ở hướng Tây.",
                        Explanation = "Sử dụng Hiện tại đơn cho quy luật thiên nhiên và sự thật vĩnh cửu."
                    });
                }
                else if (title.Contains("Cấu trúc") || title.Contains("Bài 2"))
                {
                    list.Add(new LessonGrammarExampleViewModel
                    {
                        Title = "1. Quy tắc thêm đuôi -es với động từ tận cùng -ch",
                        Syntax = "He / She / It + V(-es)",
                        Example = "She watches her favorite cartoon every afternoon.",
                        Translation = "Cô ấy xem bộ phim hoạt hình yêu thích mỗi buổi chiều.",
                        Explanation = "Động từ 'watch' kết thúc bằng 'ch' nên phải thêm '-es' khi đi với chủ ngữ số ít 'She'."
                    });
                    list.Add(new LessonGrammarExampleViewModel
                    {
                        Title = "2. Cấu trúc câu hỏi nghi vấn với Trợ động từ Does",
                        Syntax = "Does + He/She/It + V-bare?",
                        Example = "Does Peter study English on Tuesday nights?",
                        Translation = "Peter có học tiếng Anh vào tối thứ Ba không?",
                        Explanation = "Khi đã mượn trợ động từ 'Does', động từ chính 'study' bắt buộc ở dạng nguyên mẫu."
                    });
                }
                else if (title.Contains("Thực hành") || title.Contains("Bài 3"))
                {
                    list.Add(new LessonGrammarExampleViewModel
                    {
                        Title = "1. Mẫu câu hỏi giao tiếp về thời gian sinh hoạt",
                        Syntax = "What time + do/does + S + V-bare?",
                        Example = "What time do you usually wake up in the morning?",
                        Translation = "Cậu thường thức dậy lúc mấy giờ vào buổi sáng?",
                        Explanation = "Mẫu câu hỏi thói quen phổ biến nhất trong giao tiếp hằng ngày."
                    });
                    list.Add(new LessonGrammarExampleViewModel
                    {
                        Title = "2. Mẫu câu phủ định phản hồi thói quen",
                        Syntax = "S + do/does + NOT + V-bare, so + Clause",
                        Example = "My sister does not like spicy food, so she always orders salad.",
                        Translation = "Chị tôi không thích đồ ăn cay, nên chị ấy luôn gọi món salad.",
                        Explanation = "Mượn 'does not' cho chủ ngữ số ít 'My sister', động từ 'like' giữ nguyên mẫu."
                    });
                }
                else
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
            else if (title.Contains("Academic Essay") || title.Contains("Tổng quan"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc câu diễn đạt sự ảnh hưởng sâu rộng",
                    Syntax = "Advancements in + Field + have significantly enhanced + Direct Object",
                    Example = "Advancements in medical robotics have significantly enhanced surgical precision, thereby reducing human error.",
                    Translation = "Những tiến bộ trong robot y tế đã làm tăng đáng kể độ chính xác phẫu thuật, từ đó giảm thiểu lỗi con người.",
                    Explanation = "Cấu trúc khách quan sử dụng động từ mạnh 'enhanced' và trạng từ liên kết 'thereby' chuẩn Academic Formal."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc liên kết hệ quả với từ vựng AWL",
                    Syntax = "Frequent consumption of + Item + is associated with + Elevated Risks",
                    Example = "Frequent consumption of highly processed fast food is associated with elevated health risks among urban residents.",
                    Translation = "Việc tiêu thụ thường xuyên thực phẩm chế biến sẵn có liên quan đến nguy cơ sức khỏe gia tăng ở cư dân đô thị.",
                    Explanation = "Thay vì dùng từ suồng sã 'make people sick', dùng cụm từ học thuật 'associated with elevated health risks'."
                });
            }
            else if (title.Contains("Phân tích đề") || title.Contains("xác định yêu cầu"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc mở đầu phân tích hiện tượng xã hội",
                    Syntax = "Nouns / Trend + has increased / developed dramatically in + Region",
                    Example = "Childhood obesity has increased dramatically in many developed countries in recent decades.",
                    Translation = "Béo phì ở trẻ em đã gia tăng một cách đột biến tại nhiều quốc gia phát triển trong những thập kỷ gần đây.",
                    Explanation = "Mẫu câu phân tích hiện tượng trung tâm (Micro-topic) của đề bài."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc nêu câu hỏi 2 yêu cầu (Two-part Question)",
                    Syntax = "What problems does + Noun + cause, and what solutions can + Actor + implement?",
                    Example = "What problems does urban overcrowding cause, and what solutions can municipal governments implement?",
                    Translation = "Nạn quá tải đô thị gây ra những vấn đề gì, và các chính quyền thành phố có thể thực thi những giải pháp nào?",
                    Explanation = "Yêu cầu người viết phải trả lời cả 2 khía cạnh: Nguyên nhân/Vấn đề ở Thân bài 1 và Giải pháp ở Thân bài 2."
                });
            }
            else if (title.Contains("Thesis Statement"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc Strong Thesis cho bài luận môi trường",
                    Syntax = "Although + Counter-view, I firmly contend that + Actor + should + Action + to + Purpose",
                    Example = "Although international air travel boosts global tourism, I firmly contend that governments should impose environmental taxes on aviation companies.",
                    Translation = "Mặc dù du lịch hàng không quốc tế thúc đẩy du lịch toàn cầu, tôi vững tin các chính phủ nên đánh thuế môi trường lên công ty hàng không.",
                    Explanation = "Cấu trúc Thesis sắc bén công nhận ý kiến đối lập trước khi khẳng định quan điểm cá nhân."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc Strong Thesis cho bài luận giải pháp",
                    Syntax = "Actor + should replace + Problematic Item + with + Sustainable Alternative + to + Benefit",
                    Example = "Supermarkets should replace single-use plastic packaging with biodegradable materials to reduce ocean pollution.",
                    Translation = "Các siêu thị nên thay thế bao bì nhựa dùng một lần bằng vật liệu sinh học để giảm ô nhiễm đại dương.",
                    Explanation = "Đưa ra giải pháp cụ thể thay vì viết câu Thesis chung chung mập mờ."
                });
            }
            else if (title.Contains("Introduction"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc Paraphrase đề bài bằng thể bị động khách quan",
                    Syntax = "It is increasingly asserted that + Clause (Topic aspect)",
                    Example = "It is increasingly asserted that automated intelligent systems might eventually supersede human educators.",
                    Translation = "Đang có quan điểm ngày càng phổ biến rằng các hệ thống trí tuệ tự động cuối cùng có thể thay thế giáo viên con người.",
                    Explanation = "Paraphrase từ gốc (replace -> supersede, human teachers -> human educators) bằng thể bị động khách quan."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Câu Thesis khẳng định sự không thể thay thế",
                    Syntax = "While + Admission, I firmly believe that + Target + remain indispensable due to + Reasons",
                    Example = "While AI can process data efficiently, I firmly believe that human teachers remain indispensable due to emotional empathy.",
                    Translation = "Mặc dù AI có thể xử lý dữ liệu hiệu quả, tôi vững tin giáo viên con người vẫn không thể thay thế nhờ sự thấu hiểu cảm xúc.",
                    Explanation = "Hoàn thiện đoạn Mở bài 2 câu hoàn chỉnh đạt chuẩn B2."
                });
            }
            else if (title.Contains("Body Paragraph") || title.Contains("Topic Sentence"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Câu Topic Sentence nêu tác dụng giảm thiểu đô thị",
                    Syntax = "First and foremost, + Action / Trend + significantly alleviates + Problem",
                    Example = "First and foremost, expanding public mass transit systems significantly alleviates urban traffic congestion.",
                    Translation = "Đầu tiên và quan trọng nhất, việc mở rộng hệ thống giao thông công cộng khối lượng lớn giúp giảm đáng kể ùn tắc giao thông.",
                    Explanation = "Câu Topic Sentence chuẩn mực chứa Topic và Controlling Idea, không chứa số liệu quá chi tiết."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Câu Topic Sentence nêu vai trò then chốt",
                    Syntax = "On the one hand, + Action + plays a pivotal role in + V-ing / Noun",
                    Example = "On the one hand, transitioning to renewable energy sources plays a pivotal role in mitigating environmental pollution.",
                    Translation = "Một mặt, việc chuyển đổi sang các nguồn năng lượng tái tạo đóng vai trò then chốt trong việc giảm thiểu ô nhiễm môi trường.",
                    Explanation = "Sử dụng cụm từ ăn điểm 'plays a pivotal role in mitigating' cho đoạn thân bài 1."
                });
            }
            else if (title.Contains("Supporting Ideas") || title.Contains("Explanation"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc giải thích nguyên nhân khoa học (Explanation)",
                    Syntax = "This is primarily because + Synthetic Item + persist in + Ecosystem + for + Time",
                    Example = "This is primarily because non-biodegradable synthetic polymers persist in aquatic ecosystems for centuries.",
                    Translation = "Điều này chủ yếu là do các polymer tổng hợp không phân hủy sinh học tồn tại trong hệ sinh thái dưới nước nhiều thế kỷ.",
                    Explanation = "Giải thích cơ chế hoạt động 'Why' và 'How' thay vì chỉ liệt kê từ khóa hời hợt."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc đưa dẫn chứng ví dụ có địa danh và số liệu (Example)",
                    Syntax = "A prominent example is + Case / Place, where + Clause with specific Impact",
                    Example = "A prominent example is the Pacific Trash Vortex, where accumulated plastic waste threatens over 700 marine species.",
                    Translation = "Một ví dụ nổi bật là Xoáy rác Thái Bình Dương, nơi rác thải nhựa tích tụ đe dọa hơn 700 loài sinh vật biển.",
                    Explanation = "Dẫn chứng cụ thể mang tính thuyết phục cao cho lập luận học thuật."
                });
            }
            else if (title.Contains("Linking Words") || title.Contains("Cohesion"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc nối câu bằng đại từ tham chiếu (Referencing Device)",
                    Syntax = "Clause A. This + Noun (This technological integration) + allows / enables + Object + to + V-bare",
                    Example = "Financial institutions are deploying fraud detection algorithms. This technological integration allows banks to analyze transactions in real time.",
                    Translation = "Các định chế tài chính đang triển khai các thuật toán phát hiện gian lận. Sự tích hợp công nghệ này cho phép ngân hàng phân tích giao dịch theo thời gian thực.",
                    Explanation = "Dùng cụm 'This technological integration' để gắn kết 2 câu tự nhiên mà không lặp từ."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc nối câu bằng dấu chấm phẩy và trạng từ hệ quả",
                    Syntax = "Clause A; consequently, + Clause B",
                    Example = "Banks analyze millions of transactions in real time; consequently, fraudulent activities are identified early.",
                    Translation = "Ngân hàng phân tích hàng triệu giao dịch theo thời gian thực; do đó, các hoạt động gian lận được phát hiện sớm.",
                    Explanation = "Sử dụng dấu chấm phẩy kết hợp '; consequently,' thể hiện trình độ ngữ pháp nâng cao."
                });
            }
            else if (title.Contains("Academic Vocabulary") || title.Contains("Grammar"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc Hedging nâng cao với động từ suy thoái",
                    Syntax = "It is observed that + Unsustainable Trend + tends to accelerate + Degradation of Target",
                    Example = "It is observed that unsustainable tourist inflows tend to accelerate the degradation of historic monuments.",
                    Translation = "Người ta quan sát thấy dòng du khách không bền vững có xu hướng làm tăng nhanh sự suy thoái của các di tích lịch sử.",
                    Explanation = "Dùng 'It is observed that' và 'tends to accelerate' để duy trì tính khách quan khoa học."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc sử dụng từ vựng AWL cản trở hiệu suất",
                    Syntax = "Evidence indicates that + Trend + is likely to + impede + Academic/Work Performance",
                    Example = "Evidence indicates that excessive smartphone usage is likely to impede academic performance among secondary students.",
                    Translation = "Bằng chứng cho thấy việc sử dụng điện thoại quá mức có khả năng cản trở kết quả học tập của học sinh.",
                    Explanation = "Thay từ suồng sã 'make students get bad results' bằng cụm từ AWL 'impede academic performance'."
                });
            }
            else if (title.Contains("Conclusion") || title.Contains("Proofreading"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc Kết bài khẳng định lại tác hại của tải công việc",
                    Syntax = "In conclusion, while + Temporary Advantage, I reiterate that + Main Disadvantage",
                    Example = "In conclusion, while prolonged working hours may offer temporary gains, I reiterate that excessive workload compromises employee health.",
                    Translation = "Tóm lại, mặc dù giờ làm việc kéo dài có thể mang lại mức tăng tạm thời, tôi khẳng định lại khối lượng công việc quá mức làm ảnh hưởng đến sức khỏe.",
                    Explanation = "Câu chốt bài 1 tóm tắt mượt mà toàn bộ lập luận thân bài."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Câu khuyến nghị tương lai cho môi trường làm việc",
                    Syntax = "Employers / Governments should therefore + Action + to + foster / ensure + Target",
                    Example = "Employers should therefore prioritize flexible scheduling to foster a sustainable workplace environment.",
                    Translation = "Do đó người sử dụng lao động nên ưu tiên lịch trình linh hoạt để nuôi dưỡng môi trường làm việc bền vững.",
                    Explanation = "Khép lại đoạn kết bài bằng khuyến nghị thiết thực và tích cực."
                });
            }
            else if (title.Contains("Essay hoàn chỉnh"))
            {
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "1. Cấu trúc mở bài bài luận di sản thủ công truyền thống",
                    Syntax = "It is increasingly argued that + Traditional Item + have lost their significance in + Modern Economy",
                    Example = "It is increasingly asserted that traditional artisanal crafts have lost their significance in contemporary industrialized economies.",
                    Translation = "Đang có quan điểm ngày càng phổ biến rằng các nghề thủ công truyền thống đã mất đi tầm quan trọng trong nền kinh tế công nghiệp hiện đại.",
                    Explanation = "Từ vựng di sản cực kỳ phong phú (artisanal crafts, contemporary industrialized economies)."
                });
                list.Add(new LessonGrammarExampleViewModel
                {
                    Title = "2. Cấu trúc khẳng định giá trị lưu giữ bản sắc văn hóa",
                    Syntax = "Subject + serve as + indispensable tangible repositories of + Cultural Identity",
                    Example = "First and foremost, traditional crafts serve as indispensable tangible repositories of cultural identity.",
                    Translation = "Đầu tiên và quan trọng nhất, các nghề thủ công truyền thống đóng vai trò là những kho lưu trữ hữu hình không thể thiếu của bản sắc văn hóa.",
                    Explanation = "Sử dụng cụm từ học thuật đắt giá 'indispensable tangible repositories of cultural identity'."
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
