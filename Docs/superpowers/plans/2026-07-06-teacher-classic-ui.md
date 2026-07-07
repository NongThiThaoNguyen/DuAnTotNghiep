# Teacher Classic UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Đồng bộ toàn bộ khu vực Teacher với giao diện classic academic đã áp dụng cho Admin và Student, đồng thời sửa shell responsive và tách CSS/JavaScript khỏi layout để dễ bảo trì.

**Architecture:** Giữ nguyên ASP.NET Core MVC, controller, model, URL, Bootstrap, Font Awesome, TinyMCE và nghiệp vụ hiện có. `_TeacherLayout.cshtml` chỉ còn nhiệm vụ ghép các partial; `role-shell.css` cung cấp token chung, `teacher-shell.css` cung cấp component và responsive riêng cho Teacher, còn `teacher-shell.js` và `teacher-modal.js` quản lý drawer, toast và modal bằng hook tường minh thay vì quét/sửa DOM theo URL.

**Tech Stack:** ASP.NET Core MVC/Razor, Bootstrap 5.3, Tailwind utility CSS hiện có, Font Awesome 6.4, TinyMCE 6.8, vanilla JavaScript/jQuery cho luồng AJAX hiện hữu, Node test runner, Chrome/in-app browser cho visual QA.

---

## Design Direction

- Teacher là không gian soạn, duyệt và vận hành lớp học: mật độ vừa phải, form rõ ràng, bảng dễ quét, ưu tiên hành động chính.
- Dùng cùng Noto Sans, canvas ivory, surface ấm, charcoal ink, stone border và brass focus/active với Admin và Student.
- Không dùng tím làm nhận diện, gradient, glow, glassmorphism, nút pill quá mức, animation trang trí hoặc emoji trong tiêu đề nghiệp vụ.
- Sidebar desktop cố định và cuộn độc lập. Mobile dùng topbar + drawer + overlay; không biến toàn bộ sidebar thành hàng icon ngang.
- Mọi trang dài cuộn trong vùng `main`, không làm mất sidebar và không tràn ngang ở 390px.

## Current Audit Baseline

- `_TeacherLayout.cshtml` dài 1.207 dòng, chứa hơn 780 dòng CSS nhúng và gần 300 dòng JavaScript/modal/toast.
- Teacher vẫn nạp `layout.css` và `responsive.css`, hai file cũng đang được Student `_AILearnLayout` sử dụng. Không sửa hai file này trong đợt Teacher để tránh hồi quy Student.
- Font hiện tại là Be Vietnam Pro/Poppins; màu nhận diện là `#6C63FF`; layout dùng gradient, glow, glass toast và nhiều `!important`.
- Desktop `/Teacher` và `/Teacher/Courses` hoạt động nhưng khác hệ Admin/Student.
- Mobile 390px đang có `document.scrollWidth = 459px`; sidebar rộng 375px, chuyển thành dải icon ngang, không có nút drawer rõ ràng.
- Các view đang có `<style>`/`style=` cần gom lại: Home, Courses, Students, Resources, Messages, Schedule, Reports và Profile.

---

### Task 1: Lock the Teacher shell contract with failing tests

**Files:**
- Create: `Tests/Ui/TeacherShell.Tests.cjs`
- Test: `Views/Shared/_TeacherLayout.cshtml`
- Test: `Views/Shared/_TeacherSidebar.cshtml`
- Test: `Views/Shared/_TeacherAccountStrip.cshtml`
- Test: `wwwroot/css/teacher-shell.css`
- Test: `wwwroot/js/teacher-shell.js`
- Test: `wwwroot/js/teacher-modal.js`

- [ ] **Step 1: Add the shell, palette, and maintainability tests**

Create tests that assert the intended contract before implementation:

```javascript
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");
const read = relativePath => fs.readFileSync(path.join(root, relativePath), "utf8");

test("Teacher uses the shared classic role shell", () => {
  const layout = read("Views/Shared/_TeacherLayout.cshtml");

  assert.match(layout, /role-shell teacher-shell/);
  assert.match(layout, /css\/role-shell\.css/);
  assert.match(layout, /css\/teacher-shell\.css/);
  assert.match(layout, /_TeacherSidebar/);
  assert.match(layout, /_TeacherAccountStrip/);
  assert.match(layout, /class="rs-skip-link"/);
  assert.match(layout, /id="mainContent"/);
  assert.match(layout, /Noto\+Sans/);
  assert.doesNotMatch(layout, /layout\.css|responsive\.css|Be\+Vietnam\+Pro|Poppins/);
  assert.doesNotMatch(layout, /<style[\s>]/i);
});

test("Teacher shell has a real mobile drawer and bounded main scroll", () => {
  const css = read("wwwroot/css/teacher-shell.css");

  assert.match(css, /\.teacher-shell\s*{[\s\S]*height:\s*100dvh/);
  assert.match(css, /\.teacher-shell-frame\s*{[\s\S]*overflow:\s*hidden/);
  assert.match(css, /\.teacher-content\s*{[\s\S]*overflow-y:\s*auto/);
  assert.match(css, /#teacherSidebar\.teacher-sidebar-closed/);
  assert.match(css, /\.teacher-sidebar-overlay/);
  assert.match(css, /@media \(min-width:\s*1024px\)/);
  assert.match(css, /var\(--rs-z-sidebar\)/);
  assert.match(css, /var\(--rs-z-overlay\)/);
});

test("Teacher styling removes the legacy purple SaaS identity", () => {
  const sources = [
    read("Views/Shared/_TeacherLayout.cshtml"),
    read("wwwroot/css/teacher-shell.css")
  ];

  for (const source of sources) {
    assert.doesNotMatch(source, /#6C63FF|#5A52E6|linear-gradient|backdrop-filter/i);
  }
});

test("Teacher behavior uses explicit hooks instead of global DOM rewriting", () => {
  const layout = read("Views/Shared/_TeacherLayout.cshtml");
  const modal = read("wwwroot/js/teacher-modal.js");

  assert.doesNotMatch(layout, /polishButtons|href\*="\/Edit\/"|href\*="\/Delete\/"/);
  assert.match(modal, /data-teacher-modal/);
  assert.doesNotMatch(modal, /window\.alert|\balert\s*\(/);
});
```

- [ ] **Step 2: Run the tests and confirm the old implementation fails**

Run:

```powershell
node --test Tests\Ui\TeacherShell.Tests.cjs
```

Expected: FAIL because the current Teacher layout has no role shell, no shared partials, no mobile drawer contract, and still contains purple inline styling.

- [ ] **Step 3: Commit the contract test**

```powershell
git add Tests/Ui/TeacherShell.Tests.cjs
git commit -m "test: define teacher shell contract"
```

---

### Task 2: Extract the Teacher layout into focused shared partials

**Files:**
- Create: `Views/Shared/_TeacherSidebar.cshtml`
- Create: `Views/Shared/_TeacherAccountStrip.cshtml`
- Create: `Views/Shared/_TeacherModal.cshtml`
- Modify: `Views/Shared/_TeacherLayout.cshtml`
- Modify: `DESIGN.md`

- [ ] **Step 1: Build the Teacher sidebar partial**

Move route detection and navigation out of the layout. Use tag helpers and exact controller-based active state:

```razor
@{
    var controller = ViewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
    string NavClass(string target) =>
        "role-nav-link teacher-nav-link" +
        (string.Equals(controller, target, StringComparison.OrdinalIgnoreCase) ? " is-active" : string.Empty);
}

<aside id="teacherSidebar"
       class="role-sidebar teacher-sidebar teacher-sidebar-closed"
       aria-label="Điều hướng giảng viên">
    <a asp-area="Teacher" asp-controller="Home" asp-action="Index" class="teacher-brand">
        <span class="teacher-brand-mark" aria-hidden="true">D</span>
        <span>AI Study English</span>
    </a>

    <nav class="teacher-nav" aria-label="Chức năng giảng viên">
        <a class="@NavClass("Home")" asp-area="Teacher" asp-controller="Home" asp-action="Index">Tổng quan</a>
        <a class="@NavClass("Courses")" asp-area="Teacher" asp-controller="Courses" asp-action="Index">Khóa học giảng dạy</a>
        <a class="@NavClass("Students")" asp-area="Teacher" asp-controller="Students" asp-action="Index">Quản lý học viên</a>
        <a class="@NavClass("Grades")" asp-area="Teacher" asp-controller="Grades" asp-action="Index">Quản lý điểm số</a>
        <a class="@NavClass("Attendance")" asp-area="Teacher" asp-controller="Attendance" asp-action="Index">Điểm danh</a>
        <a class="@NavClass("Resources")" asp-area="Teacher" asp-controller="Resources" asp-action="Index">Tài liệu học tập</a>
        <a class="@NavClass("Messages")" asp-area="Teacher" asp-controller="Messages" asp-action="Index">Tin nhắn</a>
        <a class="@NavClass("Schedule")" asp-area="Teacher" asp-controller="Schedule" asp-action="Index">Lịch dạy</a>
        <a class="@NavClass("Reports")" asp-area="Teacher" asp-controller="Reports" asp-action="Index">Báo cáo tổng hợp</a>
        <a class="@NavClass("Profile")" asp-area="Teacher" asp-controller="Profile" asp-action="Index">Hồ sơ cá nhân</a>
        <a class="@NavClass("Settings")" asp-area="Teacher" asp-controller="Settings" asp-action="Index">Cài đặt</a>
    </nav>
</aside>
```

Keep the current Font Awesome icons, but render them through one consistent icon slot and preserve visible text labels.

- [ ] **Step 2: Build the account strip and modal partials**

`_TeacherAccountStrip.cshtml` owns the notification button, avatar fallback, teacher name, profile link and logout action. `_TeacherModal.cshtml` owns only Bootstrap modal markup and exposes `#teacherModalTitle`, `#teacherModalBody`, and an `aria-live` loading state.

- [ ] **Step 3: Reduce `_TeacherLayout.cshtml` to composition**

The resulting layout should follow this structure:

```razor
<body class="role-shell teacher-shell">
    <a class="rs-skip-link" href="#mainContent">Bỏ qua đến nội dung chính</a>
    <header class="teacher-mobile-topbar">
        <button id="teacherSidebarToggle" type="button"
                aria-controls="teacherSidebar" aria-expanded="false"
                aria-label="Mở menu giảng viên">
            <i class="fa-solid fa-bars" aria-hidden="true"></i>
        </button>
        <span>AI Study English</span>
    </header>
    <div id="teacherSidebarOverlay" class="teacher-sidebar-overlay" hidden></div>
    <div class="teacher-shell-frame">
        <partial name="_TeacherSidebar" />
        <main id="mainContent" class="teacher-content" role="main">
            <partial name="_TeacherAccountStrip" />
            <div class="teacher-page">@RenderBody()</div>
        </main>
    </div>
    <partial name="_TeacherModal" />
</body>
```

Load CSS in this order: Bootstrap, `site.css`, `role-shell.css`, `teacher-shell.css`. Load JavaScript in this order: jQuery, Bootstrap bundle, validation, `teacher-shell.js`, `teacher-modal.js`. Preserve `RenderSectionAsync("Styles")`, `RenderSectionAsync("Scripts")`, TinyMCE and `_ToastNotifications`.

- [ ] **Step 4: Add the Teacher contract to `DESIGN.md`**

Add a `Teacher Role Shell` section specifying the atelier density, Noto Sans, shared role tokens, 44px mobile targets, explicit modal hooks, scroll ownership and prohibition on purple/gradient/glass styling.

- [ ] **Step 5: Run the shell test**

```powershell
node --test Tests\Ui\TeacherShell.Tests.cjs
```

Expected: layout/partial assertions pass; CSS and JavaScript assertions still fail until Tasks 3 and 4.

- [ ] **Step 6: Commit the layout extraction**

```powershell
git add Views/Shared/_TeacherLayout.cshtml Views/Shared/_TeacherSidebar.cshtml Views/Shared/_TeacherAccountStrip.cshtml Views/Shared/_TeacherModal.cshtml DESIGN.md
git commit -m "refactor: extract teacher role layout"
```

---

### Task 3: Build the tokenized Teacher shell and responsive system

**Files:**
- Create: `wwwroot/css/teacher-shell.css`
- Modify: `Views/Shared/_TeacherLayout.cshtml`
- Test: `Tests/Ui/TeacherShell.Tests.cjs`

- [ ] **Step 1: Implement frame, sidebar, content and mobile drawer primitives**

Start `teacher-shell.css` with shared tokens and bounded scroll ownership:

```css
.teacher-shell {
  height: 100dvh;
  overflow: hidden;
  background: var(--rs-canvas);
  color: var(--rs-ink);
  font-family: var(--rs-font-sans);
}

.teacher-shell-frame {
  display: grid;
  grid-template-columns: 17.25rem minmax(0, 1fr);
  height: 100%;
  overflow: hidden;
}

.teacher-sidebar {
  z-index: var(--rs-z-sidebar);
  min-width: 0;
  overflow-y: auto;
  border-right: 1px solid var(--rs-line);
  background: var(--rs-canvas);
}

.teacher-content {
  min-width: 0;
  height: 100%;
  overflow-x: hidden;
  overflow-y: auto;
  overscroll-behavior: contain;
}

.teacher-page {
  width: min(100%, var(--rs-content-max));
  margin-inline: auto;
  padding: var(--rs-space-page-y) var(--rs-space-page-x);
}

.teacher-sidebar-overlay,
.teacher-mobile-topbar {
  display: none;
}

@media (max-width: 1023px) {
  .teacher-shell-frame { display: block; height: calc(100% - 3.5rem); }
  .teacher-mobile-topbar { display: flex; height: 3.5rem; }
  .teacher-sidebar { position: fixed; inset: 0 auto 0 0; width: min(18rem, 86vw); }
  #teacherSidebar.teacher-sidebar-closed { transform: translateX(-100%); }
  .teacher-sidebar-overlay { position: fixed; inset: 0; z-index: var(--rs-z-overlay); display: block; }
  .teacher-page { padding: 1rem; }
}
```

- [ ] **Step 2: Define reusable Teacher components**

Add focused classes for `.teacher-page-header`, `.teacher-panel`, `.teacher-stat-grid`, `.teacher-stat`, `.teacher-toolbar`, `.teacher-table-wrap`, `.teacher-empty-state`, `.teacher-form-grid`, `.teacher-action-group`, `.teacher-icon-button`, `.teacher-status`, `.teacher-toast` and `.teacher-modal-panel`.

Use 8px controls, 12px panels, 44px minimum mobile hit areas, no decorative card nesting, tabular numerals for metrics, flat borders, and role-shell status tokens.

- [ ] **Step 3: Add a compatibility layer for existing Bootstrap/Tailwind view classes**

Scope compatibility selectors under `.teacher-shell .teacher-content` so legacy classes remain functional during page migration:

```css
.teacher-shell .teacher-content .btn-primary,
.teacher-shell .teacher-content .btn-primary-ailearn {
  border: 1px solid var(--rs-ink) !important;
  border-radius: var(--rs-radius-control) !important;
  background: var(--rs-ink) !important;
  color: var(--rs-surface) !important;
  box-shadow: none !important;
}

.teacher-shell .teacher-content :is(.form-control, .form-select, input, select, textarea):focus {
  border-color: var(--rs-brass) !important;
  box-shadow: var(--rs-focus-ring) !important;
  outline: none;
}

.teacher-shell .teacher-content :is(.rounded-xl, .rounded-2xl, .card-ailearn) {
  border-radius: var(--rs-radius-panel) !important;
  box-shadow: none !important;
}
```

Do not copy the old global `!important` color remapping. Compatibility rules must stay inside `.teacher-shell`.

- [ ] **Step 4: Add reduced-motion and overflow protection**

```css
.teacher-shell img,
.teacher-shell table,
.teacher-shell iframe { max-width: 100%; }

.teacher-table-wrap { overflow-x: auto; }

@media (prefers-reduced-motion: reduce) {
  .teacher-shell *,
  .teacher-shell *::before,
  .teacher-shell *::after {
    scroll-behavior: auto !important;
    transition-duration: 0.01ms !important;
    animation-duration: 0.01ms !important;
  }
}
```

- [ ] **Step 5: Run static tests and inspect at 1440px and 390px**

```powershell
node --test Tests\Ui\TeacherShell.Tests.cjs
```

Expected: shell, palette, scroll and responsive CSS tests pass. Browser checks must report `document.documentElement.scrollWidth === innerWidth` at 390px.

- [ ] **Step 6: Commit the Teacher visual system**

```powershell
git add wwwroot/css/teacher-shell.css Views/Shared/_TeacherLayout.cshtml Tests/Ui/TeacherShell.Tests.cjs
git commit -m "feat: add classic teacher shell styles"
```

---

### Task 4: Replace global DOM rewriting with explicit Teacher interactions

**Files:**
- Create: `wwwroot/js/teacher-shell.js`
- Create: `wwwroot/js/teacher-modal.js`
- Modify: `Views/Shared/_TeacherLayout.cshtml`
- Modify: Teacher create/edit/delete/grade links listed in Tasks 6 and 7
- Test: `Tests/Ui/TeacherShell.Tests.cjs`

- [ ] **Step 1: Implement the mobile drawer and sidebar state**

`teacher-shell.js` binds only IDs owned by the layout, synchronizes `aria-expanded`, closes on overlay/Escape/navigation, and restores body scrolling:

```javascript
(function () {
  "use strict";

  const sidebar = document.getElementById("teacherSidebar");
  const toggle = document.getElementById("teacherSidebarToggle");
  const overlay = document.getElementById("teacherSidebarOverlay");
  if (!sidebar || !toggle || !overlay) return;

  function setOpen(open) {
    sidebar.classList.toggle("teacher-sidebar-closed", !open);
    overlay.hidden = !open;
    toggle.setAttribute("aria-expanded", String(open));
    document.body.classList.toggle("teacher-drawer-open", open);
  }

  toggle.addEventListener("click", () => setOpen(toggle.getAttribute("aria-expanded") !== "true"));
  overlay.addEventListener("click", () => setOpen(false));
  document.addEventListener("keydown", event => {
    if (event.key === "Escape") setOpen(false);
  });
  window.addEventListener("resize", () => {
    if (window.innerWidth >= 1024) setOpen(true);
    else setOpen(false);
  });
})();
```

- [ ] **Step 2: Implement explicit AJAX modal hooks**

`teacher-modal.js` listens only to `[data-teacher-modal]`, reads `data-modal-title` and `data-modal-size`, loads the target, parses unobtrusive validation, posts the form with `FormData`, and shows an inline error region on failure. It must never call `alert()` or globally replace link classes/HTML.

Use this markup on eligible actions:

```razor
<a asp-action="Edit" asp-route-id="@item.Id"
   class="teacher-icon-button"
   data-teacher-modal
   data-modal-title="Chỉnh sửa"
   aria-label="Chỉnh sửa">
    <i class="fa-solid fa-pen" aria-hidden="true"></i>
</a>
```

- [ ] **Step 3: Make toast output semantic and safe**

Render toast text through `textContent`, use `role="status"` for success/info and `role="alert"` for warning/error, and use `var(--rs-z-modal)` instead of `99999`.

- [ ] **Step 4: Run behavior tests**

```powershell
node --test Tests\Ui\TeacherShell.Tests.cjs
```

Expected: no `polishButtons`, URL-substring mutation, `window.alert`, or giant inline script remains in `_TeacherLayout.cshtml`.

- [ ] **Step 5: Commit the interaction layer**

```powershell
git add wwwroot/js/teacher-shell.js wwwroot/js/teacher-modal.js Views/Shared/_TeacherLayout.cshtml Tests/Ui/TeacherShell.Tests.cjs
git commit -m "refactor: isolate teacher shell interactions"
```

---

### Task 5: Rebuild the Teacher dashboard as the reference page

**Files:**
- Modify: `Areas/Teacher/Views/Home/Index.cshtml`
- Modify: `wwwroot/css/teacher-shell.css`
- Test: `Tests/Ui/TeacherShell.Tests.cjs`

- [ ] **Step 1: Remove the page-level style block and decorative greeting**

Use `.teacher-page-header`, a quiet identity strip, a four-column `.teacher-stat-grid`, and explicit text labels. Remove purple avatar rings, emoji, broad shadows and mixed blue/purple accents.

- [ ] **Step 2: Normalize dashboard information hierarchy**

Order content as: page title, today summary, key metrics, pending grading, today schedule, recent quiz results and activity. Empty states should state what is empty and provide one relevant action where possible.

- [ ] **Step 3: Make the dashboard responsive**

Use 4/2/1 metric columns at desktop/tablet/mobile. Stack schedule and grading sections at widths below 1024px. Ensure no fixed child width can exceed the main content area.

- [ ] **Step 4: Add a dashboard regression assertion**

```javascript
test("Teacher dashboard uses calm shared components", () => {
  const view = read("Areas/Teacher/Views/Home/Index.cshtml");
  assert.match(view, /teacher-page-header/);
  assert.match(view, /teacher-stat-grid/);
  assert.doesNotMatch(view, /<style[\s>]|👋|#6C63FF|gradient/i);
});
```

- [ ] **Step 5: Verify dashboard in browser**

Check `/Teacher` at 1440x900, 1024x768 and 390x844. Verify sidebar active state, main scrolling, visible focus, no horizontal overflow and readable Vietnamese line wrapping.

- [ ] **Step 6: Commit the reference dashboard**

```powershell
git add Areas/Teacher/Views/Home/Index.cshtml wwwroot/css/teacher-shell.css Tests/Ui/TeacherShell.Tests.cjs
git commit -m "feat: align teacher dashboard with role shell"
```

---

### Task 6: Standardize courses, lessons and resources

**Files:**
- Modify: `Areas/Teacher/Views/Courses/Index.cshtml`
- Modify: `Areas/Teacher/Views/Courses/Create.cshtml`
- Modify: `Areas/Teacher/Views/Courses/Edit.cshtml`
- Modify: `Areas/Teacher/Views/Courses/Details.cshtml`
- Modify: `Areas/Teacher/Views/Courses/Delete.cshtml`
- Modify: `Areas/Teacher/Views/Lessons/Index.cshtml`
- Modify: `Areas/Teacher/Views/Lessons/Create.cshtml`
- Modify: `Areas/Teacher/Views/Lessons/Edit.cshtml`
- Modify: `Areas/Teacher/Views/Lessons/Detail.cshtml`
- Modify: `Areas/Teacher/Views/Lessons/Details.cshtml`
- Modify: `Areas/Teacher/Views/Resources/Index.cshtml`
- Modify: `Areas/Teacher/Views/Resources/Create.cshtml`
- Modify: `Areas/Teacher/Views/Resources/Edit.cshtml`
- Modify: `Areas/Teacher/Views/Resources/Details.cshtml`
- Modify: `Areas/Teacher/Views/Resources/Delete.cshtml`
- Modify: `wwwroot/css/teacher-shell.css`
- Test: `Tests/Ui/TeacherShell.Tests.cjs`

- [ ] **Step 1: Normalize list-page headers and filters**

Use one `.teacher-page-header`, one `.teacher-toolbar`, labeled filters and a single primary create action. Replace course gradient thumbnails with flat skill/level metadata surfaces and use semantic status classes.

- [ ] **Step 2: Normalize item layouts**

Use compact course/resource rows or restrained cards with title, metadata, status and one `.teacher-action-group`. Keep action controls at stable dimensions; every icon-only action gets `aria-label` and `title`.

- [ ] **Step 3: Normalize create/edit forms**

Use `.teacher-panel teacher-form`, `.teacher-form-grid`, visible labels, inline validation and bottom-aligned actions. TinyMCE must remain full width, keyboard reachable and not clipped inside the modal.

- [ ] **Step 4: Normalize detail/delete states**

Details use definition-list metadata and a clear return action. Delete uses a compact danger confirmation panel with the object name and cancel action; no pulse animation.

- [ ] **Step 5: Add explicit modal hooks**

Add `data-teacher-modal` only to Create/Edit/Delete actions that are already compatible with AJAX. Keep Details and full workflow pages as normal navigation.

- [ ] **Step 6: Add static migration coverage**

Extend `TeacherShell.Tests.cjs` to read all 15 files above and assert no `<style>`, purple hex, gradient class, decorative emoji or `style="font-family` remains.

- [ ] **Step 7: Verify representative routes**

Check `/Teacher/Courses`, `/Teacher/Courses/Create`, a course Details page, `/Teacher/Lessons`, and `/Teacher/Resources`. Exercise one modal cancel and one validation failure without saving data.

- [ ] **Step 8: Commit content management pages**

```powershell
git add Areas/Teacher/Views/Courses Areas/Teacher/Views/Lessons Areas/Teacher/Views/Resources wwwroot/css/teacher-shell.css Tests/Ui/TeacherShell.Tests.cjs
git commit -m "feat: standardize teacher content workspace"
```

---

### Task 7: Standardize assignments, quizzes and grading

**Files:**
- Modify: `Areas/Teacher/Views/Assignments/Index.cshtml`
- Modify: `Areas/Teacher/Views/Assignments/Create.cshtml`
- Modify: `Areas/Teacher/Views/Assignments/Edit.cshtml`
- Modify: `Areas/Teacher/Views/Assignments/Delete.cshtml`
- Modify: `Areas/Teacher/Views/Assignments/Submissions.cshtml`
- Modify: `Areas/Teacher/Views/Assignments/Grade.cshtml`
- Modify: `Areas/Teacher/Views/Quizzes/Index.cshtml`
- Modify: `Areas/Teacher/Views/Quizzes/Create.cshtml`
- Modify: `Areas/Teacher/Views/Quizzes/Edit.cshtml`
- Modify: `Areas/Teacher/Views/Quizzes/ManageQuestions.cshtml`
- Modify: `Areas/Teacher/Views/Quizzes/QuestionBank.cshtml`
- Modify: `Areas/Teacher/Views/Quizzes/Results.cshtml`
- Modify: `Areas/Teacher/Views/Grades/Index.cshtml`
- Modify: `Areas/Teacher/Views/Grades/Pending.cshtml`
- Modify: `Areas/Teacher/Views/Grades/Grade.cshtml`
- Modify: `wwwroot/css/teacher-shell.css`
- Test: `Tests/Ui/TeacherShell.Tests.cjs`

- [ ] **Step 1: Align assessment lists and status vocabulary**

Use the same toolbar, table wrapper, row actions and text-backed statuses for draft, active, pending, submitted, graded, passed and failed. Numeric scores use tabular figures.

- [ ] **Step 2: Align assessment forms**

Keep question/answer interactions intact. Group quiz settings separately from question content and keep save/cancel actions stable when validation messages appear.

- [ ] **Step 3: Align grading screens**

Place submission content and evidence first, AI feedback as secondary reference, and teacher score/feedback as the decisive form. Do not use color alone to distinguish AI and teacher feedback.

- [ ] **Step 4: Protect complex question management on mobile**

Use horizontal table scrolling only inside `.teacher-table-wrap`; keep page actions visible; stack grading columns below 1024px; avoid fixed widths in answer editors.

- [ ] **Step 5: Add modal hooks only where appropriate**

Create/Edit/Delete and single-submission Grade may use `data-teacher-modal`. ManageQuestions, QuestionBank, Results and Submissions remain full pages.

- [ ] **Step 6: Extend static coverage and verify workflows**

Assert the 15 views have no page-level styles or purple/gradient identity. In browser, verify assignment list, grade form, quiz list, question management and results at desktop/mobile without submitting permanent changes.

- [ ] **Step 7: Commit assessment pages**

```powershell
git add Areas/Teacher/Views/Assignments Areas/Teacher/Views/Quizzes Areas/Teacher/Views/Grades wwwroot/css/teacher-shell.css Tests/Ui/TeacherShell.Tests.cjs
git commit -m "feat: align teacher assessment workflows"
```

---

### Task 8: Standardize classroom operations and account pages

**Files:**
- Modify: `Areas/Teacher/Views/Students/Index.cshtml`
- Modify: `Areas/Teacher/Views/Students/Details.cshtml`
- Modify: `Areas/Teacher/Views/Attendance/Index.cshtml`
- Modify: `Areas/Teacher/Views/Attendance/History.cshtml`
- Modify: `Areas/Teacher/Views/Schedule/Index.cshtml`
- Modify: `Areas/Teacher/Views/Schedule/Create.cshtml`
- Modify: `Areas/Teacher/Views/Schedule/Edit.cshtml`
- Modify: `Areas/Teacher/Views/Schedule/Delete.cshtml`
- Modify: `Areas/Teacher/Views/Messages/Index.cshtml`
- Modify: `Areas/Teacher/Views/Reports/Index.cshtml`
- Modify: `Areas/Teacher/Views/Profile/Index.cshtml`
- Modify: `Areas/Teacher/Views/Settings/Index.cshtml`
- Modify: `wwwroot/css/teacher-shell.css`
- Test: `Tests/Ui/TeacherShell.Tests.cjs`

- [ ] **Step 1: Align students and attendance**

Use scan-friendly student rows, compact progress/status metadata and a clear Details action. Attendance controls must keep date/topic/student context visible and preserve labels when stacked on mobile.

- [ ] **Step 2: Align schedule**

Use one restrained calendar/list surface, shared status colors and explicit create/edit/delete hooks. At 390px, switch to a list representation instead of compressing desktop columns.

- [ ] **Step 3: Align messages**

Use a two-pane layout on desktop and single-pane drill-in on mobile. Conversation list, unread state, composer, empty state and message timestamp must use role tokens and avoid nested cards.

- [ ] **Step 4: Align reports, profile and settings**

Reports use flat summary metrics and responsive table/chart containers. Profile uses a readable identity header plus grouped fields. Settings uses native controls, segmented modes only where multiple choices exist, and clear save feedback.

- [ ] **Step 5: Remove remaining page-level style blocks**

Extend the test to enumerate every `Areas/Teacher/Views/**/*.cshtml` file and reject `<style>` blocks, `#6C63FF`, `#5A52E6`, `linear-gradient`, `backdrop-filter`, Be Vietnam Pro and Poppins.

- [ ] **Step 6: Verify classroom and account routes**

Check Students Index/Details, Attendance Index/History, Schedule Index, Messages, Reports, Profile and Settings at desktop and 390px. Confirm active sidebar state follows the controller on every route.

- [ ] **Step 7: Commit operational pages**

```powershell
git add Areas/Teacher/Views/Students Areas/Teacher/Views/Attendance Areas/Teacher/Views/Schedule Areas/Teacher/Views/Messages Areas/Teacher/Views/Reports Areas/Teacher/Views/Profile Areas/Teacher/Views/Settings wwwroot/css/teacher-shell.css Tests/Ui/TeacherShell.Tests.cjs
git commit -m "feat: align teacher classroom operations"
```

---

### Task 9: Full regression, accessibility and browser handoff

**Files:**
- Modify: `Tests/Ui/TeacherShell.Tests.cjs`
- Modify: `DESIGN.md` only if implementation reveals a missing shared rule

- [ ] **Step 1: Run all UI contract tests**

```powershell
node --test Tests\Ui\TeacherShell.Tests.cjs Tests\Ui\StudentShell.Tests.cjs Tests\Ui\AdminSidebarState.Tests.cjs
```

Expected: all tests pass. Teacher changes must not regress Admin/Student contracts.

- [ ] **Step 2: Run design lint and application build**

```powershell
npm run design:lint
dotnet build DuAnTotNghiep.csproj -p:UseAppHost=false -o $env:TEMP\duantotnghiep-teacher-ui-build
```

Expected: design lint has no errors; build completes with 0 errors.

- [ ] **Step 3: Run the web application**

Use port 5117 when available:

```powershell
dotnet run --project DuAnTotNghiep.csproj --urls http://127.0.0.1:5117
```

If 5117 is occupied by the correct application, reuse it. If occupied by another process, choose the next available local port and report the URL.

- [ ] **Step 4: Execute the Teacher route matrix in Chrome**

Sign in with the Teacher seed account and check:

```text
/Teacher
/Teacher/Courses
/Teacher/Students
/Teacher/Grades
/Teacher/Attendance
/Teacher/Resources
/Teacher/Messages
/Teacher/Schedule
/Teacher/Reports
/Teacher/Profile
/Teacher/Settings
```

Also check representative Create/Edit/Delete/Details, assignment grading, quiz question management and modal validation flows.

- [ ] **Step 5: Check viewport and accessibility invariants**

At 1440x900, 1024x768, 768x1024 and 390x844 verify:

- no horizontal document overflow;
- sidebar remains visible on desktop and works as a drawer on mobile;
- long content scrolls inside `#mainContent`;
- Noto Sans renders Vietnamese consistently;
- focus rings are visible and logical;
- Escape closes drawer/modal;
- icon-only buttons have accessible names;
- modal focus, labels, validation, TinyMCE and cancel actions work;
- no console errors occur while navigating.

- [ ] **Step 6: Leave the browser ready for user review**

Leave Chrome/in-app browser on `/Teacher` at the normal desktop viewport and report the running URL plus Student, Teacher and Admin seed credentials.

- [ ] **Step 7: Commit final verification adjustments**

```powershell
git add Tests/Ui/TeacherShell.Tests.cjs DESIGN.md
git commit -m "test: verify teacher role UI"
```

---

## Acceptance Criteria

- Teacher, Admin and Student clearly belong to one product and use Noto Sans with the same role-shell palette.
- Teacher no longer depends on purple/glow/gradient/glass styling for identity.
- `_TeacherLayout.cshtml` is a small composition layout; shared navigation, account strip, modal, CSS and JavaScript live in focused files.
- No Teacher view contains a page-level `<style>` block.
- Desktop sidebar persists across Teacher destinations; mobile uses an accessible drawer and has no horizontal document overflow at 390px.
- Long pages scroll inside the main content region without hiding the sidebar.
- All current Teacher URLs, controllers, models, forms, validation, Bootstrap modal behavior and TinyMCE workflows remain functional.
- Admin and Student UI contract tests continue to pass.
- Application builds successfully, runs locally, and is left open on `/Teacher` for review.
