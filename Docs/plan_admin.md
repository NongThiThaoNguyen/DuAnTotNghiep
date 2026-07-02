# 📋 KẾ HOẠCH NÂNG CẤP — PHẦN ADMIN

> **Dự án**: AI Study English  
> **Mục tiêu**: Nâng cấp backend & hoàn thiện chức năng quản trị để demo  
> **Nguyên tắc**: Backend-first, UI chỉ cần đồng bộ — ưu tiên task nhỏ/vừa, tránh task nặng  
> **Ngày tạo**: 30/06/2026

---

## 📊 PHÂN TÍCH HIỆN TRẠNG ADMIN

### ✅ Đã có (18 controllers, 55 views):
- CRUD: EnglishSkills, ProficiencyLevels, LearningTopics, ReferenceSources
- Quản lý: PlacementTests, Sections, Questions, Attempts
- AI: Content Review, Prompts, Usage Logs
- Lộ trình: PathTemplates, LearningPaths admin
- Người dùng: UserController (list, detail, learning profile)
- Schema Status, Competency History
- Dashboard: chỉ hiển thị data cơ bản qua `DashboardService`

### ❌ Còn thiếu / Cần nâng cấp:
- Dashboard Admin quá sơ sài (chỉ 1 action, chưa có thống kê chi tiết)
- Chưa có quản lý Notification (tạo/gửi thông báo cho user)
- Chưa có quản lý SystemSettings (cài đặt hệ thống)
- Chưa có trang AuditLog để xem nhật ký thao tác
- Chưa có trang thống kê tổng quan (Overview Statistics)
- Chưa có chức năng export dữ liệu (Excel)
- Chưa có quản lý Achievements (huy hiệu cho gamification)
- User Management thiếu: đổi role, khóa/mở khóa, reset password
- Chưa có API endpoint trả JSON cho các biểu đồ Dashboard

---

## 🚀 PHASE 1 — Nâng cấp Admin Dashboard (4 tasks)
> **Mục tiêu**: Dashboard hiển thị đầy đủ thống kê, biểu đồ tổng quan
> **Độ khó**: ⭐⭐ Nhẹ

### Task 1.1 — Nâng cấp `DashboardService` thêm thống kê chi tiết
- [x] Mở file [DashboardService.cs](file:///e:/DuAnTotNghiep/Services/DashboardService.cs)
- [x] Thêm method `GetAdminOverviewAsync()` trả về `AdminDashboardViewModel` gồm:
  - `TotalUsers` (int) — đếm từ bảng `users`
  - `TotalStudents`, `TotalTeachers` — đếm theo role
  - `ActiveUsersToday` — users có `LastLoginAt` trong ngày
  - `TotalTopics` — đếm từ `learning_topics` với `IsActive = true`
  - `TotalQuizzes` — đếm từ `quizzes`
  - `TotalLessons` — đếm từ `original_lessons`
  - `PendingAiContents` — đếm `ai_generated_contents` có `ReviewStatus = "PENDING"`
  - `TotalPlacementAttempts` — đếm từ `test_attempts`
  - `AveragePlacementScore` — trung bình `TotalScore` từ `test_attempts`
  - `NewUsersThisWeek` — đếm users có `CreatedAt` trong 7 ngày qua
  - `NewUsersThisMonth` — đếm users có `CreatedAt` trong 30 ngày qua
- [x] Tất cả query dùng `AsNoTracking()`
- [x] Đăng ký interface method trong [IDashboardService.cs](file:///e:/DuAnTotNghiep/Services/Interfaces)

### Task 1.2 — Tạo `AdminDashboardViewModel`
- [x] Tạo file `Models/ViewModels/Admin/AdminDashboardViewModel.cs`
- [x] Khai báo tất cả properties từ Task 1.1
- [x] Thêm property `List<RecentUserViewModel> RecentUsers` (5 user mới nhất)
- [x] Thêm property `List<RecentActivityViewModel> RecentActivities` (10 hoạt động gần nhất)
- [x] Tạo class `RecentUserViewModel` gồm: Id, FullName, Email, RoleName, CreatedAt, AvatarUrl
- [x] Tạo class `RecentActivityViewModel` gồm: Id, UserName, ActivityType, Description, CreatedAt

### Task 1.3 — Cập nhật Admin `HomeController` sử dụng ViewModel mới
- [x] Mở [HomeController.cs](file:///e:/DuAnTotNghiep/Areas/Admin/Controllers/HomeController.cs)
- [x] Thay `GetDashboardDataAsync()` bằng `GetAdminOverviewAsync()`
- [x] Truyền `AdminDashboardViewModel` vào View
- [x] Thêm action `[HttpGet] DashboardChartData()` trả JSON cho biểu đồ:
  - Số user đăng ký theo 7 ngày gần nhất
  - Phân bố user theo role (pie chart data)
  - Số placement test attempts theo ngày

### Task 1.4 — Cập nhật Admin Dashboard View
- [x] Cập nhật file `Areas/Admin/Views/Home/Index.cshtml`
- [x] Dùng `@model AdminDashboardViewModel`
- [x] Hiển thị stat cards: Tổng Users, Students, Teachers, Topics, Quizzes, AI Pending
- [x] Hiển thị bảng Recent Users (5 user mới)
- [x] Hiển thị bảng Recent Activities (10 hoạt động gần nhất)
- [x] Giữ layout `_AdminLayout.cshtml`, Tailwind CSS, font Roboto

---

## 🚀 PHASE 2 — Quản lý Thông báo - Notification Management (5 tasks)
> **Mục tiêu**: Admin có thể tạo, gửi, xem thông báo cho users
> **Độ khó**: ⭐⭐ Nhẹ-Vừa

### Task 2.1 — Tạo `NotificationService`
- [x] Tạo file `Services/Interfaces/INotificationService.cs` với methods:
  - `Task<List<Notification>> GetAllAsync(int page, int pageSize)`
  - `Task<Notification?> GetByIdAsync(int id)`
  - `Task<int> GetTotalCountAsync()`
  - `Task CreateAsync(Notification notification)`
  - `Task DeleteAsync(int id)`
  - `Task<int> GetUnreadCountAsync(int userId)`
  - `Task MarkAsReadAsync(int notificationId, int userId)`
  - `Task SendToAllStudentsAsync(string title, string content, string type, int createdBy)`
  - `Task SendToUserAsync(string title, string content, string type, int targetUserId, int createdBy)`
- [x] Tạo file `Services/NotificationService.cs` implement interface
- [x] Tất cả read query dùng `AsNoTracking()`
- [x] Đăng ký DI trong [Program.cs](file:///e:/DuAnTotNghiep/Program.cs)

### Task 2.2 — Tạo `NotificationViewModel`
- [x] Tạo file `Models/ViewModels/Admin/NotificationViewModels.cs` gồm:
  - `NotificationListViewModel`: List notifications + phân trang
  - `CreateNotificationViewModel`: Title, Content, NotificationType (SYSTEM/INFO/WARNING/URGENT), TargetType (ALL/STUDENTS/TEACHERS/SPECIFIC_USER), TargetUserId?
- [x] Thêm validation annotations: `[Required]`, `[StringLength]`

### Task 2.3 — Tạo Admin `NotificationsController`
- [x] Tạo file `Areas/Admin/Controllers/NotificationsController.cs`
- [x] `[Area("Admin")]`, `[Authorize(Roles = "ADMIN")]`
- [x] Action `Index(int page = 1)` — danh sách thông báo với phân trang
- [x] Action `Create()` GET — form tạo thông báo
- [x] Action `Create(CreateNotificationViewModel model)` POST — lưu thông báo
- [x] Action `Details(int id)` — xem chi tiết
- [x] Action `Delete(int id)` POST — xóa thông báo
- [x] Inject `INotificationService`, `IUserService`

### Task 2.4 — Tạo Views cho Notification
- [x] Tạo thư mục `Areas/Admin/Views/Notifications/`
- [x] Tạo `Index.cshtml` — bảng danh sách, phân trang, badge theo type
- [x] Tạo `Create.cshtml` — form tạo với dropdown type, radio target
- [x] Tạo `Details.cshtml` — xem chi tiết, danh sách người đã đọc
- [x] Tạo `_Delete.cshtml` — confirm dialog xóa

### Task 2.5 — Thêm menu Notifications vào sidebar Admin
- [x] Mở layout `_AdminLayout.cshtml` (trong Views/Shared hoặc Admin area)
- [x] Thêm menu item "Thông báo" với icon bell, link đến `/Admin/Notifications`
- [x] Hiển thị badge số thông báo pending (nếu có)

---

## 🚀 PHASE 3 — Quản lý User nâng cao (4 tasks)
> **Mục tiêu**: Đổi role, khóa/mở khóa tài khoản, reset password cho user
> **Độ khó**: ⭐⭐ Nhẹ-Vừa

### Task 3.1 — Nâng cấp `UserService` thêm chức năng quản trị
- [x] Mở [UserService.cs](file:///e:/DuAnTotNghiep/Services/UserService.cs)
- [x] Thêm method `ChangeRoleAsync(int userId, int newRoleId)` — đổi role
- [x] Thêm method `ToggleLockAsync(int userId)` — khóa/mở khóa (set Status = "LOCKED"/"ACTIVE", LockoutUntil)
- [x] Thêm method `AdminResetPasswordAsync(int userId, string newPassword)` — admin đặt lại mật khẩu (BCrypt hash)
- [x] Thêm method `GetUserStatisticsAsync(int userId)` — trả về stats: số quiz đã làm, số lesson đã hoàn thành, số ngày hoạt động, tổng thời gian học
- [x] Cập nhật interface `IUserService`

### Task 3.2 — Nâng cấp Admin `UserController`
- [x] Mở [UserController.cs](file:///e:/DuAnTotNghiep/Areas/Admin/Controllers/UserController.cs)
- [x] Thêm action `ChangeRole(int id, int roleId)` POST
- [x] Thêm action `ToggleLock(int id)` POST
- [x] Thêm action `ResetPassword(int id)` GET/POST — form nhập mật khẩu mới
- [x] Thêm action `Statistics(int id)` — trang thống kê chi tiết user
- [x] Mỗi action thêm `AuditService.LogAsync()` ghi nhật ký

### Task 3.3 — Tạo ViewModels cho User Management
- [x] Tạo `Models/ViewModels/Admin/UserManagementViewModels.cs`
- [x] `ChangeRoleViewModel`: UserId, CurrentRoleId, NewRoleId, Roles list
- [x] `ResetPasswordViewModel`: UserId, UserEmail, NewPassword, ConfirmPassword
- [x] `UserStatisticsViewModel`: UserInfo + TotalQuizzes, TotalLessons, TotalStudyMinutes, TotalDaysActive, LastLoginAt, CompetencyScore
- [x] Validation annotations đầy đủ

### Task 3.4 — Cập nhật Views User Management
- [x] Cập nhật `Areas/Admin/Views/User/Details.cshtml` — thêm nút: Đổi Role, Khóa/Mở khóa, Reset Password
- [x] Tạo `Areas/Admin/Views/User/ResetPassword.cshtml` — form reset password
- [x] Tạo `Areas/Admin/Views/User/Statistics.cshtml` — trang thống kê chi tiết user
- [x] Thêm confirm dialog cho các thao tác nguy hiểm (khóa, reset password)

---

## 🚀 PHASE 4 — Quản lý AuditLog & System Settings (5 tasks)
> **Mục tiêu**: Xem nhật ký thao tác hệ thống, quản lý cài đặt
> **Độ khó**: ⭐⭐ Nhẹ

### Task 4.1 — Tạo `AuditLogManagementService`
- [x] Tạo `Services/Interfaces/IAuditLogManagementService.cs` gồm:
  - `Task<List<AuditLog>> GetLogsAsync(string? action, string? user, DateTime? from, DateTime? to, int page, int pageSize)`
  - `Task<int> GetTotalCountAsync(string? action, string? user, DateTime? from, DateTime? to)`
  - `Task<List<string>> GetDistinctActionsAsync()` — lấy danh sách action types để filter
- [x] Tạo `Services/AuditLogManagementService.cs` implement
- [x] Dùng `AsNoTracking()`, Include User navigation
- [x] Đăng ký DI trong Program.cs

### Task 4.2 — Tạo Admin `AuditLogsController`
- [x] Tạo `Areas/Admin/Controllers/AuditLogsController.cs`
- [x] Action `Index(string? action, string? user, DateTime? from, DateTime? to, int page = 1)` — danh sách với filter + phân trang
- [x] Action `Details(int id)` — xem chi tiết log entry
- [x] `[Area("Admin")]`, `[Authorize(Roles = "ADMIN")]`

### Task 4.3 — Tạo `SystemSettingService`
- [x] Tạo `Services/Interfaces/ISystemSettingService.cs`:
  - `Task<List<SystemSetting>> GetAllAsync()`
  - `Task<SystemSetting?> GetByKeyAsync(string key)`
  - `Task<string?> GetValueAsync(string key)`
  - `Task UpdateAsync(string key, string value, int updatedBy)`
  - `Task SeedDefaultsAsync()` — tạo setting mặc định nếu chưa có
- [x] Tạo `Services/SystemSettingService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 4.4 — Tạo Admin `SystemSettingsController`
- [x] Tạo `Areas/Admin/Controllers/SystemSettingsController.cs`
- [x] Action `Index()` — hiển thị tất cả settings dạng bảng editable
- [x] Action `Update(string key, string value)` POST — cập nhật setting
- [x] Action `SeedDefaults()` POST — tạo settings mặc định
- [x] `[Area("Admin")]`, `[Authorize(Roles = "ADMIN")]`

### Task 4.5 — Tạo Views cho AuditLogs & SystemSettings
- [x] Tạo `Areas/Admin/Views/AuditLogs/Index.cshtml` — bảng log + filter (action, user, date range) + phân trang
- [x] Tạo `Areas/Admin/Views/AuditLogs/Details.cshtml` — chi tiết log entry
- [x] Tạo `Areas/Admin/Views/SystemSettings/Index.cshtml` — bảng settings với inline edit
- [x] Thêm menu AuditLogs và SystemSettings vào sidebar Admin

---

## 🚀 PHASE 5 — Quản lý Achievement (Gamification Backend) (4 tasks)
> **Mục tiêu**: CRUD huy hiệu, quản lý trao huy hiệu cho user
> **Độ khó**: ⭐⭐ Nhẹ-Vừa

### Task 5.1 — Tạo `AchievementService`
- [x] Tạo `Services/Interfaces/IAchievementService.cs`:
  - `Task<List<Achievement>> GetAllAsync()`
  - `Task<Achievement?> GetByIdAsync(int id)`
  - `Task CreateAsync(Achievement achievement)`
  - `Task UpdateAsync(Achievement achievement)`
  - `Task DeleteAsync(int id)`
  - `Task<List<UserAchievement>> GetUserAchievementsAsync(int userId)`
  - `Task GrantAchievementAsync(int userId, int achievementId)` — trao huy hiệu
  - `Task RevokeAchievementAsync(int userId, int achievementId)` — thu hồi
- [x] Tạo `Services/AchievementService.cs` implement
- [x] Đăng ký DI trong Program.cs

### Task 5.2 — Tạo Admin `AchievementsController`
- [x] Tạo `Areas/Admin/Controllers/AchievementsController.cs`
- [x] CRUD actions: Index, Create, Edit, Delete
- [x] Action `UserAchievements(int userId)` — xem huy hiệu của user
- [x] Action `Grant(int userId, int achievementId)` POST — trao
- [x] Action `Revoke(int userId, int achievementId)` POST — thu hồi

### Task 5.3 — Tạo ViewModels
- [x] Tạo `Models/ViewModels/Admin/AchievementViewModels.cs`:
  - `AchievementFormViewModel`: Code, Title, Description, IconUrl, XpReward, IsActive
  - `AchievementListViewModel`: List + phân trang
  - `UserAchievementDetailViewModel`: UserInfo + List badges

### Task 5.4 — Tạo Views cho Achievements
- [x] Tạo `Areas/Admin/Views/Achievements/Index.cshtml` — bảng danh sách
- [x] Tạo `Areas/Admin/Views/Achievements/Create.cshtml` — form tạo
- [x] Tạo `Areas/Admin/Views/Achievements/Edit.cshtml` — form sửa
- [x] Thêm menu Achievements vào sidebar Admin

---

## 🚀 PHASE 6 — Export dữ liệu & Báo cáo (3 tasks)
> **Mục tiêu**: Xuất dữ liệu Excel cho quản trị
> **Độ khó**: ⭐⭐ Nhẹ (dùng ClosedXML đã có trong dự án)

### Task 6.1 — Tạo `ExportService`
- [x] Tạo `Services/Interfaces/IExportService.cs`:
  - `Task<byte[]> ExportUsersToExcelAsync()`
  - `Task<byte[]> ExportPlacementResultsToExcelAsync()`
  - `Task<byte[]> ExportAuditLogsToExcelAsync(DateTime? from, DateTime? to)`
  - `Task<byte[]> ExportAiUsageLogsToExcelAsync()`
- [x] Tạo `Services/ExportService.cs` implement dùng `ClosedXML`
- [x] Mỗi method tạo worksheet với headers, data rows, auto-fit columns
- [x] Đăng ký DI trong Program.cs

### Task 6.2 — Tạo Admin `ExportController`
- [x] Tạo `Areas/Admin/Controllers/ExportController.cs`
- [x] Action `ExportUsers()` — trả file `Users_yyyyMMdd.xlsx`
- [x] Action `ExportPlacementResults()` — trả file `PlacementResults_yyyyMMdd.xlsx`
- [x] Action `ExportAuditLogs(DateTime? from, DateTime? to)` — trả file
- [x] Action `ExportAiUsageLogs()` — trả file
- [x] Tất cả trả `File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename)`

### Task 6.3 — Thêm nút Export vào các trang hiện có
- [x] Thêm nút "Xuất Excel" vào `Areas/Admin/Views/User/Index.cshtml`
- [x] Thêm nút "Xuất Excel" vào `Areas/Admin/Views/AuditLogs/Index.cshtml`
- [x] Thêm nút "Xuất Excel" vào `Areas/Admin/Views/AiUsageLogs/Index.cshtml`
- [x] Thêm nút "Xuất Excel" vào trang PlacementAttempts

---

## Phase 7 — Cài đặt hệ thống (Settings) & Báo cáo
> **Mục tiêu**: Cho phép Admin tuỳ chỉnh các cấu hình chung của hệ thống (System Settings) và cập nhật giao diện Báo cáo.
> **Độ khó**: ⭐⭐ Nhẹ

### Task 7.1 — Xây dựng `SystemSettingService`
- [x] Tạo `Services/Interfaces/ISystemSettingService.cs`:
  - `Task<List<SystemSetting>> GetAllSettingsAsync()`
  - `Task<SystemSetting?> GetSettingByKeyAsync(string key)`
  - `Task UpdateSettingAsync(string key, string value, int updatedBy)`
- [x] Tạo `Services/SystemSettingService.cs` implement interface trên.
- [x] Đăng ký `ISystemSettingService` vào DI trong `Program.cs`.

### Task 7.2 — Tạo Admin `SettingsController`
- [x] Tạo `Areas/Admin/Controllers/SettingsController.cs`.
- [x] Action `Index` (GET): Lấy tất cả cài đặt và truyền vào View.
- [x] Action `Update` (POST): Cập nhật giá trị của một setting cụ thể qua AJAX hoặc Form Submit.

### Task 7.3 — Xây dựng giao diện Settings
- [x] Tạo View `Areas/Admin/Views/Settings/Index.cshtml`.
- [x] Hiển thị danh sách các cấu hình, bao gồm Key, Value, Description.
- [x] Bố trí Modal hoặc Inline Form để chỉnh sửa Value.

### Task 7.4 — Liên kết Sidebar
- [x] Cập nhật menu "Cài đặt" trong `_AdminLayout.cshtml` trỏ đúng vào `SettingsController`.

---

## 📋 TỔNG KẾT

| Phase | Số tasks | Độ khó | Mô tả |
|-------|----------|--------|-------|
| 1 | 4 | ⭐⭐ | Nâng cấp Dashboard Admin |
| 2 | 5 | ⭐⭐ | Quản lý Notification |
| 3 | 4 | ⭐⭐ | Quản lý User nâng cao |
| 4 | 5 | ⭐⭐ | AuditLog & System Settings |
| 5 | 4 | ⭐⭐ | Achievement Management |
| 6 | 3 | ⭐⭐ | Export Excel & Báo cáo |
| 7 | 4 | ⭐⭐ | System Settings |
| **Tổng** | **29 tasks** | | **Tất cả nhẹ-vừa, demo-ready** |

> **Ghi chú cho AI Agent**: Làm từ Phase 1 → 6 theo thứ tự. Mỗi task hoàn thành thì tích `[x]` vào checkbox. Mỗi phase hoàn thành kiểm tra build thành công trước khi sang phase tiếp theo. Nếu gặp lỗi build, sửa ngay trước khi tiếp tục.
