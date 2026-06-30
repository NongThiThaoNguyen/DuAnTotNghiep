# 📋 KẾ HOẠCH NÂNG CẤP — PHẦN STUDENT

> **Dự án**: AI Study English  
> **Mục tiêu**: Nâng cấp backend & hoàn thiện trải nghiệm học viên để demo  
> **Nguyên tắc**: Backend-first, UI chỉ cần đồng bộ — ưu tiên task nhỏ/vừa  
> **Ngày tạo**: 30/06/2026

---

## 📊 PHÂN TÍCH HIỆN TRẠNG STUDENT

### ✅ Đã có — Student Area (6 controllers):
- **HomeController** — Dashboard với tiến trình, timeline, thông báo
- **OnboardingController** — Quy trình onboarding 6 bước (hoàn chỉnh)
- **PlacementTestController** — Làm bài kiểm tra xếp lớp (hoàn chỉnh)
- **LearningPathController** — Xem, tạo, theo dõi lộ trình (hoàn chỉnh)
- **ProgressController** — Dashboard tiến trình chi tiết (hoàn chỉnh)
- **ProfileController** — Sửa hồ sơ học tập

### ✅ Đã có — Controllers chung (dùng cho student):
- **DashboardController** — Dashboard tổng quan với XP, rank
- **CoursesController** — Danh sách khóa học
- **LessonController** — Xem bài học, đánh dấu hoàn thành
- **QuizController** — Làm quiz, submit, xem kết quả
- **AITutorController** — Chat với AI (đang dùng simulated replies)
- **AchievementsController** — Xem huy hiệu
- **StatisticsController** — Thống kê học tập
- **CompetencyAnalysisController** — Xem phân tích năng lực

### ❌ Vấn đề / Còn thiếu:
- **M10 Study Plan** đang phát triển (có plan nhưng chưa implement)
- AITutorController dùng **simulated replies** (hardcoded), chưa kết nối OpenAI thật
- DashboardController, CoursesController, LessonController, QuizController, StatisticsController dùng **DbContext trực tiếp** (thiếu service layer)
- AchievementsController có nhưng **thiếu logic auto-grant** (tự trao huy hiệu khi đạt điều kiện)
- Streak tracking chỉ hardcode `StreakDays = 4`, chưa tính thật
- Thiếu hệ thống **Notification cho student** (đọc, đánh dấu đã đọc)
- Thiếu **Student Settings** (cài đặt ngôn ngữ, thông báo, giao diện)
- Thiếu trang **Bookmark/Favorites** (lưu bài học yêu thích)

---

## 🚀 PHASE 1 — Hoàn thiện Study Plan - M10 (5 tasks)
> **Mục tiêu**: Implement kế hoạch học theo ngày/tuần/tháng từ lộ trình
> **Độ khó**: ⭐⭐⭐ Vừa
> **Tham khảo**: [M10_plan.md](file:///e:/DuAnTotNghiep/M10_plan.md)

### Task 1.1 — Tạo ViewModels cho Study Plan
- [ ] Tạo thư mục `Models/ViewModels/StudyPlan/`
- [ ] Tạo `DailyStudyTaskViewModel.cs`:
  - `int NodeId`, `string Title`, `string? SkillName`, `string NodeType`
  - `string StatusLabel` (TODAY/OVERDUE/UPCOMING/COMPLETED)
  - `string StatusCss` — CSS class tương ứng
  - `int EstimatedMinutes`, `string? TargetUrl`, `DateOnly? ScheduledDate`
  - `bool IsOverdue`, `bool IsToday`, `bool IsCompleted`
- [ ] Tạo `WeeklyPlanViewModel.cs`:
  - `DateOnly WeekStart`, `DateOnly WeekEnd`
  - `Dictionary<DateOnly, List<DailyStudyTaskViewModel>> DayTasks`
  - `int TotalTasksThisWeek`, `int CompletedThisWeek`, `double CompletionPercent`
- [ ] Tạo `MonthlyPlanViewModel.cs`:
  - `int Year`, `int Month`
  - `Dictionary<DateOnly, List<DailyStudyTaskViewModel>> DayTasks`
  - `int TotalTasks`, `int CompletedTasks`

### Task 1.2 — Tạo `StudyPlanService`
- [ ] Tạo `Services/Interfaces/IStudyPlanService.cs`:
  - `Task<List<DailyStudyTaskViewModel>> GetTodayTasksAsync(int userId)`
  - `Task<WeeklyPlanViewModel> GetWeeklyPlanAsync(int userId, DateOnly? weekStart)`
  - `Task<MonthlyPlanViewModel> GetMonthlyPlanAsync(int userId, int? year, int? month)`
  - `Task RescheduleTaskAsync(int nodeId, int userId, DateOnly newDate, string? reason)`
  - `Task SkipTaskAsync(int nodeId, int userId, string reason)`
  - `Task<int> GetOverdueCountAsync(int userId)` — đếm task quá hạn
- [ ] Tạo `Services/StudyPlanService.cs` implement:
  - Lấy active LearningPath của student
  - Query `LearningPathNodes` theo `ScheduledDate`
  - Tính `IsOverdue` = ScheduledDate < today && status != COMPLETED
  - Tính `IsToday` = ScheduledDate == today
  - Dùng `IPathViewService.BuildNodeTargetUrlAsync()` để tạo URL
  - Dùng `AsNoTracking()` cho read queries
- [ ] Đăng ký DI trong Program.cs

### Task 1.3 — Tạo `StudyPlanController` trong Student Area
- [ ] Tạo `Areas/Student/Controllers/StudyPlanController.cs`
- [ ] `[Area("Student")]`, `[Authorize(Roles = "STUDENT")]`
- [ ] Action `Index()` — hiển thị kế hoạch hôm nay (mặc định)
- [ ] Action `Weekly(string? weekStart)` — kế hoạch tuần
- [ ] Action `Monthly(int? year, int? month)` — kế hoạch tháng
- [ ] Action `Reschedule(int nodeId, DateOnly newDate, string? reason)` POST
- [ ] Action `Skip(int nodeId, string reason)` POST
- [ ] Inject `IStudyPlanService`

### Task 1.4 — Tạo Views cho Study Plan
- [ ] Tạo thư mục `Areas/Student/Views/StudyPlan/`
- [ ] Tạo `Index.cshtml` — danh sách task hôm nay + overdue, progress bar
- [ ] Tạo `Weekly.cshtml` — grid 7 ngày, mỗi ngày liệt kê tasks
- [ ] Tạo `Monthly.cshtml` — calendar view, mỗi ngày hiển thị số task
- [ ] Tab navigation: Hôm nay | Tuần | Tháng
- [ ] Dùng Tailwind CSS, badge màu cho status (xanh=done, vàng=today, đỏ=overdue)

### Task 1.5 — Thêm Study Plan vào sidebar/navigation Student
- [ ] Cập nhật layout Student để thêm menu "Kế hoạch học" link đến `/Student/StudyPlan`
- [ ] Hiển thị badge số task overdue trên menu icon
- [ ] Cập nhật Student Dashboard thêm widget "Kế hoạch hôm nay" (3 task tiếp theo)

---

## 🚀 PHASE 2 — Nâng cấp Student Dashboard (4 tasks)
> **Mục tiêu**: Dashboard thống kê thực, streak tracking, timeline cải thiện
> **Độ khó**: ⭐⭐ Nhẹ

### Task 2.1 — Tạo `StudentDashboardService`
- [ ] Tạo `Services/Interfaces/IStudentDashboardService.cs`:
  - `Task<StudentDashboardViewModel> GetDashboardAsync(int userId)`
- [ ] Tạo `Services/StudentDashboardService.cs` implement:
  - **Streak calculation thật**: đếm số ngày liên tục có `StudyActivityLog` (query ngược từ hôm nay)
  - **XP calculation thật**: studyMinutes * 10 + completedLessons * 50 + completedQuizzes * 100
  - **Rank tier**: Bronze (<1000 XP), Silver (1000-2999), Gold (≥3000)
  - Tổng hợp: Tổng lessons hoàn thành, Tổng quizzes, Điểm quiz trung bình
  - Recent activities (5 gần nhất) từ StudyActivityLog
  - Tổng thời gian học (phút) tuần này
  - Next node trong learning path
- [ ] Đăng ký DI

### Task 2.2 — Tạo `StudentDashboardViewModel`
- [ ] Tạo `Models/ViewModels/Student/StudentDashboardViewModel.cs`:
  - `string StudentName`, `string AvatarUrl`
  - `int StreakDays` (tính thật), `int TotalXp`, `int Level`, `string RankTier`
  - `int CompletedLessons`, `int CompletedQuizzes`, `decimal AverageQuizScore`
  - `double ProgressPercent` (% lộ trình hoàn thành)
  - `int StudyMinutesThisWeek`
  - `string? CurrentLevel`, `string? TargetLevel`
  - `List<ActivityItemViewModel> RecentActivities`
  - `NextTaskViewModel? NextTask` (node tiếp theo cần làm)
  - `string? AiRecommendation` (gợi ý từ AI, lấy từ CompetencyAnalysis)

### Task 2.3 — Refactor `DashboardController` dùng Service
- [ ] Mở [DashboardController.cs](file:///e:/DuAnTotNghiep/Controllers/DashboardController.cs)
- [ ] Inject `IStudentDashboardService` thay thế DbContext trực tiếp
- [ ] Thay toàn bộ logic inline bằng service call
- [ ] Bỏ hết hardcoded data (StreakDays = 4, default progress, mock recommendation)
- [ ] Truyền `StudentDashboardViewModel` vào View

### Task 2.4 — Cập nhật Dashboard View
- [ ] Cập nhật `Views/Dashboard/Index.cshtml` dùng `@model StudentDashboardViewModel`
- [ ] Thay tất cả ViewBag bằng Model properties
- [ ] Thêm widget "Kế hoạch hôm nay" (nếu StudyPlan đã có)
- [ ] Hiển thị streak thật, XP thật, rank tier thật

---

## 🚀 PHASE 3 — Kết nối AI Tutor với OpenAI (3 tasks)
> **Mục tiêu**: Chat với AI thật thay vì simulated replies
> **Độ khó**: ⭐⭐⭐ Vừa

### Task 3.1 — Tạo `AiTutorService`
- [ ] Tạo `Services/Interfaces/IAiTutorService.cs`:
  - `Task<AiTutorConversation> GetOrCreateConversationAsync(int userId)`
  - `Task<List<ChatMessageViewModel>> GetMessagesAsync(int conversationId)`
  - `Task<string> SendMessageAndGetReplyAsync(int conversationId, int userId, string message)`
  - `Task<int> GetConversationCountAsync(int userId)`
- [ ] Tạo `Services/AiTutorService.cs` implement:
  - Inject `IAIProvider` (OpenAI) để gọi AI thật
  - Build prompt context: include student's profile, recent chat history (10 tin gần nhất)
  - System prompt: "You are an English tutor. The student's level is {level}. Respond in Vietnamese with English examples."
  - Lưu cả student message và AI reply vào `AiTutorMessages`
  - Ghi `StudyActivityLog` (ActivityType = "CHAT")
  - Ghi `AiUsageLog` (tokens, cost)
  - **Fallback**: nếu AI call thất bại, trả simulated reply (giữ logic cũ làm fallback)
- [ ] Đăng ký DI

### Task 3.2 — Refactor `AITutorController`
- [ ] Mở [AITutorController.cs](file:///e:/DuAnTotNghiep/Controllers/AITutorController.cs)
- [ ] Inject `IAiTutorService` thay thế DbContext trực tiếp
- [ ] Thay `GetSimulatedTutorReply()` bằng `_service.SendMessageAndGetReplyAsync()`
- [ ] Giữ method `GetSimulatedTutorReply()` làm fallback (dùng khi AI key chưa config)
- [ ] Thêm error handling: try-catch, trả JSON error nếu AI fail

### Task 3.3 — Cập nhật AI Tutor View
- [ ] Cập nhật `Views/AITutor/Index.cshtml`:
  - Thêm loading indicator khi chờ AI response
  - Thêm thông báo nếu AI không available ("Đang dùng chế độ offline")
  - Hiển thị model name đã dùng (nếu có)
  - Scroll tự động xuống tin nhắn mới nhất

---

## 🚀 PHASE 4 — Nâng cấp Courses/Lesson/Quiz cho Student (5 tasks)
> **Mục tiêu**: Cải thiện trải nghiệm học, thêm service layer
> **Độ khó**: ⭐⭐ Nhẹ-Vừa

### Task 4.1 — Tạo `StudentCourseService`
- [ ] Tạo `Services/Interfaces/IStudentCourseService.cs`:
  - `Task<List<StudentCourseViewModel>> GetCoursesAsync(int userId, string? category, string? search)`
  - `Task<StudentCourseDetailViewModel?> GetCourseDetailAsync(int courseId, int userId)`
  - `Task<double> GetCourseProgressAsync(int courseId, int userId)` — % hoàn thành
  - `Task EnrollCourseAsync(int courseId, int userId)` — ghi nhận bắt đầu học
- [ ] Tạo `Services/StudentCourseService.cs` implement
- [ ] Đăng ký DI

### Task 4.2 — Tạo `StudentLessonService`
- [ ] Tạo `Services/Interfaces/IStudentLessonService.cs`:
  - `Task<StudentLessonViewModel?> GetLessonAsync(int lessonId, int userId)`
  - `Task MarkLessonCompletedAsync(int lessonId, int userId)`
  - `Task<int?> GetNextLessonIdAsync(int currentLessonId)` — lesson tiếp theo
  - `Task<int?> GetPreviousLessonIdAsync(int currentLessonId)` — lesson trước
  - `Task<List<LessonNavigationItemViewModel>> GetCourseLessonsNavAsync(int topicId, int userId)`
- [ ] Tạo `Services/StudentLessonService.cs` implement
- [ ] Đăng ký DI

### Task 4.3 — Tạo `StudentQuizService`
- [ ] Tạo `Services/Interfaces/IStudentQuizService.cs`:
  - `Task<StudentQuizViewModel?> GetQuizForTakingAsync(int topicId)`
  - `Task<QuizResultViewModel> SubmitQuizAsync(int quizId, int userId, Dictionary<int, string> answers)`
  - `Task<List<QuizHistoryViewModel>> GetQuizHistoryAsync(int userId)` — lịch sử quiz
- [ ] Tạo `Services/StudentQuizService.cs` implement
- [ ] Đăng ký DI

### Task 4.4 — Refactor Controllers
- [ ] Refactor [CoursesController.cs](file:///e:/DuAnTotNghiep/Controllers/CoursesController.cs) — inject IStudentCourseService
- [ ] Refactor [LessonController.cs](file:///e:/DuAnTotNghiep/Controllers/LessonController.cs) — inject IStudentLessonService
- [ ] Refactor [QuizController.cs](file:///e:/DuAnTotNghiep/Controllers/QuizController.cs) — inject IStudentQuizService
- [ ] Refactor [StatisticsController.cs](file:///e:/DuAnTotNghiep/Controllers/StatisticsController.cs) — bỏ hardcoded data, dùng service/db thật
- [ ] Bỏ tất cả mock/placeholder data, dùng data thật từ DB

### Task 4.5 — Cập nhật Views
- [ ] Cập nhật `Views/Courses/Index.cshtml` — dùng ViewModel, bỏ mock progress
- [ ] Cập nhật `Views/Lesson/Detail.cshtml` — dùng ViewModel
- [ ] Cập nhật `Views/Quiz/Take.cshtml` và `Result.cshtml` — dùng ViewModel
- [ ] Cập nhật `Views/Statistics/Index.cshtml` — dùng data thật, bỏ mock weekly minutes

---

## 🚀 PHASE 5 — Gamification Backend (4 tasks)
> **Mục tiêu**: Tự động trao huy hiệu, XP tracking, streak thật
> **Độ khó**: ⭐⭐ Nhẹ-Vừa

### Task 5.1 — Tạo `GamificationService`
- [ ] Tạo `Services/Interfaces/IGamificationService.cs`:
  - `Task<int> CalculateXpAsync(int userId)` — tổng XP
  - `Task<int> CalculateStreakAsync(int userId)` — streak thật (ngày liên tục có activity)
  - `Task<string> GetRankTierAsync(int userId)` — Bronze/Silver/Gold/Platinum
  - `Task<int> GetLevelAsync(int userId)` — level = totalXP / 1000 + 1
  - `Task CheckAndGrantAchievementsAsync(int userId)` — kiểm tra và tự trao huy hiệu
  - `Task<List<AchievementBadgeViewModel>> GetUserBadgesAsync(int userId)`
- [ ] Tạo `Services/GamificationService.cs` implement:
  - **Streak**: Query StudyActivityLog grouped by date, đếm ngược từ hôm nay
  - **Auto-grant achievements** dựa trên conditions:
    - "FIRST_LOGIN" — lần đầu đăng nhập
    - "FIRST_QUIZ" — làm quiz đầu tiên
    - "STREAK_7" — streak 7 ngày liên tục
    - "STREAK_30" — streak 30 ngày
    - "QUIZ_MASTER" — hoàn thành 10 quizzes
    - "LESSON_COMPLETE_10" — hoàn thành 10 bài học
    - "PLACEMENT_DONE" — hoàn thành placement test
    - "PATH_CREATED" — tạo learning path đầu tiên
- [ ] Đăng ký DI

### Task 5.2 — Tạo Seeder cho Achievements
- [ ] Tạo/cập nhật seeder seed danh sách Achievement mặc định:
  - Code, Title, Description, IconUrl (emoji/icon class), XpReward, IsActive
  - 8-10 achievements với các conditions trên
- [ ] Chạy trong `DatabaseSeeder.SeedAsync()`

### Task 5.3 — Tích hợp Gamification vào flow Student
- [ ] Sau khi student hoàn thành quiz → gọi `CheckAndGrantAchievementsAsync()`
- [ ] Sau khi student hoàn thành lesson → gọi `CheckAndGrantAchievementsAsync()`
- [ ] Sau khi student tạo learning path → gọi `CheckAndGrantAchievementsAsync()`
- [ ] Sau khi student hoàn thành placement test → gọi `CheckAndGrantAchievementsAsync()`
- [ ] Hook vào các service/controller tương ứng

### Task 5.4 — Refactor `AchievementsController`
- [ ] Mở [AchievementsController.cs](file:///e:/DuAnTotNghiep/Controllers/AchievementsController.cs)
- [ ] Inject `IGamificationService`
- [ ] Hiển thị cả achievements đã đạt và chưa đạt (với progress bar)
- [ ] Cập nhật View hiển thị all achievements (unlocked và locked)

---

## 🚀 PHASE 6 — Notification & Settings cho Student (4 tasks)
> **Mục tiêu**: Student nhận thông báo, quản lý cài đặt cá nhân
> **Độ khó**: ⭐⭐ Nhẹ

### Task 6.1 — Tạo `StudentNotificationService`
- [ ] Tạo `Services/Interfaces/IStudentNotificationService.cs`:
  - `Task<List<NotificationViewModel>> GetNotificationsAsync(int userId, int page, int pageSize)`
  - `Task<int> GetUnreadCountAsync(int userId)`
  - `Task MarkAsReadAsync(int notificationId, int userId)`
  - `Task MarkAllAsReadAsync(int userId)`
- [ ] Tạo `Services/StudentNotificationService.cs` implement
  - Lấy notifications WHERE TargetUserId == userId OR TargetUserId IS NULL
  - Join với NotificationRead để biết đã đọc chưa
- [ ] Đăng ký DI

### Task 6.2 — Tạo Student Notification UI
- [ ] Tạo `Areas/Student/Controllers/NotificationController.cs`
  - Action `Index(int page = 1)` — danh sách thông báo
  - Action `MarkRead(int id)` POST — đánh dấu đã đọc
  - Action `MarkAllRead()` POST — đánh dấu tất cả
- [ ] Tạo `Areas/Student/Views/Notification/Index.cshtml`
  - Danh sách thông báo, badge unread, phân trang
- [ ] Cập nhật Student layout: thêm bell icon + unread count trên navbar

### Task 6.3 — Tạo `StudentSettingService`
- [ ] Tạo `Services/Interfaces/IStudentSettingService.cs`:
  - `Task<UserSetting?> GetSettingsAsync(int userId)`
  - `Task UpdateSettingsAsync(int userId, UpdateSettingViewModel model)`
  - `Task<UserSetting> GetOrCreateDefaultAsync(int userId)` — tạo default nếu chưa có
- [ ] Tạo `Services/StudentSettingService.cs` implement
- [ ] Đăng ký DI

### Task 6.4 — Tạo Student Settings UI
- [ ] Tạo hoặc cập nhật controller Settings trong Student area
  - Action `Index()` — hiển thị settings
  - Action `Update(UpdateSettingViewModel model)` POST — cập nhật
- [ ] Tạo View Settings: form cài đặt thông báo (on/off), ngôn ngữ, theme preference
- [ ] Tạo `UpdateSettingViewModel`: EmailNotification (bool), DailyReminder (bool), Language (string), ThemePreference (string)

---

## 📋 TỔNG KẾT

| Phase | Số tasks | Độ khó | Mô tả |
|-------|----------|--------|-------|
| 1 | 5 | ⭐⭐⭐ | Study Plan (M10) |
| 2 | 4 | ⭐⭐ | Nâng cấp Dashboard |
| 3 | 3 | ⭐⭐⭐ | AI Tutor thật |
| 4 | 5 | ⭐⭐ | Courses/Lesson/Quiz |
| 5 | 4 | ⭐⭐ | Gamification |
| 6 | 4 | ⭐⭐ | Notification & Settings |
| **Tổng** | **25 tasks** | | **Backend-first, demo-ready** |

> **Ghi chú cho AI Agent**:
> - **Ưu tiên Phase 1 (Study Plan M10)** vì đây là module đang phát triển dở, cần hoàn thiện trước.
> - **Phase 2, 4** tập trung refactor từ DbContext trực tiếp sang Service pattern.
> - **Phase 3 (AI Tutor)** cần API key trong `.env` để test — nếu chưa có key, để fallback simulated.
> - **Phase 5 (Gamification)** cần seed achievements trước khi test auto-grant.
> - Sau mỗi Phase: `dotnet build` kiểm tra, sửa lỗi trước khi tiếp.
> - Tích `[x]` khi hoàn thành task.
> - Nếu gặp lỗi liên quan DB (missing column, missing table), kiểm tra migration hoặc tạo SQL script.
