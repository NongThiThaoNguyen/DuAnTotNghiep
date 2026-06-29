# M8 — AI Learning Path Engine — Kế hoạch triển khai

> **Module:** M8 — AI Learning Path Engine
> **Mục tiêu:** Sinh learning path cá nhân hóa dựa trên profile, placement test, competency analysis, khung kỹ năng/topic/lesson/quiz đã duyệt.
> **Nhánh:** `ngthebao-m8`
> **Bắt buộc đọc trước:** `RULES.md`, `DESIGN.md` (khi có UI)

---

## Phase 1: Rà soát CSDL & Chuẩn hóa Schema (Task 1)

### Task 1.1 — Kiểm tra schema hiện tại của `student_learning_paths`
- [x] Đọc `Data/ApplicationDbContext.cs` phần cấu hình bảng `student_learning_paths`.
- [x] Xác nhận các cột đã có: `Id`, `StudentId`, `TemplateId`, `CompetencyAnalysisId`, `Title`, `Description`, `GoalId`, `StartDate`, `TargetEndDate`, `AiPlanSummary`, `Status`, `GeneratedByAi`, `CreatedAt`, `UpdatedAt`.
- [x] Ghi chú nếu thiếu cột: `path_version`, `archived_at`, `replaced_by_path_id`, `ai_model`, `prompt_template_id`.
  - Ghi chú: `student_learning_paths` thiếu `path_version`, `archived_at`, `replaced_by_path_id`, `ai_model`, `prompt_template_id`.
- [x] **Test:** Chạy `dotnet build` pass.

### Task 1.2 — Kiểm tra schema hiện tại của `learning_path_nodes`
- [x] Đọc cấu hình bảng `learning_path_nodes` trong `ApplicationDbContext.cs`.
- [x] Xác nhận các cột đã có: `Id`, `LearningPathId`, `TopicId`, `LessonId`, `QuizId`, `PracticeTaskId`, `NodeTitle`, `NodeDescription`, `NodeType`, `PathPhase`, `ScheduledDate`, `EstimatedMinutes`, `OrderIndex`, `Status`, `AiReason`, `CompletedAt`.
- [x] Ghi chú nếu thiếu cột: `unlock_condition`, `required_node_id`.
  - Ghi chú: `learning_path_nodes` thiếu `unlock_condition`, `required_node_id`.
- [x] **Test:** Chạy `dotnet build` pass.

### Task 1.3 — Tạo migration SQL bổ sung cột (nếu cần)
- [x] Nếu Task 1.1/1.2 phát hiện cột thiếu, tạo file `migration_m8_schema.sql` tại root.
- [x] Thêm cột `path_version INT DEFAULT 1`, `archived_at DATETIME NULL`, `replaced_by_path_id INT NULL` vào `student_learning_paths` (nếu thiếu).
- [x] Thêm cột `required_node_id INT NULL` vào `learning_path_nodes` (nếu thiếu).
- [x] Tạo index: `IX_student_learning_paths_student_id_status`, `IX_learning_path_nodes_learning_path_id_order_index`.
- [x] **Test:** Migration SQL chạy được trên DB dev. `dotnet build` pass.

### Task 1.4 — Cập nhật Entity Model nếu thêm cột mới
- [x] Cập nhật `Models/StudentLearningPath.cs` thêm property mới (nếu có cột mới từ Task 1.3).
- [x] Cập nhật `Models/LearningPathNode.cs` thêm property `RequiredNodeId` (nếu thêm cột).
- [x] Cập nhật `ApplicationDbContext.OnModelCreating()` cấu hình cột/FK mới bằng Fluent API.
- [x] **Test:** `dotnet build` pass. Không phá code M9 hiện tại.

---

## Phase 2: Chuẩn hóa Input & DTO (Task 2 + Task 3)

### Task 2.1 — Tạo `LearningPathInputDto`
- [x] Tạo file `DTOs/LearningPath/LearningPathInputDto.cs`.
- [x] Bao gồm: `StudentId`, `GoalName`, `TargetLevelName`, `CurrentLevelName`, `AvailableMinutesPerDay`, `SkillPriorities` (list), `Strengths`, `Weaknesses`, `PriorityTopics` (list), `AvailableTopics` (list id+name), `AvailableLessons` (list id+name), `AvailableQuizzes` (list id+name).
- [x] **Test:** `dotnet build` pass.

### Task 2.2 — Tạo `LearningPathOutputDto` (AI output schema)
- [x] Tạo file `DTOs/LearningPath/LearningPathOutputDto.cs`.
- [x] Gồm: `PathTitle`, `Summary`, `TotalWeeks`, `Phases[]` (mỗi phase có `PhaseName`, `Weeks`, `Nodes[]`).
- [x] Mỗi Node gồm: `NodeTitle`, `NodeDescription`, `ActionType` (map NodeType), `TopicId?`, `LessonId?`, `QuizId?`, `PracticeTaskId?`, `EstimatedMinutes`, `AiReason`, `ScheduledDay?`, `PathPhase`.
- [x] **Test:** `dotnet build` pass.

### Task 2.3 — Tạo Enum `LearningPathStatus`
- [x] Tạo file `Enums/LearningPathStatus.cs`.
- [x] Giá trị: `Active = "ACTIVE"`, `Archived = "ARCHIVED"`, `Paused = "PAUSED"`, `Failed = "FAILED"`, `Generating = "GENERATING"`.
- [x] Theo pattern string-const static class giống `ProgressStatus.cs`.
- [x] **Test:** `dotnet build` pass.

### Task 2.4 — Tạo ViewModels cho M8
- [x] Tạo folder `ViewModels/LearningPath/M8/`.
- [x] Tạo `LearningPathSummaryViewModel.cs`: `PathId`, `Title`, `Status`, `AiPlanSummary`, `TotalNodes`, `CompletedNodes`, `CurrentNodeTitle`, `NextNodeTitle`, `StartDate`, `TargetEndDate`, `PriorityTopics`, `GeneratedByAi`, `PathVersion`.
- [x] Tạo `LearningPathDetailViewModel.cs`: kế thừa Summary + `Nodes[]` (dùng `PathNodeViewModel` đã có từ M9).
- [x] Tạo `LearningPathGenerateViewModel.cs`: `StudentId`, `HasOnboarding`, `HasPlacementTest`, `HasCompetencyAnalysis`, `HasActivePath`, `MissingStep` (string mô tả thiếu gì).
- [x] Tạo `AdminPathHistoryViewModel.cs`: `Paths[]` mỗi item gồm `PathId`, `StudentName`, `Status`, `CreatedAt`, `GeneratedByAi`, `AiModel`, `ErrorMessage`.
- [x] **Test:** `dotnet build` pass.

---

## Phase 3: Service & Repository Layer (Task 4)

### Task 3.1 — Tạo `ILearningPathRepository` interface
- [x] Tạo file `Repositories/Interfaces/ILearningPathRepository.cs`.
- [x] Methods: `GetActivePathByStudentIdAsync(int studentId)`, `GetPathWithNodesAsync(int pathId)`, `GetPathHistoryByStudentIdAsync(int studentId)`, `AddPathAsync(StudentLearningPath)`, `UpdatePathAsync(StudentLearningPath)`, `AddNodesAsync(IEnumerable<LearningPathNode>)`, `GetAllPathsPagedAsync(int page, int pageSize, string? statusFilter)`.
- [x] **Test:** `dotnet build` pass.

### Task 3.2 — Tạo `LearningPathRepository` implementation
- [x] Tạo file `Repositories/LearningPathRepository.cs`.
- [x] Implement tất cả methods từ `ILearningPathRepository`.
- [x] Dùng `AsNoTracking()` cho read-only queries.
- [x] Dùng `Include(p => p.LearningPathNodes)` khi cần nodes.
- [x] Hỗ trợ phân trang cho `GetAllPathsPagedAsync`.
- [x] **Test:** `dotnet build` pass.

### Task 3.3 — Tạo `ILearningPathEngineService` interface
- [x] Tạo file `Services/Interfaces/ILearningPathEngineService.cs`.
- [x] Methods: `BuildInputAsync(int studentId)` → `LearningPathInputDto`, `GenerateInitialPathAsync(int studentId, int competencyAnalysisId)` → `StudentLearningPath`, `GetActivePathAsync(int studentId)` → `StudentLearningPath?`, `GetPathDetailAsync(int pathId, int userId)` → `LearningPathDetailViewModel`, `GetPathSummaryAsync(int studentId)` → `LearningPathSummaryViewModel`, `ArchivePathAsync(int pathId, int userId)`, `RegeneratePathAsync(int studentId, string reason)` → `StudentLearningPath`, `CanGeneratePathAsync(int studentId)` → `LearningPathGenerateViewModel`.
- [x] **Test:** `dotnet build` pass.

### Task 3.4 — Tạo `LearningPathEngineService` — phần BuildInput
- [x] Tạo file `Services/LearningPathEngineService.cs`.
- [x] Inject: `ApplicationDbContext`, `ILearningPathRepository`, `ILogger<LearningPathEngineService>`.
- [x] Implement `BuildInputAsync`: lấy profile từ `student_learning_profiles`, scores từ `competency_skill_scores`, strengths/weaknesses từ `competency_analyses`, topics/lessons/quizzes active.
- [x] Implement `CanGeneratePathAsync`: kiểm tra onboarding, placement test, competency analysis, active path.
- [x] **Test:** `dotnet build` pass.

### Task 3.5 — Tạo `LearningPathEngineService` — phần Generate & Mapping
- [x] Implement `GenerateInitialPathAsync`: kiểm tra điều kiện → build input → gọi AI (mock) → validate output → mapping → lưu DB trong transaction.
- [x] Tạo private method `MapAiOutputToPathAsync(LearningPathOutputDto, int studentId, int competencyAnalysisId)`: tạo `StudentLearningPath` + `LearningPathNode[]` từ AI output.
- [x] Node đầu tiên status = `AVAILABLE`, node sau = `LOCKED`.
- [x] Validate FK: topic/lesson/quiz phải tồn tại và active.
- [x] Dùng transaction khi lưu path + nodes.
- [x] **Test:** `dotnet build` pass.

### Task 3.6 — Tạo `LearningPathEngineService` — phần Query & Archive
- [x] Implement `GetActivePathAsync`, `GetPathDetailAsync`, `GetPathSummaryAsync`.
- [x] Implement `ArchivePathAsync`: đổi status sang `ARCHIVED`, set `archived_at`.
- [x] Implement `RegeneratePathAsync`: archive path cũ → tạo path mới version+1 → set `replaced_by_path_id`.
- [x] Kiểm tra ownership: studentId phải match currentUser.
- [x] **Test:** `dotnet build` pass.

### Task 3.7 — Đăng ký DI trong `Program.cs`
- [x] Thêm `builder.Services.AddScoped<ILearningPathRepository, LearningPathRepository>()`.
- [x] Thêm `builder.Services.AddScoped<ILearningPathEngineService, LearningPathEngineService>()`.
- [x] **Test:** `dotnet build` pass.

---

## Phase 4: AI Prompt & Validation (Task 5 + Task 7)

### Task 4.1 — Tạo `ILearningPathAiService` interface
- [x] Tạo file `Services/Interfaces/ILearningPathAiService.cs`.
- [x] Methods: `GeneratePathFromAiAsync(LearningPathInputDto input)` → `LearningPathOutputDto`, `ValidateAiOutputAsync(LearningPathOutputDto output, LearningPathInputDto input)` → `(bool IsValid, string[] Errors)`.
- [x] **Test:** `dotnet build` pass.

### Task 4.2 — Tạo `LearningPathAiService` implementation (Mock AI)
- [x] Tạo file `Services/LearningPathAiService.cs`.
- [x] Implement `GeneratePathFromAiAsync`: tạo mock `LearningPathOutputDto` dựa trên input (chọn random topics/lessons từ available list, tạo 3 phases, mỗi phase 5-10 nodes).
- [x] Comment rõ vị trí sẽ thay bằng gọi AI thật sau (Gemini API).
- [x] Implement `ValidateAiOutputAsync`: kiểm tra node có ActionType hợp lệ, TopicId/LessonId/QuizId tồn tại, EstimatedMinutes > 0.
- [x] **Test:** `dotnet build` pass.

### Task 4.3 — Tạo `LearningPathMapper` helper
- [x] Tạo file `Helpers/LearningPathMapper.cs`.
- [x] Static method `MapOutputToEntities(LearningPathOutputDto output, int studentId, int competencyAnalysisId)` → `(StudentLearningPath path, List<LearningPathNode> nodes)`.
- [x] Set status path = `ACTIVE`, node đầu = `AVAILABLE`, node sau = `LOCKED`.
- [x] Set `GeneratedByAi = true`, `CreatedAt = DateTime.UtcNow`.
- [x] Assign `OrderIndex` tuần tự.
- [x] **Test:** `dotnet build` pass.

### Task 4.4 — Tạo `LearningPathValidator` helper
- [x] Tạo file `Helpers/LearningPathValidator.cs`.
- [x] Static method `ValidateNodes(List<LearningPathNode> nodes)` → `(bool IsValid, List<string> Errors)`.
- [x] Kiểm tra: không có node trùng OrderIndex, mọi NodeType hợp lệ, path không rỗng.
- [x] **Test:** `dotnet build` pass.

### Task 4.5 — Đăng ký DI `ILearningPathAiService`
- [x] Thêm `builder.Services.AddScoped<ILearningPathAiService, LearningPathAiService>()` trong `Program.cs`.
- [x] **Test:** `dotnet build` pass.

---

## Phase 5: Student Controller & Views (Task 10 + Task 11 + Task 9)

### Task 5.1 — Mở rộng `LearningPathController` phía Student
- [x] Mở file `Areas/Student/Controllers/LearningPathController.cs`.
- [x] Inject thêm `ILearningPathEngineService`.
- [x] Thêm action `[HttpGet] Generate()`: gọi `CanGeneratePathAsync` → trả view xác nhận hoặc redirect nếu thiếu dữ liệu.
- [x] Thêm action `[HttpPost] Generate()`: gọi `GenerateInitialPathAsync` → redirect về `Index` khi thành công, `TempData["ErrorMessage"]` khi lỗi.
- [x] Thêm action `[HttpGet] Detail(int id)`: gọi `GetPathDetailAsync` → trả view chi tiết path.
- [x] Thêm action `[HttpGet] Summary()`: gọi `GetPathSummaryAsync` → trả view tóm tắt.
- [x] Thêm action `[HttpPost] Regenerate()`: gọi `RegeneratePathAsync` → redirect.
- [x] Kiểm tra ownership trong mọi action.
- [x] **Test:** `dotnet build` pass.

### Task 5.2 — Tạo View `Generate.cshtml` (Student)
- [x] Tạo file `Areas/Student/Views/LearningPath/Generate.cshtml`.
- [x] **BẮT BUỘC đọc `DESIGN.md` trước khi code UI.**
- [x] Hiển thị trạng thái: đã có onboarding? placement test? competency analysis?
- [x] Nếu đủ điều kiện: nút "Tạo lộ trình học" (form POST).
- [x] Nếu thiếu: hiển thị bước cần hoàn thành + link redirect.
- [x] Dùng `_StudentLayout.cshtml`.
- [x] **Test:** `dotnet build` pass.

### Task 5.3 — Tạo View `Summary.cshtml` (Student)
- [x] Tạo file `Areas/Student/Views/LearningPath/Summary.cshtml`.
- [x] **BẮT BUỘC đọc `DESIGN.md` trước khi code UI.**
- [x] Hiển thị: tiêu đề path, AI plan summary, tiến độ (completed/total), node hiện tại, node tiếp theo, priority topics.
- [x] Card hiển thị lý do AI đề xuất.
- [x] Nút "Xem chi tiết" link đến Detail.
- [x] Nút "Tạo lại lộ trình" nếu cho phép.
- [x] Dùng `_StudentLayout.cshtml`.
- [x] **Test:** `dotnet build` pass.

### Task 5.4 — Tạo View `Detail.cshtml` (Student)
- [x] Tạo file `Areas/Student/Views/LearningPath/Detail.cshtml`.
- [x] **BẮT BUỘC đọc `DESIGN.md` trước khi code UI.**
- [x] Hiển thị danh sách nodes dạng card/bảng.
- [x] Mỗi node card: title, type (icon), status (color), estimated_minutes, ai_reason, scheduled_date.
- [x] Node LOCKED hiển thị khóa, AVAILABLE hiển thị nút bắt đầu, COMPLETED hiển thị tick.
- [x] Dùng `_StudentLayout.cshtml`.
- [x] **Test:** `dotnet build` pass.

### Task 5.5 — Tạo CSS `learning-path-m8.css`
- [x] Tạo file `wwwroot/css/learning-path-m8.css`.
- [x] **BẮT BUỘC đọc `DESIGN.md` trước khi code CSS.**
- [x] Style cho: `.m8-path-summary`, `.m8-node-card`, `.m8-node-status--locked/available/completed`, `.m8-generate-form`, `.m8-missing-step`.
- [x] Dùng BEM naming, tuân theo color palette trong DESIGN.md.
- [x] Responsive mobile-first.
- [x] **Test:** `dotnet build` pass. CSS không conflict với `learning-path.css` (M9).

### Task 5.6 — Xử lý unlock node logic
- [x] Trong `LearningPathEngineService`, thêm method `UnlockNextNodeAsync(int completedNodeId, int studentId)`.
- [x] Khi node hoàn thành (status = COMPLETED) → tìm node tiếp theo (OrderIndex + 1) → đổi status LOCKED → AVAILABLE.
- [x] Kiểm tra `required_node_id` nếu có (prerequisite).
- [x] **Test:** `dotnet build` pass.

---

## Phase 6: Admin Controller & Views (Task 8 + Task 12)

### Task 6.1 — Mở rộng `Admin/LearningPathsController`
- [x] Mở file `Areas/Admin/Controllers/LearningPathsController.cs` (đã có từ M9).
- [x] Thêm action `[HttpGet] PathHistory(int page, string? status)`: phân trang danh sách tất cả paths.
- [x] Thêm action `[HttpGet] PathDetail(int id)`: xem chi tiết path bất kỳ (admin quyền xem tất cả).
- [x] Thêm action `[HttpGet] GenerationLogs()`: xem log tạo path (join `ai_usage_logs` WHERE `module_code = 'LEARNING_PATH'`).
- [x] **Test:** `dotnet build` pass.

### Task 6.2 — Tạo `Admin/PathTemplatesController`
- [x] Tạo file `Areas/Admin/Controllers/PathTemplatesController.cs`.
- [x] Action: `Index`, `Create (GET/POST)`, `Edit (GET/POST)`, `Details`, `Publish (POST)`, `Archive (POST)`.
- [x] CRUD `learning_path_templates` và `learning_path_template_nodes`.
- [x] Chỉ publish template có ít nhất 1 node.
- [x] Chỉ archive (không xóa) template đã dùng.
- [x] **Test:** `dotnet build` pass.

### Task 6.3 — Tạo ViewModels cho Admin Path
- [x] Tạo folder `ViewModels/Admin/LearningPaths/`.
- [x] Tạo `PathHistoryViewModel.cs`: list paths + pagination.
- [x] Tạo `PathDetailAdminViewModel.cs`: path + nodes + AI log info (ai_model, prompt version, error).
- [x] Tạo `GenerationLogViewModel.cs`: list ai_usage_logs filtered by module LEARNING_PATH.
- [x] Tạo folder `ViewModels/Admin/PathTemplates/`.
- [x] Tạo `PathTemplateViewModel.cs`, `CreatePathTemplateViewModel.cs`, `EditPathTemplateViewModel.cs`.
- [x] **Test:** `dotnet build` pass.

### Task 6.4 — Tạo Admin Views cho Path History
- [x] Tạo `Areas/Admin/Views/LearningPaths/PathHistory.cshtml`: bảng paths, filter status, pagination.
- [x] Tạo `Areas/Admin/Views/LearningPaths/PathDetail.cshtml`: thông tin path + nodes + AI log.
- [x] Tạo `Areas/Admin/Views/LearningPaths/GenerationLogs.cshtml`: bảng logs.
- [x] **BẮT BUỘC đọc `DESIGN.md`.** Dùng `_AdminLayout.cshtml`.
- [x] **Test:** `dotnet build` pass.

### Task 6.5 — Tạo Admin Views cho Path Templates
- [x] Tạo folder `Areas/Admin/Views/PathTemplates/`.
- [x] Tạo `Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `Details.cshtml`.
- [x] **BẮT BUỘC đọc `DESIGN.md`.** Dùng `_AdminLayout.cshtml`.
- [x] **Test:** `dotnet build` pass.

### Task 6.6 — Cập nhật `_AdminLayout.cshtml` menu
- [x] Thêm menu items cho: "Path History", "Path Templates", "Generation Logs" trong sidebar.
- [x] **Test:** `dotnet build` pass.

---

## Phase 7: Error Handling, Compliance & Logging (Task 13 + 14 + 15 + 16 + 17)

### Task 7.1 — Xử lý AI lỗi & fallback template
- [x] Trong `LearningPathEngineService.GenerateInitialPathAsync`: wrap gọi AI bằng try/catch.
- [x] Nếu AI timeout/lỗi → tìm `learning_path_template` PUBLISHED phù hợp (match goal, level) → dùng template tạo path.
- [x] Nếu không có template phù hợp → set path status = `FAILED`, ghi error log.
- [x] Ghi `ai_usage_logs` mọi lần gọi AI (success/fail).
- [x] **Test:** `dotnet build` pass.

### Task 7.2 — Content compliance check
- [x] Tạo file `Services/LearningPathComplianceService.cs` + interface `Services/Interfaces/ILearningPathComplianceService.cs`.
- [x] Method `ValidateContentComplianceAsync(List<LearningPathNode> nodes)` → `(bool IsCompliant, List<string> Violations)`.
- [x] Kiểm tra: topic/lesson/quiz phải status `ACTIVE`/`PUBLISHED`/`APPROVED`. Không dùng reference_only content.
- [x] Gọi compliance check trước khi lưu path.
- [x] **Test:** `dotnet build` pass.

### Task 7.3 — Lưu AI version & reasoning
- [x] Khi tạo path: lưu `ai_plan_summary` vào `StudentLearningPath`.
- [x] Khi tạo nodes: lưu `ai_reason` từ AI output vào từng `LearningPathNode`.
- [x] Ghi `ai_usage_logs`: `module_code = "LEARNING_PATH"`, `prompt_template_id`, `ai_model`, `input_tokens`, `output_tokens`, `request_status`.
- [x] **Test:** `dotnet build` pass.

### Task 7.4 — Regenerate path khi đổi mục tiêu
- [x] Hoàn thiện `RegeneratePathAsync`: archive path cũ (giữ progress history), tạo path mới.
- [x] Giới hạn: không cho regenerate quá 3 lần/ngày (nếu cấu hình).
- [x] Lưu `reason` thay đổi.
- [x] **Test:** `dotnet build` pass.

### Task 7.5 — Phân quyền ownership
- [x] Review tất cả controller actions: Student chỉ xem path của mình.
- [x] Admin xem tất cả. Teacher xem theo quyền được cấp (nếu có).
- [x] Test: Student A truy cập path Student B → 403/404.
- [x] ViewModel Student không chứa raw prompt, API key, technical log.
- [x] **Test:** `dotnet build` pass.

### Task 7.6 — Đăng ký DI cho Compliance Service
- [x] Thêm `builder.Services.AddScoped<ILearningPathComplianceService, LearningPathComplianceService>()` trong `Program.cs`.
- [x] **Test:** `dotnet build` pass.

---

## Phase 8: Testing & Bàn giao (Task 18 + 19 + 20)

### Task 8.1 — Tạo file test `Tests/M8LearningPathEngineServiceTests.cs`
- [x] Test cases: tạo path đủ dữ liệu, thiếu onboarding → lỗi, thiếu placement test → lỗi, thiếu competency → lỗi.
- [x] Test mapping AI output → entities.
- [x] Test unlock next node logic.
- [x] Test archive path.
- [x] Test regenerate path version tăng.
- [x] **Test:** `dotnet build` pass. Tất cả test pass.

### Task 8.2 — Tạo file test `Tests/M8ComplianceTests.cs`
- [x] Test: path chứa topic chưa active → violation.
- [x] Test: path chứa quiz chưa approved → violation.
- [x] Test: path hợp lệ → pass compliance.
- [x] **Test:** `dotnet build` pass. Tất cả test pass.

### Task 8.3 — Tạo file test `Tests/M8ControllerTests.cs`
- [x] Test Student Generate action: đủ điều kiện → success.
- [x] Test Student Detail action: ownership check.
- [x] Test Admin PathHistory: phân trang đúng.
- [x] **Test:** `dotnet build` pass. Tất cả test pass.

### Task 8.4 — Seed data cho M8
- [x] Cập nhật `Data/Seeders/DatabaseSeeder.cs`: thêm seed AI prompt template cho module `LEARNING_PATH`.
- [x] Thêm seed learning path template mẫu (1-2 templates PUBLISHED).
- [x] **Test:** `dotnet build` pass.

### Task 8.5 — Tài liệu bàn giao M8
- [x] Tạo file `m8_handover.md` tại root.
- [x] Ghi rõ: routes, controllers, services, models, viewmodels.
- [x] Ghi schema AI output + mapping DB.
- [x] Ghi danh sách seed data.
- [x] Ghi cách M9/M10/M12/M16 sử dụng output M8.
- [x] **Test:** File hoàn chỉnh, rõ ràng.

### Task 8.6 — Final build & checklist
- [x] Chạy `dotnet build` pass 0 errors 0 warnings.
- [x] Tất cả test pass.
- [x] Không có `Console.WriteLine`, `TODO`, `HACK` trong code.
- [x] Không có file nhạy cảm (connection string production).
- [x] Mọi service có interface và đăng ký DI.
- [x] Mọi controller có `[Authorize]` phù hợp.

---

## Tổng kết files sẽ tạo/sửa

| Loại | File | Trạng thái |
|------|------|-----------|
| Migration SQL | `migration_m8_schema.sql` | NEW (nếu cần) |
| Entity | `Models/StudentLearningPath.cs` | MODIFY (nếu thêm cột) |
| Entity | `Models/LearningPathNode.cs` | MODIFY (nếu thêm cột) |
| DTO | `DTOs/LearningPath/LearningPathInputDto.cs` | NEW |
| DTO | `DTOs/LearningPath/LearningPathOutputDto.cs` | NEW |
| Enum | `Enums/LearningPathStatus.cs` | NEW |
| ViewModel | `ViewModels/LearningPath/M8/*.cs` | NEW |
| ViewModel | `ViewModels/Admin/LearningPaths/*.cs` | NEW |
| ViewModel | `ViewModels/Admin/PathTemplates/*.cs` | NEW |
| Repository | `Repositories/Interfaces/ILearningPathRepository.cs` | NEW |
| Repository | `Repositories/LearningPathRepository.cs` | NEW |
| Service | `Services/Interfaces/ILearningPathEngineService.cs` | NEW |
| Service | `Services/LearningPathEngineService.cs` | NEW |
| Service | `Services/Interfaces/ILearningPathAiService.cs` | NEW |
| Service | `Services/LearningPathAiService.cs` | NEW |
| Service | `Services/Interfaces/ILearningPathComplianceService.cs` | NEW |
| Service | `Services/LearningPathComplianceService.cs` | NEW |
| Helper | `Helpers/LearningPathMapper.cs` | NEW |
| Helper | `Helpers/LearningPathValidator.cs` | NEW |
| Controller | `Areas/Student/Controllers/LearningPathController.cs` | MODIFY |
| Controller | `Areas/Admin/Controllers/LearningPathsController.cs` | MODIFY |
| Controller | `Areas/Admin/Controllers/PathTemplatesController.cs` | NEW |
| View | `Areas/Student/Views/LearningPath/Generate.cshtml` | NEW |
| View | `Areas/Student/Views/LearningPath/Summary.cshtml` | NEW |
| View | `Areas/Student/Views/LearningPath/Detail.cshtml` | NEW |
| View | `Areas/Admin/Views/LearningPaths/PathHistory.cshtml` | NEW |
| View | `Areas/Admin/Views/LearningPaths/PathDetail.cshtml` | NEW |
| View | `Areas/Admin/Views/LearningPaths/GenerationLogs.cshtml` | NEW |
| View | `Areas/Admin/Views/PathTemplates/*.cshtml` | NEW |
| CSS | `wwwroot/css/learning-path-m8.css` | NEW |
| Test | `Tests/M8LearningPathEngineServiceTests.cs` | NEW |
| Test | `Tests/M8ComplianceTests.cs` | NEW |
| Test | `Tests/M8ControllerTests.cs` | NEW |
| Config | `Program.cs` | MODIFY (thêm DI) |
| Config | `Data/ApplicationDbContext.cs` | MODIFY (nếu thêm cột) |
| Seed | `Data/Seeders/DatabaseSeeder.cs` | MODIFY |
| Layout | `Views/Shared/_AdminLayout.cshtml` | MODIFY (thêm menu) |
| Doc | `m8_handover.md` | NEW |

---

## Errors Encountered

| Error | Attempt | Resolution |
|-------|---------|------------|
| Build fail: `EditReferenceSourceViewModel` thiếu `ComplianceEvidenceUrl` khi `ReferenceSourceService` đọc/ghi field này. | Chạy `dotnet build` ở Task 1.1. | Thêm property vào `EditReferenceSourceViewModel`, map từ entity trong `ReferenceSourceService`, bổ sung field trong view Edit; build pass. |

---

> **Cập nhật lần cuối:** 2026-06-29
> **Trạng thái:** Hoàn thành toàn bộ Module M8 đến Task 8.6
