# 📋 Kế hoạch bổ sung chức năng Admin – Quản lý Student & Teacher

> Phân tích hiện trạng và các chức năng còn thiếu cho Admin để quản lý Student và Teacher.

---

## 🔍 Hiện trạng hệ thống

### 👨‍🎓 Student Area – Đã có
| Controller | Chức năng |
|---|---|
| `HomeController` | Dashboard học tập, timeline, thông báo |
| `LearningPathController` | Xem & điều hướng lộ trình học |
| `OnboardingController` | Khảo sát ban đầu, tạo hồ sơ học tập |
| `PlacementTestController` | Làm bài kiểm tra xếp lớp |
| `ProfileController` | Xem & chỉnh sửa profile cá nhân |
| `ProgressController` | Xem tiến độ, lịch sử, chi tiết kỹ năng/chủ đề |
| `StudyPlanController` | Xem kế hoạch học tập |

### 👨‍🏫 Teacher Area – Đã có
| Controller | Chức năng |
|---|---|
| `HomeController` | Dashboard giáo viên |
| `AssignmentsController` | Quản lý bài tập, chấm bài, xem submission |
| `AttendanceController` | Điểm danh học viên theo chủ đề, lịch sử điểm danh |
| `CoursesController` | Quản lý khóa học/topic |
| `GradesController` | Xem điểm, chấm bài pending |
| `LessonsController` | CRUD bài học |
| `MessagesController` | Chat với học viên |
| `ProfileController` | Quản lý hồ sơ giáo viên |
| `QuizzesController` | CRUD quiz, xem kết quả |
| `ReportsController` | Báo cáo tổng quan (thống kê attendance, điểm, lộ trình) |
| `ResourcesController` | Quản lý tài liệu học tập |
| `ScheduleController` | Xem lịch giảng dạy |
| `SettingsController` | Cài đặt giáo viên |
| `StudentsController` | Danh sách + chi tiết học viên (progress, activity logs) |

### 🔑 Admin Area – Đã có
| Controller | Chức năng |
|---|---|
| `UserController` | DS user, xem chi tiết, khóa/mở khóa, đổi role, reset password, thống kê, learning profile, reset onboarding |
| `TeacherSchedulesController` | Xem/tạo/sửa/xóa lịch giảng dạy của giáo viên + AI suggestions |
| `NotificationsController` | Gửi thông báo (tất cả, học viên, giáo viên, cụ thể) |
| `AuditLogsController` | Xem audit log hệ thống |
| `CompetencyHistoryController` | Xem lịch sử phân tích năng lực, trigger regenerate |
| `AchievementsController` | CRUD huy hiệu, trao/thu hồi huy hiệu cho user |
| `PlacementTestsController` | Quản lý bài thi xếp lớp |
| `PlacementTestSectionsController` | Quản lý sections |
| `PlacementTestQuestionsController` | Quản lý câu hỏi |
| `PlacementAttemptsController` | Xem lịch sử thi |
| `LearningTopicsController` | CRUD chủ đề học |
| `LearningPathsController` | Xem lộ trình học của học viên |
| `PathTemplatesController` | Quản lý template lộ trình |
| `EnglishSkillsController` | Quản lý kỹ năng tiếng Anh |
| `EnglishProficiencyLevelsController` | Quản lý cấp độ thành thạo |
| `ReferenceSourcesController` | Quản lý nguồn tham khảo |
| `ReferenceReviewController` | Duyệt nguồn tham khảo |
| `AIContentController` | Quản lý nội dung AI |
| `AiUsageLogsController` | Log sử dụng AI |
| `ExportController` | Xuất dữ liệu |
| `SystemSettingsController` | Cài đặt hệ thống |

---

## ❌ Admin – Các chức năng còn THIẾU

### 🎯 Nhóm 1: Quản lý Student (thiếu so với Teacher có)
- **Không có trang quản lý Attendance của học viên** (Teacher điểm danh được, Admin chỉ có thể xem gián tiếp qua User details)
- **Không có trang xem bài tập/submission của học viên** (Teacher có Assignments + Grades)
- **Không có trang quản lý Quiz attempts của học viên theo từng bài** (chỉ có PlacementAttempts)
- **Không có báo cáo tổng hợp học viên** (Teacher có ReportsController, Admin không có)
- **Không có trang quản lý Study Plan của học viên** (không thể can thiệp kế hoạch học)
- **Không có trang xem/gửi tin nhắn giữa Teacher-Student** (không monitor được chat)

### 🎯 Nhóm 2: Quản lý Teacher (thiếu cơ bản)
- **Không có trang quản lý Profile/hồ sơ Giáo viên** (chỉ xem user thông thường, không thấy chuyên môn)
- **Không có trang giao nhiệm vụ / Phân công giáo viên dạy Topic** (Teacher có AssignmentsController, Admin không có giao việc)
- **Không có trang xem báo cáo hiệu suất giảng dạy của Giáo viên** (Teacher report từ phía giáo viên, không có phía Admin nhìn vào)
- **Không có trang quản lý Tài liệu (Resources) mà giáo viên đã tải lên**
- **Không có trang quản lý Quiz do giáo viên tạo** (Admin chỉ có PlacementTest, không có quiz thông thường)

### 🎯 Nhóm 3: Dashboard & Thống kê Admin
- **Không có Dashboard tổng quan Admin** (không có HomeController với thống kê hệ thống)
- **Không có báo cáo thống kê tổng hợp** (số lượng học viên đang hoạt động, tỷ lệ hoàn thành, ...)
- **Không có biểu đồ trực quan hóa dữ liệu**

---

## 📦 PHASE PLAN – Bổ sung chức năng Admin

---

## 🚀 Phase 1 – Dashboard & Báo cáo tổng quan Admin

> **Mục tiêu**: Admin có trang chủ với số liệu tổng quan hệ thống.

### Tasks:
- [x] **1.1** Tạo `Areas/Admin/Controllers/HomeController.cs`
  - Action `Index()`: Thống kê tổng số user (student, teacher, admin)
  - Số học viên đang học, đã hoàn thành lộ trình
  - Số lượng placement test đã làm trong 30 ngày
  - Số thông báo chưa đọc
- [x] **1.2** Tạo `Areas/Admin/Views/Home/Index.cshtml`
  - Cards thống kê tổng quan
  - Biểu đồ người dùng đăng ký theo tháng
  - Biểu đồ tỷ lệ hoàn thành lộ trình học
  - Danh sách học viên mới đăng ký gần nhất
- [x] **1.3** Tạo `AdminDashboardService.cs` trong `Services/`
  - `GetSystemOverviewAsync()`: tổng hợp số liệu
  - `GetNewUsersThisMonthAsync()`: user mới trong tháng
  - `GetLearningPathCompletionStatsAsync()`: tỷ lệ hoàn thành
- [x] **1.4** Tạo `IAdminDashboardService.cs` trong `Services/Interfaces/`
- [x] **1.5** Đăng ký service trong `Program.cs`

---

## 🚀 Phase 2 – Quản lý Học viên (Student Management nâng cao)

> **Mục tiêu**: Admin có đầy đủ công cụ xem & can thiệp vào hoạt động học của học viên.

### Tasks:
- [x] **2.1** Bổ sung action `StudentList()` trong `UserController.cs` (hoặc tách riêng `StudentManagementController`)
  - Filter riêng cho học viên: trạng thái lộ trình, cấp độ, attendance rate
  - Pagination + search
- [x] **2.2** Tạo `Areas/Admin/Controllers/StudentAttendanceController.cs`
  - `Index(int? studentId, int? topicId, DateOnly? from, DateOnly? to)`: Xem lịch sử điểm danh của học viên
  - `ByTopic(int topicId)`: Xem điểm danh theo chủ đề
  - `Export(int studentId)`: Xuất báo cáo điểm danh
- [x] **2.3** Tạo `Areas/Admin/Views/StudentAttendance/Index.cshtml`
  - Bảng điểm danh, filter theo student/topic/date
  - Badge trạng thái (Present/Absent/Late)
- [x] **2.4** Tạo `Areas/Admin/Controllers/StudentQuizController.cs`
  - `Index(int? studentId)`: Danh sách quiz attempts của học viên
  - `Details(int attemptId)`: Chi tiết kết quả quiz
- [x] **2.5** Tạo `Areas/Admin/Views/StudentQuiz/` (Index + Details)
- [x] **2.6** Tạo `Areas/Admin/Controllers/StudentAssignmentController.cs`
  - `Index(int? studentId)`: Danh sách bài tập & trạng thái nộp bài
  - `Details(int submissionId)`: Xem chi tiết submission

---

## 🚀 Phase 3 – Quản lý Giáo viên (Teacher Management nâng cao)

> **Mục tiêu**: Admin có thể xem hồ sơ chuyên môn, phân công và theo dõi hiệu suất giáo viên.

### Tasks:
- [x] **3.1** Tạo `Areas/Admin/Controllers/TeacherManagementController.cs`
  - `Index()`: Danh sách giáo viên (tách khỏi User chung)
  - `Profile(int teacherId)`: Xem hồ sơ chuyên môn giáo viên
  - `Performance(int teacherId)`: Xem báo cáo hiệu suất (số học viên dạy, tỷ lệ hoàn thành, điểm TB)
- [x] **3.2** Tạo `Areas/Admin/Views/TeacherManagement/` (Index, Profile, Performance)
- [x] **3.3** Tạo `IAdminTeacherManagementService.cs` + `AdminTeacherManagementService.cs`
  - `GetTeacherListAsync()`: DS giáo viên với số liệu tóm tắt
  - `GetTeacherProfileAsync(int teacherId)`: Hồ sơ chuyên môn
  - `GetTeacherPerformanceAsync(int teacherId)`: Số liệu hiệu suất
- [x] **3.4** Hợp nhất phân công giáo viên vào `TeacherSchedulesController`
  - Dùng `TeacherSchedules` làm màn quản lý duy nhất cho phân công/lịch dạy
  - Tránh module `TeacherAssignmentAdmin` trùng chức năng, thao tác trực tiếp vào cùng bảng `Schedules`
- [x] **3.5** Bỏ màn `TeacherAssignmentAdmin/` riêng
  - Admin tạo/sửa/xóa phân công tại `Admin/TeacherSchedules`
- [x] **3.6** Tạo `Areas/Admin/Controllers/TeacherResourceAdminController.cs`
  - `Index(int? teacherId)`: Xem tất cả tài liệu mà giáo viên đã upload
  - `Delete(int resourceId)`: Xóa tài liệu vi phạm
  - `Approve(int resourceId)` / `Reject(int resourceId)`: Duyệt tài liệu
- [x] **3.7** Tạo `Areas/Admin/Views/TeacherResourceAdmin/` (Index)

---

## 🚀 Phase 4 – Quản lý Quiz & Bài tập toàn hệ thống

> **Mục tiêu**: Admin có cái nhìn toàn cảnh về quiz và bài tập (không chỉ placement test).

### Tasks:
- [x] **4.1** Tạo `Areas/Admin/Controllers/QuizManagementController.cs`
  - `Index(int? topicId, int? teacherId)`: DS quiz toàn hệ thống
  - `Details(int quizId)`: Xem chi tiết quiz + danh sách câu hỏi
  - `Delete(int quizId)`: Xóa quiz vi phạm
  - `Attempts(int quizId)`: Xem tất cả lượt làm của học viên
- [x] **4.2** Tạo `Areas/Admin/Views/QuizManagement/` (Index, Details, Attempts)
- [x] **4.3** Tạo `IAdminQuizManagementService.cs` + `AdminQuizManagementService.cs`
  - `GetAllQuizzesAsync()`: DS quiz với thống kê
  - `GetQuizAttemptsAsync(int quizId)`: Lịch sử làm quiz
- [x] **4.4** Tạo `Areas/Admin/Controllers/AssignmentManagementController.cs`
  - `Index(int? topicId)`: DS bài tập toàn hệ thống
  - `Submissions(int assignmentId)`: Xem tất cả submission
  - `Delete(int assignmentId)`: Xóa bài tập

---

## 🚀 Phase 5 – Báo cáo & Thống kê Admin

> **Mục tiêu**: Admin có trang báo cáo toàn hệ thống có thể export.

### Tasks:
- [x] **5.1** Tạo `Areas/Admin/Controllers/ReportsController.cs`
  - `StudentProgress()`: Báo cáo tiến độ học viên tổng hợp
  - `TeacherActivity()`: Báo cáo hoạt động giáo viên
  - `AttendanceSummary()`: Báo cáo điểm danh tổng hợp
  - `QuizPerformance()`: Báo cáo hiệu suất quiz
- [x] **5.2** Tạo `Areas/Admin/Views/Reports/` (Index, StudentProgress, TeacherActivity, AttendanceSummary, QuizPerformance)
- [x] **5.3** Tạo `IAdminReportService.cs` + `AdminReportService.cs`
  - `GetStudentProgressReportAsync()`: Tổng hợp tiến độ
  - `GetTeacherActivityReportAsync()`: Hoạt động giáo viên
  - `GetAttendanceSummaryAsync()`: Tóm tắt điểm danh
- [x] **5.4** Bổ sung action export vào `ExportController.cs`
  - `ExportStudentReport(int? studentId)`: Xuất Excel
  - `ExportTeacherReport(int? teacherId)`: Xuất Excel
  - `ExportAttendanceReport()`: Xuất Excel

---

## 🚀 Phase 6 – Giám sát Chat & Tin nhắn

> **Mục tiêu**: Admin có thể giám sát (không phải xem nội dung) luồng chat Teacher-Student và AI Tutor.

### Tasks:
- [x] **6.1** Tạo `Areas/Admin/Controllers/ChatMonitorController.cs`
  - `Index()`: Tổng quan số cuộc hội thoại, tin nhắn gần đây
  - `TeacherChats()`: DS các cuộc chat Teacher-Student (metadata: ai chat với ai, bao nhiêu tin)
  - `AiTutorSessions()`: DS phiên AI Tutor (session summary, không xem nội dung)
- [x] **6.2** Tạo `Areas/Admin/Views/ChatMonitor/` (Index, TeacherChats, AiTutorSessions)
- [x] **6.3** Tạo `IAdminChatMonitorService.cs` + `AdminChatMonitorService.cs`
  - `GetChatStatsAsync()`: Thống kê chat
  - `GetTeacherStudentChatsAsync()`: Metadata các cuộc chat

---

## 📌 Thứ tự ưu tiên thực hiện

| Phase | Độ ưu tiên | Ước tính |
|---|---|---|
| Phase 1 – Dashboard | 🔴 Cao nhất | 1-2 ngày |
| Phase 2 – Student Management | 🔴 Cao | 2-3 ngày |
| Phase 3 – Teacher Management | 🟡 Trung bình-cao | 2-3 ngày |
| Phase 4 – Quiz & Assignment | 🟡 Trung bình | 1-2 ngày |
| Phase 5 – Báo cáo | 🟡 Trung bình | 2 ngày |
| Phase 6 – Chat Monitor | 🟢 Thấp | 1 ngày |
