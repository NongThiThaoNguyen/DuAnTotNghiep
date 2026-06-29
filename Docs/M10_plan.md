# M10 — Kế Hoạch Học Tập (Study Plan) — Codex Plan

## Quy tắc bắt buộc
- Dùng `ProgressStatus` tại `Enums/ProgressStatus.cs` (LOCKED/AVAILABLE/IN_PROGRESS/COMPLETED/NEED_REVIEW/SKIPPED) — KHÔNG tạo enum mới.
- Dùng `NodeType` tại `Enums/NodeType.cs` (TOPIC/LESSON/QUIZ/PRACTICE/REVIEW/AI_TUTOR).
- Query trực tiếp `learning_path_nodes` — KHÔNG tạo bảng `study_plan_tasks`.
- Dùng `IPathViewService` có sẵn tại `Services/Interfaces/IPathViewService.cs` — KHÔNG viết lại các method sau:
  - `BuildNodeTargetUrlAsync(node)` → lấy URL điều hướng
  - `CanOpenNodeAsync(nodeId, userId)` → kiểm tra quyền mở node
  - `MarkNodeCompletedAsync(nodeId, userId, ...)` → đánh dấu hoàn thành
  - `TryUnlockNextNodesAsync(completedNodeId, userId)` → mở node tiếp
- `today`/`overdue` là computed trong Service/ViewModel, KHÔNG phải status DB.
- UI theo `DESIGN.md`: Primary `#2563EB`, border `#E2E8F0`, bo góc 16px, shadow nhẹ, font Roboto.
- Tất cả read query dùng `AsNoTracking()` + `Select()` projection.

## Existing models (đã có, KHÔNG tạo lại)
- `Models/LearningPathNode.cs`: Id, LearningPathId, TopicId, LessonId, QuizId, PracticeTaskId, NodeTitle, NodeType, PathPhase, ScheduledDate(DateOnly?), EstimatedMinutes(int?), OrderIndex, Status, AiReason, CompletedAt, RequiredNodeId + nav properties.
- `Models/StudentLearningPath.cs`: Id, StudentId, Title, Status, StartDate, TargetEndDate, AiPlanSummary, GeneratedByAi, PathVersion + nav `LearningPathNodes`.
- `Models/StudyActivityLog.cs`: Id, StudentId, ActivityType, TopicId, LearningPathNodeId, DurationMinutes, Score, Metadata, CreatedAt.
- `Models/StudentLearningProfile.cs`: có `DailyStudyMinutes(int?)`, `WeeklyStudyDays(int?)`.

---

## PHASE 1 — Entity + Migration (3 tasks)

### P1.1 — Sửa `Models/LearningPathNode.cs`
Thêm 2 property (sau `RequiredNodeId`):
```csharp
public DateOnly? RescheduledFrom { get; set; }
public string? SkippedReason { get; set; }
```

### P1.2 — Tạo `migration_m10_studyplan.sql`
```sql
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='learning_path_nodes' AND COLUMN_NAME='RescheduledFrom')
  ALTER TABLE learning_path_nodes ADD RescheduledFrom DATE NULL;
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='learning_path_nodes' AND COLUMN_NAME='SkippedReason')
  ALTER TABLE learning_path_nodes ADD SkippedReason NVARCHAR(500) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LPN_PathId_ScheduledDate_Status')
  CREATE NONCLUSTERED INDEX IX_LPN_PathId_ScheduledDate_Status ON learning_path_nodes (LearningPathId, ScheduledDate, Status);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_SLP_StudentId_Status')
  CREATE NONCLUSTERED INDEX IX_SLP_StudentId_Status ON student_learning_paths (StudentId, Status);
```

### P1.3 — Sửa `Data/ApplicationDbContext.cs`
Trong `OnModelCreating`, thêm mapping cho `RescheduledFrom`, `SkippedReason` và index `HasIndex(n => new { n.LearningPathId, n.ScheduledDate, n.Status })`.

---

## PHASE 2 — ViewModels + Helper (5 tasks, song song)

### P2.1 — Tạo `ViewModels/StudyPlan/DailyStudyTaskViewModel.cs`
```csharp
namespace DuAnTotNghiep.ViewModels.StudyPlan;
public class DailyStudyTaskViewModel
{
    public int NodeId { get; set; }
    public string Title { get; set; } = "";
    public string? SkillName { get; set; }
    public string NodeType { get; set; } = "";
    public string Status { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusCssClass { get; set; } = "";
    public int EstimatedMinutes { get; set; }
    public string? TargetUrl { get; set; }
    public string? AiReason { get; set; }
    public DateOnly? ScheduledDate { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsTodayTask { get; set; }
    public bool HasConfigError { get; set; }
    public DateOnly? RescheduledFrom { get; set; }
}
```

### P2.2 — Tạo `ViewModels/StudyPlan/TodayStudyPlanViewModel.cs`
```csharp
namespace DuAnTotNghiep.ViewModels.StudyPlan;
public class TodayStudyPlanViewModel
{
    public string PathTitle { get; set; } = "";
    public int TotalMinutesToday { get; set; }
    public int DailyLimitMinutes { get; set; }
    public bool IsOverloaded => TotalMinutesToday > DailyLimitMinutes && DailyLimitMinutes > 0;
    public List<DailyStudyTaskViewModel> TodayTasks { get; set; } = new();
    public List<DailyStudyTaskViewModel> OverdueTasks { get; set; } = new();
    public DailyStudyTaskViewModel? ContinueTask { get; set; }
    public DailyStudyTaskViewModel? NextTask { get; set; }
    public bool HasActivePath { get; set; }
}
```

### P2.3 — Tạo `ViewModels/StudyPlan/WeeklyStudyPlanViewModel.cs`
```csharp
namespace DuAnTotNghiep.ViewModels.StudyPlan;
public class WeeklyStudyPlanViewModel
{
    public string PathTitle { get; set; } = "";
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TotalMinutes { get; set; }
    public double CompletionPercent => TotalTasks > 0 ? Math.Round(CompletedTasks * 100.0 / TotalTasks, 1) : 0;
    public List<DayGroupViewModel> DayGroups { get; set; } = new();
    public bool HasActivePath { get; set; }
}
public class DayGroupViewModel
{
    public DateOnly Date { get; set; }
    public string DayLabel { get; set; } = "";
    public bool IsToday { get; set; }
    public List<DailyStudyTaskViewModel> Tasks { get; set; } = new();
    public int TotalMinutes => Tasks.Sum(t => t.EstimatedMinutes);
}
```

### P2.4 — Tạo `ViewModels/StudyPlan/MonthlyStudyPlanViewModel.cs`
```csharp
namespace DuAnTotNghiep.ViewModels.StudyPlan;
public class MonthlyStudyPlanViewModel
{
    public string PathTitle { get; set; } = "";
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = "";
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalMinutes { get; set; }
    public double CompletionPercent => TotalTasks > 0 ? Math.Round(CompletedTasks * 100.0 / TotalTasks, 1) : 0;
    public List<WeekSummaryViewModel> WeekSummaries { get; set; } = new();
    public bool HasActivePath { get; set; }
}
public class WeekSummaryViewModel
{
    public int WeekNumber { get; set; }
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
    public int TaskCount { get; set; }
    public int CompletedCount { get; set; }
    public int TotalMinutes { get; set; }
    public List<DailyStudyTaskViewModel> Tasks { get; set; } = new();
}
```

### P2.5 — Tạo `Helpers/StudyPlanHelper.cs`
```csharp
namespace DuAnTotNghiep.Helpers;
public static class StudyPlanHelper
{
    public static string GetStatusLabel(string s) => s switch {
        "LOCKED"=>"Chưa mở","AVAILABLE"=>"Sẵn sàng","IN_PROGRESS"=>"Đang học",
        "COMPLETED"=>"Đã xong","NEED_REVIEW"=>"Cần ôn lại","SKIPPED"=>"Đã bỏ qua",_=>s
    };
    public static string GetStatusCssClass(string s, bool overdue) => s switch {
        "COMPLETED"=>"task-completed","IN_PROGRESS"=>"task-in-progress","NEED_REVIEW"=>"task-need-review",
        "SKIPPED"=>"task-skipped","LOCKED"=>"task-locked",_=>overdue?"task-overdue":"task-available"
    };
    public static bool IsOverdue(DateOnly? d, string s) =>
        d.HasValue && s is not("COMPLETED" or "SKIPPED") && d.Value < DateOnly.FromDateTime(DateTime.Today);
    public static bool IsTodayTask(DateOnly? d, string s) =>
        d.HasValue && s is "AVAILABLE" or "IN_PROGRESS" && d.Value == DateOnly.FromDateTime(DateTime.Today);
    public static int GetDefaultMinutes(string t) => t switch {
        "LESSON"=>20,"QUIZ"=>15,"PRACTICE"=>25,"AI_TUTOR"=>15,"REVIEW"=>10,_=>15
    };
    public static string GetViDayName(DayOfWeek d) => d switch {
        DayOfWeek.Monday=>"Thứ Hai",DayOfWeek.Tuesday=>"Thứ Ba",DayOfWeek.Wednesday=>"Thứ Tư",
        DayOfWeek.Thursday=>"Thứ Năm",DayOfWeek.Friday=>"Thứ Sáu",DayOfWeek.Saturday=>"Thứ Bảy",
        DayOfWeek.Sunday=>"Chủ Nhật",_=>""
    };
}
```

---

## PHASE 3 — Service Layer (3 tasks)

### P3.1 — Tạo `Services/Interfaces/IStudyPlanService.cs`
```csharp
using DuAnTotNghiep.ViewModels.StudyPlan;
namespace DuAnTotNghiep.Services.Interfaces;
public interface IStudyPlanService
{
    Task<TodayStudyPlanViewModel> GetTodayPlanAsync(int studentId);
    Task<WeeklyStudyPlanViewModel> GetWeeklyPlanAsync(int studentId, DateOnly? weekStart = null);
    Task<MonthlyStudyPlanViewModel> GetMonthlyPlanAsync(int studentId, int? year = null, int? month = null);
    Task<bool> MarkTaskSkippedAsync(int nodeId, int studentId, string? reason = null);
    Task<List<DailyStudyTaskViewModel>> GetOverdueTasksAsync(int studentId);
    Task<int> CalculateDailyLoadAsync(int studentId, DateOnly? date = null);
}
```

### P3.2 — Tạo `Services/StudyPlanService.cs`
Inject `ApplicationDbContext` + `IPathViewService`. Implement:

**GetTodayPlanAsync(studentId)**:
1. Lấy active path: `student_learning_paths WHERE StudentId==studentId AND Status=="ACTIVE"`, include `LearningPathNodes`. AsNoTracking.
2. Nếu không có → return `HasActivePath=false`.
3. Lọc nodes: `ScheduledDate == today` → TodayTasks. `IsOverdue(ScheduledDate, Status)` → OverdueTasks (max 10, sort ASC).
4. `ContinueTask` = first node `Status == IN_PROGRESS`.
5. `NextTask` = first node `Status == AVAILABLE` AND `ScheduledDate >= today` (or null), sort by OrderIndex.
6. Map mỗi node → `DailyStudyTaskViewModel` dùng `StudyPlanHelper` cho StatusLabel, StatusCssClass, IsOverdue, IsTodayTask.
7. Gọi `_pathViewService.BuildNodeTargetUrlAsync(node)` cho TargetUrl.
8. Lấy `DailyLimitMinutes` từ `StudentLearningProfile.DailyStudyMinutes` (default 60 nếu null).
9. `TotalMinutesToday` = sum EstimatedMinutes của TodayTasks (dùng `GetDefaultMinutes` nếu null).
10. Validate: node có NodeType LESSON nhưng LessonId null → `HasConfigError = true`.

**GetWeeklyPlanAsync(studentId, weekStart)**:
1. `weekStart` default = Monday tuần hiện tại. `weekEnd = weekStart + 6`.
2. Query nodes WHERE `ScheduledDate BETWEEN weekStart AND weekEnd`. AsNoTracking + Select projection.
3. Group by `ScheduledDate` → `DayGroupViewModel` cho 7 ngày (kể cả ngày không có task).
4. Tính `TotalTasks`, `CompletedTasks`, `OverdueTasks`, `TotalMinutes`.

**GetMonthlyPlanAsync(studentId, year, month)**:
1. Default = tháng hiện tại. Query nodes WHERE `ScheduledDate` trong tháng.
2. Group by ISO week → `WeekSummaryViewModel`.
3. Tính tổng.

**MarkTaskSkippedAsync(nodeId, studentId, reason)**:
1. Lấy node, verify ownership (node.LearningPath.StudentId == studentId).
2. KHÔNG cho skip nếu status LOCKED hoặc COMPLETED.
3. Set `Status = "SKIPPED"`, `SkippedReason = reason`.
4. Ghi `StudyActivityLog` type `TASK_SKIPPED`.
5. SaveChanges, return true.

**GetOverdueTasksAsync**: Query `ScheduledDate < today AND Status NOT IN (COMPLETED, SKIPPED, LOCKED)`, max 10.

**CalculateDailyLoadAsync**: Sum `EstimatedMinutes` (default if null) cho nodes scheduled ngày đó.

### P3.3 — Sửa `Program.cs`
Thêm: `builder.Services.AddScoped<IStudyPlanService, StudyPlanService>();`

---

## PHASE 4 — Controller + Views (6 tasks)

### P4.1 — Tạo `Areas/Student/Controllers/StudyPlanController.cs`
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DuAnTotNghiep.Services.Interfaces;
namespace DuAnTotNghiep.Areas.Student.Controllers;

[Area("Student")]
[Authorize]
public class StudyPlanController : Controller
{
    private readonly IStudyPlanService _studyPlanService;
    private readonly IPathViewService _pathViewService;
    // constructor inject

    // Lấy userId từ claims giống các controller Student khác đã có

    // GET /Student/StudyPlan/Today
    public async Task<IActionResult> Today()
    {
        var vm = await _studyPlanService.GetTodayPlanAsync(userId);
        if (!vm.HasActivePath) return RedirectToAction("Index","LearningPath",new{area="Student"});
        return View(vm);
    }

    // GET /Student/StudyPlan/Week?weekStart=2026-06-29
    public async Task<IActionResult> Week(DateTime? weekStart)
    {
        DateOnly? ws = weekStart.HasValue ? DateOnly.FromDateTime(weekStart.Value) : null;
        var vm = await _studyPlanService.GetWeeklyPlanAsync(userId, ws);
        if (!vm.HasActivePath) return RedirectToAction("Index","LearningPath",new{area="Student"});
        return View(vm);
    }

    // GET /Student/StudyPlan/Month?year=2026&month=7
    public async Task<IActionResult> Month(int? year, int? month)
    {
        var vm = await _studyPlanService.GetMonthlyPlanAsync(userId, year, month);
        if (!vm.HasActivePath) return RedirectToAction("Index","LearningPath",new{area="Student"});
        return View(vm);
    }

    // GET /Student/StudyPlan/OpenTask/{nodeId}
    public async Task<IActionResult> OpenTask(int nodeId)
    {
        if (!await _pathViewService.CanOpenNodeAsync(nodeId, userId))
        { TempData["Error"]="Không thể mở nhiệm vụ này."; return RedirectToAction("Today"); }
        // Lấy node từ DB để build URL
        var node = await _db.LearningPathNodes.FindAsync(nodeId);
        var url = await _pathViewService.BuildNodeTargetUrlAsync(node!);
        if (string.IsNullOrEmpty(url)) { TempData["Error"]="Chưa có nội dung."; return RedirectToAction("Today"); }
        return Redirect(url);
    }

    // POST /Student/StudyPlan/SkipTask
    [HttpPost][ValidateAntiForgeryToken]
    public async Task<IActionResult> SkipTask(int nodeId, string? reason)
    {
        await _studyPlanService.MarkTaskSkippedAsync(nodeId, userId, reason);
        return RedirectToAction("Today");
    }
}
```

### P4.2 — Tạo `Areas/Student/Views/StudyPlan/_DailyTaskCard.cshtml`
Model: `DailyStudyTaskViewModel`. Hiển thị:
- Card trắng bo góc 16px, border `#E2E8F0`, shadow nhẹ.
- Title + NodeType badge (LESSON/QUIZ/PRACTICE...) + SkillName.
- EstimatedMinutes + StatusLabel (dùng StatusCssClass cho màu).
- Badge "Quá hạn" nếu IsOverdue. Badge "Hôm nay" nếu IsTodayTask.
- AiReason truncate 100 chars.
- Nút "Bắt đầu học" → link `/Student/StudyPlan/OpenTask/{NodeId}`, disabled nếu LOCKED hoặc TargetUrl null hoặc HasConfigError.
- Nút "Tạm bỏ qua" → form POST `/Student/StudyPlan/SkipTask`, ẩn nếu COMPLETED/LOCKED.
- Nếu HasConfigError → hiện "Nhiệm vụ đang được cập nhật", disabled.

### P4.3 — Tạo `Areas/Student/Views/StudyPlan/Today.cshtml`
Model: `TodayStudyPlanViewModel`. Layout:
- Tab nav: **Today**(active) | Tuần | Tháng — link sang Week/Month.
- Tổng phút: `TotalMinutesToday / DailyLimitMinutes`. Badge "Quá tải" nếu IsOverloaded.
- Nếu `!HasActivePath`: "Chưa có lộ trình" + link tạo.
- Nếu ContinueTask != null: card lớn "Tiếp tục học" highlight.
- Nếu OverdueTasks.Any(): section warning "Nhiệm vụ quá hạn" + render `_DailyTaskCard` cho mỗi task.
- Section "Nhiệm vụ hôm nay" + render `_DailyTaskCard`.
- Nếu NextTask != null (không trùng today): section "Nhiệm vụ tiếp theo".
- Empty state nếu không có task: "Hôm nay bạn đã hoàn thành! 🎉".

### P4.4 — Tạo `Areas/Student/Views/StudyPlan/Week.cshtml`
Model: `WeeklyStudyPlanViewModel`. Layout:
- Tab nav: Today | **Tuần**(active) | Tháng.
- Header: WeekStart–WeekEnd, nút ◀ Tuần trước | Tuần sau ▶ (link với `?weekStart=`).
- Progress bar: CompletionPercent%, CompletedTasks/TotalTasks, TotalMinutes phút.
- Foreach DayGroups: header `DayLabel + Date`, highlight nếu IsToday, render `_DailyTaskCard` cho Tasks.
- Empty day: "Không có nhiệm vụ".

### P4.5 — Tạo `Areas/Student/Views/StudyPlan/Month.cshtml`
Model: `MonthlyStudyPlanViewModel`. Layout:
- Tab nav: Today | Tuần | **Tháng**(active).
- Header: MonthLabel, nút ◀ Tháng trước | Tháng sau ▶.
- Progress bar: CompletionPercent%, CompletedTasks/TotalTasks.
- Foreach WeekSummaries: "Tuần N (DD/MM–DD/MM)", TaskCount tasks, CompletedCount xong, TotalMinutes phút. Click → link Week.
- Empty: "Chưa có kế hoạch cho tháng này".

### P4.6 — Tạo `wwwroot/css/studyplan.css`
CSS classes: `.task-card` (bg white, border #E2E8F0, radius 16px, shadow 0 1px 3px rgba(0,0,0,.1), padding 16px, margin-bottom 12px), `.task-completed` (border-left 4px #22C55E), `.task-in-progress` (border-left 4px #2563EB), `.task-overdue` (border-left 4px #EF4444), `.task-locked` (opacity .5), `.task-available` (border-left 4px #F59E0B), `.task-need-review` (border-left 4px #F59E0B), `.task-skipped` (opacity .6, text-decoration line-through). Tab nav, progress bar, overload badge, responsive (mobile 320px+, tablet 768px+, desktop 1024px+).

---

## PHASE 5 — Business Logic + Admin (5 tasks)

### P5.1 — Stub M17 + M18 (thêm vào `StudyPlanService.cs`)
```csharp
// PHASE 2 — stub only
public Task<object?> BuildReplanningContextAsync(int studentId) => Task.FromResult<object?>(null);
public Task<object?> BuildStudyReminderAsync(int studentId) => Task.FromResult<object?>(null);
```

### P5.2 — Liên kết M16 Progress (sửa `StudyPlanService.cs`)
Sau MarkTaskSkippedAsync thành công, nếu `IProgressTrackingService` đã inject → gọi cập nhật. Nếu lỗi → log warning, KHÔNG rollback.

### P5.3 — Tạo `Areas/Admin/Controllers/StudyPlansController.cs`
```csharp
[Area("Admin")][Authorize(Roles="Admin")]
public class StudyPlansController : Controller
{
    // Inject IStudyPlanService
    // GET /Admin/StudyPlans/Preview/{studentId} → gọi GetTodayPlanAsync(studentId)
    // GET /Admin/StudyPlans/PreviewWeek/{studentId}?weekStart=
    // GET /Admin/StudyPlans/PreviewMonth/{studentId}?year=&month=
}
```
Dùng chung IStudyPlanService, truyền studentId trực tiếp (admin bỏ ownership check).

### P5.4 — Tạo `Areas/Admin/Views/StudyPlans/Preview.cshtml`
Giống Today.cshtml nhưng read-only: ẩn nút Skip/Complete/BắtĐầu, hiển thị tên student ở header. Tab Today/Week/Month link sang PreviewWeek/PreviewMonth.

### P5.5 — Validate integrity (sửa `StudyPlanService.cs`)
Trong map node → ViewModel: nếu NodeType là LESSON nhưng LessonId null, hoặc QUIZ nhưng QuizId null, hoặc PRACTICE nhưng PracticeTaskId null → set `HasConfigError = true`.

---

## PHASE 6 — Seed Data + Test (2 tasks)

### P6.1 — Tạo `migration_m10_seed_demo.sql`
Seed 1 student có active path ~15 nodes: 3 scheduled hôm nay (1 AVAILABLE, 1 IN_PROGRESS, 1 COMPLETED), 2 quá hạn (AVAILABLE scheduled 3 ngày trước), 5 tuần này (mix status), 3 tuần sau, 2 LOCKED. Mix NodeType. Link LessonId/QuizId/PracticeTaskId hợp lệ. Set `StudentLearningProfile.DailyStudyMinutes = 60`.

### P6.2 — Tạo `Tests/M10_StudyPlan_TestCases.md`
Test cases: (1) No active path → redirect LearningPath, (2) No today tasks → empty state, (3) 2 today + 1 overdue → overdue trước, (4) IN_PROGRESS → widget Tiếp tục, (5) Open LESSON node → redirect lesson, (6) Open LOCKED → disabled, (7) Skip → SKIPPED + log, (8) Week nav trước/sau, (9) Month empty, (10) Admin preview read-only, (11) Student A ≠ Student B ownership, (12) Overload badge, (13) Config error disabled, (14) Mobile responsive.

---

## File tổng hợp

| File | Action |
|------|--------|
| `Models/LearningPathNode.cs` | MODIFY +2 props |
| `migration_m10_studyplan.sql` | NEW |
| `Data/ApplicationDbContext.cs` | MODIFY mapping+index |
| `ViewModels/StudyPlan/DailyStudyTaskViewModel.cs` | NEW |
| `ViewModels/StudyPlan/TodayStudyPlanViewModel.cs` | NEW |
| `ViewModels/StudyPlan/WeeklyStudyPlanViewModel.cs` | NEW |
| `ViewModels/StudyPlan/MonthlyStudyPlanViewModel.cs` | NEW |
| `Helpers/StudyPlanHelper.cs` | NEW |
| `Services/Interfaces/IStudyPlanService.cs` | NEW |
| `Services/StudyPlanService.cs` | NEW |
| `Program.cs` | MODIFY +1 DI line |
| `Areas/Student/Controllers/StudyPlanController.cs` | NEW |
| `Areas/Student/Views/StudyPlan/_DailyTaskCard.cshtml` | NEW |
| `Areas/Student/Views/StudyPlan/Today.cshtml` | NEW |
| `Areas/Student/Views/StudyPlan/Week.cshtml` | NEW |
| `Areas/Student/Views/StudyPlan/Month.cshtml` | NEW |
| `wwwroot/css/studyplan.css` | NEW |
| `Areas/Admin/Controllers/StudyPlansController.cs` | NEW |
| `Areas/Admin/Views/StudyPlans/Preview.cshtml` | NEW |
| `migration_m10_seed_demo.sql` | NEW |
| `Tests/M10_StudyPlan_TestCases.md` | NEW |
