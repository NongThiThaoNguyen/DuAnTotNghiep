# 📋 KẾ HOẠCH NÂNG CẤP — PHẦN TEACHER

> **Dự án**: AI Study English  
> **Mục tiêu**: Xây dựng backend & hoàn thiện chức năng giáo viên để demo  
> **Nguyên tắc**: Backend-first, UI chỉ cần đồng bộ — ưu tiên task nhỏ/vừa  
> **Ngày tạo**: 30/06/2026

---

## 📊 PHÂN TÍCH HIỆN TRẠNG TEACHER

### ✅ Đã có (14 controllers, 14 view folders):
- **Controllers**: HomeController, CoursesController, LessonsController, QuizzesController, AssignmentsController, AttendanceController, GradesController, MessagesController, ProfileController, ReportsController, ResourcesController, ScheduleController, SettingsController, StudentsController
- **Views**: 14 thư mục view tương ứng
- **Models liên quan**: Schedule, OriginalLesson, Quiz, QuizQuestion, PracticeTask, PracticeSubmission, Attendance, ChatMessage

### ❌ Vấn đề chính:
- **Không có Service layer** — tất cả controller dùng trực tiếp `ApplicationDbContext`
- HomeController.cs đã có logic cơ bản nhưng thiếu service
- Các controller khác có thể chưa có logic backend hoàn chỉnh
- Thiếu DI registration trong Program.cs cho Teacher services
- Chưa có ViewModels riêng cho Teacher area
- Thiếu filter/authorization phù hợp cho một số action

---

## 🚀 PHASE 1 — Nâng cấp Dashboard Teacher (4 tasks)
> **Mục tiêu**: Dashboard hiển thị đầy đủ thống kê cho giáo viên
> **Độ khó**: ⭐⭐ Nhẹ

### Task 1.1 — Tạo `TeacherDashboardService`
- [x] Tạo file `Services/Interfaces/ITeacherDashboardService.cs` với methods:
  - `Task<TeacherDashboardViewModel> GetDashboardAsync(int teacherId)`
- [x] Tạo file `Services/TeacherDashboardService.cs` implement:
  - Đếm tổng số courses (topics) đang active
  - Đếm tổng số students đang active
  - Đếm bài nộp chờ chấm (`PracticeSubmissions` status = "SUBMITTED")
  - Đếm lịch dạy hôm nay từ bảng `schedules`
  - Lấy 5 bài nộp mới nhất (include Student, PracticeTask)
  - Lấy lịch dạy hôm nay (include Topic)
  - Lấy 5 quiz attempts gần nhất (include Quiz, Student)
  - Thống kê phân bố khóa học theo Skill (cho biểu đồ)
- [x] Dùng `AsNoTracking()` cho tất cả read queries
- [x] Đăng ký DI trong [Program.cs](file:///e:/DuAnTotNghiep/Program.cs)

### Task 1.2 — Tạo `TeacherDashboardViewModel`
- [x] Tạo file `Models/ViewModels/Teacher/TeacherDashboardViewModel.cs` gồm:
  - `string TeacherName`, `string AvatarUrl`
  - `int CoursesCount`, `int StudentsCount`, `int PendingSubmissionsCount`, `int TodaySchedulesCount`
  - `List<RecentSubmissionViewModel> RecentSubmissions` (gồm StudentName, TaskTitle, SubmittedAt, Status)
  - `List<TodayScheduleViewModel> TodaySchedules` (gồm Title, TopicName, StartTime, EndTime, Classroom)
  - `List<RecentQuizAttemptViewModel> RecentQuizAttempts` (gồm StudentName, QuizTitle, Score, SubmittedAt)
  - `List<string> ChartLabels`, `List<int> ChartValues` — cho biểu đồ phân bố khóa học

### Task 1.3 — Refactor `HomeController` dùng Service
- [x] Mở [HomeController.cs](file:///e:/DuAnTotNghiep/Areas/Teacher/Controllers/HomeController.cs)
- [x] Thay thế `ApplicationDbContext` bằng `ITeacherDashboardService`
- [x] Thay toàn bộ logic truy vấn trực tiếp bằng `_dashboardService.GetDashboardAsync(teacherId)`
- [x] Truyền `TeacherDashboardViewModel` vào View thay vì ViewBag
- [x] Xóa toàn bộ ViewBag assignments

### Task 1.4 — Cập nhật View Dashboard
- [x] Mở `Areas/Teacher/Views/Home/Index.cshtml`
- [x] Dùng `@model TeacherDashboardViewModel` thay vì ViewBag
- [x] Hiển thị stat cards, bảng recent submissions, lịch hôm nay
- [x] Giữ layout Tailwind CSS nhất quán với Admin area

---

## 🚀 PHASE 2 — Hoàn thiện Quản lý Khóa học - Courses (5 tasks)
> **Mục tiêu**: Teacher CRUD khóa học (mapping từ LearningTopic) + quản lý bài học trong khóa
> **Độ khó**: ⭐⭐ Nhẹ-Vừa

### Task 2.1 — Tạo `TeacherCourseService`
- [x] Tạo `Services/Interfaces/ITeacherCourseService.cs`:
  - `Task<List<CourseListItemViewModel>> GetCoursesAsync(int teacherId, string? search, string? skillFilter)`
  - `Task<CourseDetailViewModel?> GetCourseDetailAsync(int courseId)`
  - `Task<int> CreateCourseAsync(CreateCourseViewModel model, int teacherId)` — tạo LearningTopic mới
  - `Task UpdateCourseAsync(EditCourseViewModel model)`
  - `Task DeleteCourseAsync(int courseId)`
  - `Task<int> GetCourseStudentCountAsync(int courseId)` — đếm student đang học topic này
  - `Task<List<LessonSummaryViewModel>> GetCourseLessonsAsync(int courseId)`
- [x] Tạo `Services/TeacherCourseService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 2.2 — Tạo ViewModels cho Courses
- [x] Tạo `Models/ViewModels/Teacher/CourseViewModels.cs`:
  - `CourseListItemViewModel`: Id, Title, Description, SkillName, LevelName, LessonCount, StudentCount, IsActive, CreatedAt
  - `CourseDetailViewModel`: full info + List<LessonSummaryViewModel> + List<QuizSummaryViewModel>
  - `CreateCourseViewModel`: Title, Description, SkillId, ProficiencyLevelId, DifficultyLevel, EstimatedMinutes (+ dropdown data)
  - `EditCourseViewModel`: tương tự Create + Id
  - `LessonSummaryViewModel`: Id, Title, OrderIndex, EstimatedMinutes, Status

### Task 2.3 — Refactor `CoursesController`
- [x] Mở [CoursesController.cs](file:///e:/DuAnTotNghiep/Areas/Teacher/Controllers/CoursesController.cs)
- [x] Inject `ITeacherCourseService`, `IEnglishSkillService`, `IEnglishProficiencyLevelService`
- [x] Thay DbContext trực tiếp bằng service calls
- [x] Action `Index(string? search, string? skill)` — danh sách với filter
- [x] Action `Details(int id)` — chi tiết khóa học + danh sách bài học
- [x] Action `Create()` GET/POST — tạo khóa học mới
- [x] Action `Edit(int id)` GET/POST — sửa khóa học
- [x] Action `Delete(int id)` POST — xóa khóa học (soft delete)

### Task 2.4 — Cập nhật Views Courses
- [x] Cập nhật `Areas/Teacher/Views/Courses/Index.cshtml` — dùng ViewModel, filter, search
- [x] Tạo/cập nhật `Create.cshtml` — form tạo khóa học với dropdown skill, level
- [x] Tạo/cập nhật `Edit.cshtml` — form sửa
- [x] Tạo/cập nhật `Details.cshtml` — chi tiết + danh sách bài học bên trong

### Task 2.5 — Seed dữ liệu mẫu cho Teacher
- [x] Tạo hoặc cập nhật seeder để đảm bảo có ít nhất:
  - 1 tài khoản role TEACHER
  - 3-5 LearningTopics có `CreatedById` = teacher
  - Mỗi topic có 2-3 OriginalLessons
  - 1-2 Quizzes cho mỗi topic
- [x] Chạy seeder khi startup (trong DatabaseSeeder hoặc AILearnSeeder)

---

## 🚀 PHASE 3 — Quản lý Bài học - Lessons (4 tasks)
> **Mục tiêu**: Teacher CRUD bài học trong khóa học
> **Độ khó**: ⭐⭐ Nhẹ

### Task 3.1 — Tạo `TeacherLessonService`
- [x] Tạo `Services/Interfaces/ITeacherLessonService.cs`:
  - `Task<List<LessonListItemViewModel>> GetLessonsByTopicAsync(int topicId)`
  - `Task<LessonDetailViewModel?> GetLessonDetailAsync(int lessonId)`
  - `Task<int> CreateLessonAsync(CreateLessonViewModel model, int teacherId)`
  - `Task UpdateLessonAsync(EditLessonViewModel model)`
  - `Task DeleteLessonAsync(int lessonId)`
  - `Task ReorderLessonsAsync(int topicId, List<int> orderedLessonIds)`
- [x] Tạo `Services/TeacherLessonService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 3.2 — Tạo ViewModels cho Lessons
- [x] Tạo `Models/ViewModels/Teacher/LessonViewModels.cs`:
  - `LessonListItemViewModel`: Id, Title, Summary, EstimatedMinutes, OrderIndex, Status, CreatedAt
  - `LessonDetailViewModel`: full info + Content (HTML)
  - `CreateLessonViewModel`: TopicId, Title, Summary, Content (textarea HTML), EstimatedMinutes, DifficultyLevel
  - `EditLessonViewModel`: tương tự + Id

### Task 3.3 — Refactor `LessonsController`
- [x] Mở [LessonsController.cs](file:///e:/DuAnTotNghiep/Areas/Teacher/Controllers/LessonsController.cs)
- [x] Inject `ITeacherLessonService`
- [x] Action `Index(int courseId)` — danh sách bài học theo khóa
- [x] Action `Create(int courseId)` GET/POST — tạo bài học mới
- [x] Action `Edit(int id)` GET/POST — sửa bài học
- [x] Action `Delete(int id)` POST — xóa bài học
- [x] Action `Detail(int id)` — xem chi tiết nội dung bài học

### Task 3.4 — Cập nhật Views Lessons
- [x] Cập nhật `Areas/Teacher/Views/Lessons/Index.cshtml` — bảng danh sách bài học
- [x] Tạo/cập nhật `Create.cshtml` — form tạo bài học (textarea cho content)
- [x] Tạo/cập nhật `Edit.cshtml` — form sửa
- [x] Tạo/cập nhật `Detail.cshtml` — xem nội dung bài học full

---

## 🚀 PHASE 4 — Quản lý Quiz & Chấm bài (5 tasks)
> **Mục tiêu**: Teacher tạo quiz, xem kết quả, chấm bài thực hành
> **Độ khó**: ⭐⭐⭐ Vừa

### Task 4.1 — Tạo `TeacherQuizService`
- [x] Tạo `Services/Interfaces/ITeacherQuizService.cs`:
  - `Task<List<QuizListItemViewModel>> GetQuizzesByTopicAsync(int topicId)`
  - `Task<QuizManageViewModel?> GetQuizDetailAsync(int quizId)`
  - `Task<int> CreateQuizAsync(CreateQuizViewModel model, int teacherId)` — tạo Quiz + QuizQuestions
  - `Task UpdateQuizAsync(EditQuizViewModel model)`
  - `Task DeleteQuizAsync(int quizId)`
  - `Task<List<QuizAttemptResultViewModel>> GetQuizAttemptsAsync(int quizId)` — danh sách kết quả sinh viên
- [x] Tạo `Services/TeacherQuizService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 4.2 — Tạo `TeacherGradingService`
- [x] Tạo `Services/Interfaces/ITeacherGradingService.cs`:
  - `Task<List<PendingSubmissionViewModel>> GetPendingSubmissionsAsync(int teacherId)`
  - `Task<PracticeSubmissionDetailViewModel?> GetSubmissionDetailAsync(int submissionId)`
  - `Task GradeSubmissionAsync(int submissionId, decimal score, string? feedback, int teacherId)`
  - `Task<List<GradeOverviewViewModel>> GetGradesOverviewAsync(int? topicId)` — tổng hợp điểm
- [x] Tạo `Services/TeacherGradingService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 4.3 — Tạo ViewModels
- [x] Tạo `Models/ViewModels/Teacher/QuizViewModels.cs`:
  - `QuizListItemViewModel`: Id, Title, TopicName, QuestionCount, AttemptCount, AverageScore
  - `CreateQuizViewModel`: TopicId, Title, Description, TimeLimitMinutes, PassScore, Questions list
  - `QuizQuestionFormViewModel`: QuestionText, QuestionType, Options list, CorrectAnswer, Points
  - `QuizAttemptResultViewModel`: StudentName, Score, SubmittedAt, TimeTaken
- [x] Tạo `Models/ViewModels/Teacher/GradingViewModels.cs`:
  - `PendingSubmissionViewModel`: Id, StudentName, TaskTitle, SubmittedAt
  - `PracticeSubmissionDetailViewModel`: full info + StudentAnswer + TaskDescription
  - `GradeOverviewViewModel`: StudentName, TopicName, QuizScore, PracticeScore, TotalScore

### Task 4.4 — Refactor 
- [x] **AssignmentsController**: (Bài tập) - _Cần ITeacherAssignmentService_
- [x] **ProfileController**: (Hồ sơ giảng viên) - _Cần ITeacherProfileService_
- [x] **ReportsController**: (Báo cáo/Thống kê) - _Cần ITeacherReportService_
- [x] **StudentsController**: (Danh sách sinh viên) - _Cần ITeacherStudentService_
- [x] **SettingsController**: (Cài đặt hệ thống) - _Cần ITeacherSettingsService_

### Task 4.5 — Cập nhật Views Quiz & Grades
- [x] Cập nhật `Areas/Teacher/Views/Quizzes/` — danh sách, tạo, xem kết quả
- [x] Cập nhật `Areas/Teacher/Views/Grades/` — danh sách chờ chấm, form chấm điểm, tổng hợp
- [x] Giao diện form tạo quiz cho phép thêm nhiều câu hỏi (dynamic form fields)

---

## 🚀 PHASE 5 — Lịch dạy & Điểm danh (4 tasks)
> **Mục tiêu**: Teacher quản lý lịch dạy, điểm danh sinh viên
> **Độ khó**: ⭐⭐ Nhẹ

### Task 5.1 — Tạo `TeacherScheduleService`
- [x] Tạo `Services/Interfaces/ITeacherScheduleService.cs`:
  - `Task<List<ScheduleViewModel>> GetSchedulesAsync(int teacherId, DateTime? from, DateTime? to)`
  - `Task<ScheduleViewModel?> GetByIdAsync(int id)`
  - `Task<int> CreateAsync(CreateScheduleViewModel model)`
  - `Task UpdateAsync(EditScheduleViewModel model)`
  - `Task DeleteAsync(int id)`
  - `Task<List<ScheduleViewModel>> GetTodaySchedulesAsync(int teacherId)`
  - `Task<List<ScheduleViewModel>> GetWeekSchedulesAsync(int teacherId, DateTime weekStart)`
- [x] Tạo `Services/TeacherScheduleService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 5.2 — Tạo `AttendanceService`
- [x] Tạo `Services/Interfaces/IAttendanceService.cs`:
  - `Task<List<AttendanceRecordViewModel>> GetAttendanceByScheduleAsync(int scheduleId)`
  - `Task MarkAttendanceAsync(int scheduleId, int studentId, string status)` — status: PRESENT/ABSENT/LATE
  - `Task BulkMarkAttendanceAsync(int scheduleId, List<AttendanceMarkViewModel> records)`
  - `Task<AttendanceSummaryViewModel> GetStudentAttendanceSummaryAsync(int studentId)`
- [x] Tạo `Services/AttendanceService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 5.3 — Refactor Controllers
- [x] Refactor ScheduleController.cs — inject ITeacherScheduleService
- [x] CRUD actions + calendar view data
- [x] Refactor AttendanceController.cs — inject IAttendanceService
- [x] Action điểm danh theo buổi, xem tổng hợp

### Task 5.4 — Cập nhật Views Schedule & Attendance
- [x] Cập nhật `Areas/Teacher/Views/Schedule/` — danh sách, tạo/sửa lịch, view theo tuần
- [x] Cập nhật `Areas/Teacher/Views/Attendance/` — form điểm danh bulk, tổng hợp điểm danh

---

## 🚀 PHASE 6 — Tài nguyên & Tin nhắn (4 tasks)
> **Mục tiêu**: Teacher quản lý tài liệu, nhắn tin với student
> **Độ khó**: ⭐⭐ Nhẹ

### Task 6.1 — Tạo `TeacherResourceService`
- [x] Tạo `Services/Interfaces/ITeacherResourceService.cs`:
  - `Task<List<ResourceViewModel>> GetResourcesAsync(int teacherId, string? typeFilter)`
  - `Task<int> CreateResourceAsync(CreateResourceViewModel model, int teacherId)` — mapping đến ReferenceSource
  - `Task UpdateResourceAsync(EditResourceViewModel model)`
  - `Task DeleteResourceAsync(int id)`
  - `Task<List<ResourceViewModel>> GetResourcesByTopicAsync(int topicId)`
- [x] Tạo `Services/TeacherResourceService.cs` implement
- [x] Đăng ký DI

### Task 6.2 — Tạo `TeacherMessageService`
- [x] Tạo `Services/Interfaces/ITeacherMessageService.cs`:
  - `Task<List<ConversationViewModel>> GetConversationsAsync(int teacherId)`
  - `Task<List<MessageViewModel>> GetMessagesAsync(int conversationId)`
  - `Task SendMessageAsync(int senderId, int receiverId, string content)`
  - `Task<int> GetUnreadCountAsync(int teacherId)`
- [x] Tạo `Services/TeacherMessageService.cs` implement (dùng bảng `ChatMessage` hoặc `Notification`)
- [x] Đăng ký DI

### Task 6.3 — Refactor Controllers
- [x] Refactor ResourcesController.cs — inject service
- [x] Refactor MessagesController.cs — inject service
- [x] Tạo ViewModels riêng cho Resources và Messages

### Task 6.4 — Cập nhật Views
- [x] Cập nhật `Areas/Teacher/Views/Resources/` — CRUD resources
- [x] Cập nhật `Areas/Teacher/Views/Messages/` — inbox, chat view
- [x] Đảm bảo consistent UI với các phần khác

---

## 📋 TỔNG KẾT

| Phase | Số tasks | Độ khó | Mô tả |
|-------|----------|--------|-------|
| 1 | 4 | ⭐⭐ | Nâng cấp Dashboard Teacher |
| 2 | 5 | ⭐⭐ | Quản lý Khóa học |
| 3 | 4 | ⭐⭐ | Quản lý Bài học |
| 4 | 5 | ⭐⭐⭐ | Quiz & Chấm bài |
| 5 | 4 | ⭐⭐ | Lịch dạy & Điểm danh |
| 6 | 4 | ⭐⭐ | Tài nguyên & Tin nhắn |
| **Tổng** | **26 tasks** | | **Backend-first, demo-ready** |

> **Ghi chú cho AI Agent**: 
> - **Ưu tiên Phase 1-3 trước** vì đây là core features cho Teacher demo.
> - Mỗi Phase tạo service → viewmodel → refactor controller → update view.
> - Sau mỗi Phase: chạy `dotnet build` kiểm tra, sửa lỗi trước khi tiếp.
> - Tích `[x]` khi hoàn thành task. 
> - Nếu controller đã có code cũ dùng DbContext trực tiếp, refactor sang service pattern.
