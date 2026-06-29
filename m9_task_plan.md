# 📋 M9 - Learning Path UI (Duolingo-style) — M9 IMPLEMENTATION PLAN

> **Project:** AI Study English — `e:\DuAnTotNghiep`  
> **Module:** M9 — Duolingo-style Learning Path UI  
> **Date:** 2026-06-27  
> **Status:** `planning`

## 🎯 INSTRUCTIONS FOR AI AGENTS
1. **Context:** You are implementing the M9 module (Learning Path UI) for an ASP.NET Core MVC project.
2. **Execution:** Work through the 7 Phases below sequentially. Do not skip phases.
3. **Completion:** When you complete a task, check it off `[x]`.
4. **Validation:** Ensure the project builds successfully (`dotnet build`) after completing each phase before moving to the next.
5. **No Migrations:** The database schema (`LearningPathNode`, `StudentLearningPath`) is already complete and contains all necessary fields. DO NOT create new EF Core migrations. Calculate display properties (icons, colors, labels) in ViewModels based on existing data.

---

## 🔍 GAP ANALYSIS (Task Spec vs Actual Codebase)

### ✅ Already exists in codebase
- **Entity `LearningPathNode`:** [LearningPathNode.cs](Models/LearningPathNode.cs) (Has fields: NodeType, Status, OrderIndex, AiReason, ScheduledDate, EstimatedMinutes, CompletedAt)
- **Entity `StudentLearningPath`:** [StudentLearningPath.cs](Models/StudentLearningPath.cs)
- **Enum `ProgressStatus`:** [ProgressStatus.cs](Enums/ProgressStatus.cs) (LOCKED, AVAILABLE, IN_PROGRESS, COMPLETED, NEED_REVIEW, SKIPPED)
- **Enum `ActivityType`:** [ActivityType.cs](Enums/ActivityType.cs)
- **Dashboard load path logic:** `HomeController.cs` (Student Area) lines 60-120
- **Progress update logic:** `ProgressController.cs` (Student Area)

### ⚠️ IMPORTANT DEVIATIONS FROM ORIGINAL TASK DOC
1. **Status Naming:** The original task asked for `REVIEW_NEEDED`. The codebase actually uses `NEED_REVIEW` (in `ProgressStatus.cs`). **Use `NEED_REVIEW`.**
2. **Current Status:** The original task mentioned a `CURRENT` status. **Do NOT add `CURRENT` to the DB.** Determine the "current" node logically in the Service (e.g., the first `AVAILABLE` or `IN_PROGRESS` node).
3. **UI Properties in DB:** Original task asked to add `ui_icon`, `ui_color`, `short_label` to the database. **Do NOT add these.** Compute them in the ViewModels based on `NodeType` and `Status`.
4. **M10/M13 Integrations:** M10 (Study Plan) and M13 (AI Feedback) modules do not exist yet. Use `ScheduledDate` on the node for "Today's Tasks" and the existing `NEED_REVIEW` status logic.

---

# 🚀 IMPLEMENTATION PLAN — 7 PHASES

## Phase 1: Core Definitions (Enums & ViewModels)
> **Goal:** Establish data contracts before implementing services and controllers.

- [x] **Task 1.1: Create `NodeType` constants**
  - File: `e:\DuAnTotNghiep\Enums\NodeType.cs`
  - Define: `TOPIC`, `LESSON`, `QUIZ`, `PRACTICE`, `REVIEW`, `AI_TUTOR`
  - Add `IsValid(string type)` and `All` array (copy pattern from `ProgressStatus.cs`).
- [x] **Task 1.2: Create `PathNodeViewModel`**
  - File: `e:\DuAnTotNghiep\ViewModels\LearningPath\PathNodeViewModel.cs`
  - Fields: `NodeId`, `Title`, `Description`, `NodeType`, `Status`, `OrderIndex`, `EstimatedMinutes`, `AiReason`, `TargetUrl`, `IsClickable`, `CssClass`, `IconClass`, `StatusLabel`, `CompletedAt`, `ScheduledDate`, `TopicName`.
- [x] **Task 1.3: Create `TodayTaskViewModel`**
  - File: `e:\DuAnTotNghiep\ViewModels\LearningPath\TodayTaskViewModel.cs`
  - Fields: `NodeId`, `Title`, `NodeType`, `AiReason`, `EstimatedMinutes`, `TargetUrl`, `IsOverdue`.
- [x] **Task 1.4: Create `PathProgressSummaryViewModel`**
  - File: `e:\DuAnTotNghiep\ViewModels\LearningPath\PathProgressSummaryViewModel.cs`
  - Fields: `TotalNodes`, `CompletedNodes`, `InProgressNodes`, `ProgressPercent`, `TotalStudyMinutes`, `CurrentStreak`.
- [x] **Task 1.5: Create `LearningPathPageViewModel`**
  - File: `e:\DuAnTotNghiep\ViewModels\LearningPath\LearningPathPageViewModel.cs`
  - Fields: `PathId`, `PathTitle`, `PathDescription`, `PathStatus`, `StartDate`, `TargetEndDate`, `GeneratedByAi`, `AiPlanSummary`, `HasPath` (bool).
  - Collections: `List<PathNodeViewModel> Nodes`, `List<TodayTaskViewModel> TodayTasks`.
  - Nested Object: `PathProgressSummaryViewModel Progress`.

---

## Phase 2: Service Layer (`PathViewService`)
> **Goal:** Centralize logic for mapping DB entities to ViewModels and calculating progress/availability.

- [x] **Task 2.1: Create `IPathViewService` interface**
  - File: `e:\DuAnTotNghiep\Services\Interfaces\IPathViewService.cs`
  - Methods: `GetCurrentPathPageAsync(int userId)`, `EnsurePathOwnerAsync(int pathId, int userId)`, `BuildNodeTargetUrlAsync(LearningPathNode node)`, `CanOpenNodeAsync(int nodeId, int userId)`.
- [x] **Task 2.2: Implement `PathViewService`**
  - File: `e:\DuAnTotNghiep\Services\PathViewService.cs`
  - `GetCurrentPathPageAsync`: Query active path for student, `.Include` nodes and related entities (Topic, Lesson, etc.) ordered by `OrderIndex`. Map to `LearningPathPageViewModel`.
  - `BuildNodeTargetUrlAsync`: Return URL strings based on `NodeType` (use placeholders if target controllers don't exist yet, e.g., `/Student/Lesson/Details/{lessonId}`).
  - `CanOpenNodeAsync`: Return `false` for `LOCKED`. Return `true` for `COMPLETED`, `AVAILABLE`, `IN_PROGRESS`, `NEED_REVIEW`, `SKIPPED`.
- [x] **Task 2.3: Register Dependency Injection**
  - File: `e:\DuAnTotNghiep\Program.cs`
  - Add `builder.Services.AddScoped<IPathViewService, PathViewService>();`

---

## Phase 3: Controller (`LearningPathController` - Student)
> **Goal:** Create the MVC controller to serve the Learning Path page.

- [x] **Task 3.1: Create `LearningPathController`**
  - File: `e:\DuAnTotNghiep\Areas\Student\Controllers\LearningPathController.cs`
  - Attributes: `[Area("Student")]`, `[Authorize(Roles = "STUDENT")]`.
  - Inject `IPathViewService`. Implement `GetUserId()` helper.
- [x] **Task 3.2: Implement `Index` Action (GET)**
  - Call `GetCurrentPathPageAsync(userId)`.
  - Return `View(model)`.
- [x] **Task 3.3: Implement `OpenNode` Action (GET)**
  - Route: `/Student/LearningPath/OpenNode/{nodeId}`
  - Check `CanOpenNodeAsync` and `EnsurePathOwnerAsync`.
  - Call `BuildNodeTargetUrlAsync` and redirect to that URL. Handle null URLs with `TempData` error.

---

## Phase 4: Razor Views & Styling (Duolingo-style)
> **Goal:** Build the interactive, visual path UI.

- [x] **Task 4.1: Create CSS `learning-path.css`**
  - File: `e:\DuAnTotNghiep\wwwroot\css\learning-path.css`
  - Design a mobile-first, vertical zigzag layout for nodes.
  - Styles for states: `.node-locked` (gray), `.node-available` (colored, pulse animation), `.node-completed` (gold/green).
- [x] **Task 4.2: Create Partial `_PathNodeCard.cshtml`**
  - File: `e:\DuAnTotNghiep\Areas\Student\Views\LearningPath\_PathNodeCard.cshtml`
  - Model: `PathNodeViewModel`
  - Display icon, title, estimated minutes, and truncated AI Reason. Wrap in `<a>` tag pointing to `OpenNode` if clickable. Show lock icon if locked.
- [x] **Task 4.3: Create Partial `_PathStageHeader.cshtml`**
  - File: `e:\DuAnTotNghiep\Areas\Student\Views\LearningPath\_PathStageHeader.cshtml`
  - Render stage/phase titles (e.g., "Foundation", "Practice") above node groups.
- [x] **Task 4.4: Create Partial `_PathProgressBar.cshtml`**
  - File: `e:\DuAnTotNghiep\Areas\Student\Views\LearningPath\_PathProgressBar.cshtml`
  - Render progress bar based on `PathProgressSummaryViewModel`.
- [x] **Task 4.5: Create Main View `Index.cshtml`**
  - File: `e:\DuAnTotNghiep\Areas\Student\Views\LearningPath\Index.cshtml`
  - Model: `LearningPathPageViewModel`
  - Link CSS, include progress bar, "Today's Tasks", and render node list grouped by stage using `_PathNodeCard`. Handle empty state if `!Model.HasPath`.

---

## Phase 5: Business Logic (Unlocking & Progression)
> **Goal:** Implement the logic to unlock sequential nodes as previous ones are completed.

- [x] **Task 5.1: Create `TryUnlockNextNodesAsync` method**
  - In `PathViewService`. When node N is completed, find node N+1 and update its status from `LOCKED` to `AVAILABLE`.
- [x] **Task 5.2: Create `MarkNodeCompletedAsync` method**
  - In `PathViewService`. Updates node status to `COMPLETED`, sets `CompletedAt`, logs `StudyActivityLog`, and calls `TryUnlockNextNodesAsync`.
- [x] **Task 5.3: Integrate with `ProgressController`**
  - File: `e:\DuAnTotNghiep\Areas\Student\Controllers\ProgressController.cs`
  - Refactor existing direct node updates to call `IPathViewService.MarkNodeCompletedAsync` to ensure unlock logic triggers.

---

## Phase 6: Admin Preview & Seed Data
> **Goal:** Allow admins to preview student paths and create demo data.

- [x] **Task 6.1: Create Admin `LearningPathsController`**
  - File: `e:\DuAnTotNghiep\Areas\Admin\Controllers\LearningPathsController.cs`
  - Area: Admin. Implement `Preview(int studentId)` using `PathViewService` (bypass ownership check).
- [x] **Task 6.2: Create Admin View `Preview.cshtml`**
  - File: `e:\DuAnTotNghiep\Areas\Admin\Views\LearningPaths\Preview.cshtml`
  - Re-use partials but disable all links (read-only mode).
- [x] **Task 6.3: Create Demo Seed Data**
  - File: `e:\DuAnTotNghiep\Data\Seeders\DatabaseSeeder.cs`
  - Add method `SeedLearningPathDemoAsync()` to insert 1 `StudentLearningPath` and 10 `LearningPathNode`s (with mixed statuses: COMPLETED, AVAILABLE, LOCKED) for `student1@aistudyenglish.com`.

---

## Phase 7: Optimization & QA
> **Goal:** Final polish and testing.

- [x] **Task 7.1: Query Optimization**
  - Review `PathViewService` to ensure `.Include()` is used correctly. Prevent N+1 query problems.
- [x] **Task 7.2: Error Handling & Resilience**
  - Prevent application crashes if a node references a deleted Topic/Lesson.
- [x] **Task 7.3: Activity Logging**
  - Implement basic `ILogger` calls when students open or complete nodes.
- [x] **Task 7.4: E2E Verification**
  - Build project. Log in as `student1`. View path. Click available node. Click locked node (should be prevented). Check Admin preview.
