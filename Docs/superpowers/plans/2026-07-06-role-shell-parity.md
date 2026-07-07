# Admin and Student Role Shell Parity Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Admin and Student header, sidebar, palette, spacing, active state, and responsive drawer match the approved Teacher shell while preserving role-specific content and behavior.

**Architecture:** `role-shell.css` owns shared geometry and visual primitives through `.rs-*` classes. Teacher keeps its current role classes as compatibility aliases, while Student and Admin add the shared classes to their existing Razor structures. A small shared `role-shell.js` controls only Admin/Student drawer state through explicit data attributes.

**Tech Stack:** ASP.NET Core Razor MVC, vanilla CSS, vanilla JavaScript, Noto Sans, Node built-in test runner, in-app browser.

---

## File Responsibilities

- `wwwroot/css/role-shell.css`: shared shell tokens, desktop geometry, sidebar/header components, mobile drawer, and responsive behavior.
- `wwwroot/js/role-shell.js`: explicit drawer toggle, overlay, Escape, navigation-close, and body-scroll behavior.
- `Views/Shared/_TeacherLayout.cshtml`, `_TeacherSidebar.cshtml`, `_TeacherAccountStrip.cshtml`: reference shell receives shared `.rs-*` aliases without behavior changes.
- `Views/Shared/_StudentLayout.cshtml`, `_StudentSidebar.cshtml`, `_StudentAccountStrip.cshtml`: Student adopts the reference structure while keeping its routes, notification count, and labels.
- `Views/Shared/_AdminLayout.cshtml`: Admin adopts the shared shell structure while keeping all Admin navigation and compatibility CSS.
- `Tests/Ui/RoleShellParity.Tests.cjs`: focused contract tests for parity and responsive ownership.

### Task 1: Lock the shared shell contract

**Files:**
- Create: `Tests/Ui/RoleShellParity.Tests.cjs`
- Test: `wwwroot/css/role-shell.css`
- Test: `Views/Shared/_TeacherLayout.cshtml`
- Test: `Views/Shared/_StudentLayout.cshtml`
- Test: `Views/Shared/_AdminLayout.cshtml`

- [ ] **Step 1: Write the failing parity tests**

```javascript
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const root = path.resolve(__dirname, "../..");
const read = relativePath => fs.readFileSync(path.join(root, relativePath), "utf8");

test("role shell owns the shared Teacher geometry", () => {
  const css = read("wwwroot/css/role-shell.css");
  assert.match(css, /--rs-sidebar-width:\s*17\.25rem/);
  assert.match(css, /--rs-account-height:\s*4\.5rem/);
  assert.match(css, /\.rs-shell-frame\s*{[\s\S]*grid-template-columns:\s*var\(--rs-sidebar-width\)/);
  assert.match(css, /\.rs-account-strip\s*{[\s\S]*min-height:\s*var\(--rs-account-height\)/);
  assert.match(css, /@media \(max-width:\s*1023px\)[\s\S]*\.rs-mobile-topbar/);
});

test("all role layouts consume the shared shell primitives", () => {
  const layouts = [
    read("Views/Shared/_TeacherLayout.cshtml"),
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AdminLayout.cshtml")
  ];
  for (const layout of layouts) {
    assert.match(layout, /rs-shell-frame/);
    assert.match(layout, /rs-main-content/);
    assert.match(layout, /rs-mobile-topbar/);
  }
});

test("Admin and Student account headers match the Teacher structure", () => {
  const admin = read("Views/Shared/_AdminLayout.cshtml");
  const student = read("Views/Shared/_StudentAccountStrip.cshtml");
  for (const source of [admin, student]) {
    assert.match(source, /rs-account-strip/);
    assert.match(source, /rs-account-context/);
    assert.match(source, /rs-account-actions/);
    assert.match(source, /rs-account-avatar/);
  }
});

test("Admin and Student use the shared explicit drawer behavior", () => {
  const script = read("wwwroot/js/role-shell.js");
  const layouts = [
    read("Views/Shared/_StudentLayout.cshtml"),
    read("Views/Shared/_AdminLayout.cshtml")
  ];
  assert.match(script, /\[data-rs-sidebar\]/);
  assert.match(script, /event\.key === "Escape"/);
  for (const layout of layouts) {
    assert.match(layout, /data-rs-sidebar/);
    assert.match(layout, /js\/role-shell\.js/);
  }
});
```

- [ ] **Step 2: Run the test and verify RED**

Run: `node --test Tests\Ui\RoleShellParity.Tests.cjs`

Expected: FAIL because the shared geometry variables, `.rs-*` structural classes, and `role-shell.js` do not exist yet.

- [ ] **Step 3: Commit the contract test**

```powershell
git add Tests/Ui/RoleShellParity.Tests.cjs
git commit -m "test: define shared role shell parity"
```

### Task 2: Create shared shell primitives and drawer behavior

**Files:**
- Modify: `wwwroot/css/role-shell.css`
- Create: `wwwroot/js/role-shell.js`
- Modify: `Views/Shared/_TeacherLayout.cshtml`
- Modify: `Views/Shared/_TeacherSidebar.cshtml`
- Modify: `Views/Shared/_TeacherAccountStrip.cshtml`
- Test: `Tests/Ui/RoleShellParity.Tests.cjs`

- [ ] **Step 1: Add shared geometry tokens and components**

Add these tokens to `:root` and implement the classes using the current Teacher values:

```css
--rs-sidebar-width: 17.25rem;
--rs-account-height: 4.5rem;
--rs-mobile-bar-height: 3.5rem;

.rs-shell-frame {
  display: grid;
  grid-template-columns: var(--rs-sidebar-width) minmax(0, 1fr);
  height: 100%;
  overflow: hidden;
}

.rs-sidebar {
  display: flex;
  min-width: 0;
  height: 100%;
  flex-direction: column;
  overflow-y: auto;
  border-right: 1px solid var(--rs-line);
  background: var(--rs-canvas);
}

.rs-sidebar-body { flex: 1 1 auto; padding: 1.25rem 0.9rem; }
.rs-sidebar-footer { flex: 0 0 auto; padding: 0.9rem; border-top: 1px solid var(--rs-line); background: var(--rs-canvas); }
.rs-brand { display: flex; min-height: 3rem; align-items: center; gap: 0.75rem; margin: 0 0.4rem 1.5rem; color: var(--rs-ink); }
.rs-nav-section { margin: 1.35rem 0.75rem 0.45rem; color: var(--rs-ink-muted); font-size: 0.68rem; font-weight: 700; text-transform: uppercase; }
.rs-nav-list { display: grid; gap: 0.2rem; }
.rs-nav-link { min-height: 2.75rem; gap: 0.8rem; padding: 0.65rem 0.75rem; }

.rs-main-content { min-width: 0; height: 100%; overflow-x: hidden; overflow-y: auto; overscroll-behavior: contain; }
.rs-account-strip { position: sticky; top: 0; z-index: 20; display: flex; min-height: var(--rs-account-height); align-items: center; justify-content: space-between; gap: 1rem; padding: 0.75rem var(--rs-space-page-x); border-bottom: 1px solid var(--rs-line); background: rgb(255 254 250 / 0.96); }
.rs-account-context, .rs-account-name { display: grid; min-width: 0; }
.rs-account-actions { display: flex; align-items: center; gap: 0.55rem; }
.rs-account-avatar { display: inline-grid; width: 2.2rem; height: 2.2rem; place-items: center; border-radius: var(--rs-radius-control); background: var(--rs-ink); color: var(--rs-surface); font-size: 0.72rem; font-weight: 800; }
```

- [ ] **Step 2: Add the shared responsive rules**

```css
@media (max-width: 1023px) {
  .rs-shell-frame { display: block; height: calc(100% - var(--rs-mobile-bar-height)); }
  .rs-mobile-topbar { display: grid; grid-template-columns: var(--rs-hit-area) minmax(0, 1fr) var(--rs-hit-area); height: var(--rs-mobile-bar-height); align-items: center; gap: 0.75rem; padding: 0 0.75rem; border-bottom: 1px solid var(--rs-line); background: var(--rs-surface); }
  .rs-sidebar { position: fixed; inset: 0 auto 0 0; z-index: var(--rs-z-sidebar); width: min(18rem, 86vw); height: 100dvh; box-shadow: var(--rs-shadow-raised); transition: transform var(--rs-transition); }
  .rs-sidebar.is-closed { transform: translateX(-100%); }
  .rs-sidebar-overlay { position: fixed; inset: 0; z-index: var(--rs-z-overlay); display: block; background: rgb(23 26 31 / 0.38); }
  .rs-sidebar-overlay[hidden] { display: none !important; }
  .rs-account-strip { display: none; }
}

@media (min-width: 1024px) {
  .rs-mobile-topbar { display: none; }
  .rs-sidebar, .rs-sidebar.is-closed { transform: none; }
}
```

- [ ] **Step 3: Add explicit shared drawer JavaScript**

```javascript
(function () {
  "use strict";
  const root = document.querySelector("[data-rs-shell]");
  const sidebar = root?.querySelector("[data-rs-sidebar]");
  const toggle = root?.querySelector("[data-rs-toggle]");
  const overlay = root?.querySelector("[data-rs-overlay]");
  if (!root || !sidebar || !toggle || !overlay) return;

  function setOpen(open) {
    sidebar.classList.toggle("is-closed", !open);
    overlay.hidden = !open;
    toggle.setAttribute("aria-expanded", String(open));
    document.body.classList.toggle("rs-drawer-open", open);
  }

  toggle.addEventListener("click", () => setOpen(toggle.getAttribute("aria-expanded") !== "true"));
  overlay.addEventListener("click", () => setOpen(false));
  sidebar.addEventListener("click", event => {
    if (event.target.closest("a") && window.innerWidth < 1024) setOpen(false);
  });
  document.addEventListener("keydown", event => {
    if (event.key === "Escape") setOpen(false);
  });
})();
```

- [ ] **Step 4: Add shared aliases to Teacher**

Add `rs-shell-frame`, `rs-main-content`, `rs-mobile-topbar`, `rs-sidebar`, `rs-sidebar-body`, `rs-sidebar-footer`, `rs-brand`, `rs-nav-section`, `rs-nav-list`, `rs-nav-link`, `rs-account-strip`, `rs-account-context`, `rs-account-actions`, and `rs-account-avatar` alongside the existing Teacher classes. Do not remove Teacher classes.

- [ ] **Step 5: Run parity and Teacher tests**

Run: `node --test Tests\Ui\RoleShellParity.Tests.cjs Tests\Ui\TeacherShell.Tests.cjs`

Expected: shared CSS assertions pass; layout assertions still fail for Admin and Student.

- [ ] **Step 6: Commit shared primitives**

```powershell
git add wwwroot/css/role-shell.css wwwroot/js/role-shell.js Views/Shared/_TeacherLayout.cshtml Views/Shared/_TeacherSidebar.cshtml Views/Shared/_TeacherAccountStrip.cshtml Tests/Ui/RoleShellParity.Tests.cjs
git commit -m "refactor: centralize role shell primitives"
```

### Task 3: Adopt the shared shell in Student

**Files:**
- Modify: `Views/Shared/_StudentLayout.cshtml`
- Modify: `Views/Shared/_StudentSidebar.cshtml`
- Modify: `Views/Shared/_StudentAccountStrip.cshtml`
- Modify: `wwwroot/css/student-shell.css`
- Test: `Tests/Ui/RoleShellParity.Tests.cjs`
- Test: `Tests/Ui/StudentShell.Tests.cjs`

- [ ] **Step 1: Add shared structural classes and data hooks**

Use `data-rs-shell` on the body, `data-rs-toggle` on the mobile menu button, `data-rs-sidebar` on the sidebar, and `data-rs-overlay` on the overlay. Add the corresponding shared `.rs-*` classes while preserving Student classes and all current links.

- [ ] **Step 2: Restructure the Student account strip**

```razor
<header class="student-account-strip rs-account-strip">
    <div class="rs-account-context">
        <span class="rs-account-eyebrow">Học viên</span>
        <span class="rs-account-page">@ViewData["Title"]</span>
    </div>
    <div class="student-account-actions rs-account-actions">
        <!-- Keep the existing notification link and unread count. -->
        <a href="/Profile" class="student-account-card rs-account-profile">
            <span class="student-account-meta rs-account-name">
                <strong>@userName</strong>
                <small>Hồ sơ học viên</small>
            </span>
            <span class="student-account-avatar rs-account-avatar">@initials</span>
        </a>
    </div>
</header>
```

- [ ] **Step 3: Replace inline drawer behavior with the shared script**

Remove the Student drawer script from `_StudentLayout.cshtml` and load `~/js/role-shell.js` after `site.js`.

- [ ] **Step 4: Remove conflicting Student shell dimensions**

Keep Student page-body compatibility rules, but change its shell declarations to consume `--rs-sidebar-width`, `--rs-account-height`, and `--rs-mobile-bar-height`, matching Teacher values exactly.

- [ ] **Step 5: Run Student and parity tests**

Run: `node --test Tests\Ui\RoleShellParity.Tests.cjs Tests\Ui\StudentShell.Tests.cjs`

Expected: Student and Teacher contracts pass; Admin layout assertions remain failing.

- [ ] **Step 6: Commit Student adoption**

```powershell
git add Views/Shared/_StudentLayout.cshtml Views/Shared/_StudentSidebar.cshtml Views/Shared/_StudentAccountStrip.cshtml wwwroot/css/student-shell.css Tests/Ui/RoleShellParity.Tests.cjs Tests/Ui/StudentShell.Tests.cjs
git commit -m "refactor: align student shell with teacher"
```

### Task 4: Adopt the shared shell in Admin

**Files:**
- Modify: `Views/Shared/_AdminLayout.cshtml`
- Test: `Tests/Ui/RoleShellParity.Tests.cjs`
- Test: `Tests/Ui/AdminSidebarState.Tests.cjs`

- [ ] **Step 1: Add shared structural classes and data hooks**

Apply the same `data-rs-*` hooks and `.rs-*` structure used by Student. Preserve every Admin link, role check, controller/action match, breadcrumb, compatibility style, and page wrapper.

- [ ] **Step 2: Move the Admin account strip above the constrained page body**

```razor
<main id="mainContent" role="main" class="role-content rs-main-content flex min-h-0 flex-1 flex-col">
    <header class="admin-account-strip rs-account-strip">
        <div class="rs-account-context">
            <span class="rs-account-eyebrow">Quản trị viên</span>
            <span class="rs-account-page">@ViewData["Title"]</span>
        </div>
        <div class="rs-account-actions">
            <a asp-area="Admin" asp-controller="Notifications" asp-action="Index" class="rs-notification-button" aria-label="Thông báo">
                <!-- Keep the existing icon system. -->
            </a>
            <a class="admin-account-card rs-account-profile" asp-area="" asp-controller="Profile" asp-action="Index">
                <span class="admin-account-meta rs-account-name">
                    <strong>@userName</strong>
                    <small>Hồ sơ quản trị viên</small>
                </span>
                <span class="admin-account-avatar rs-account-avatar">@userInitials</span>
            </a>
        </div>
    </header>
    <div class="flex-1 p-4 sm:p-6 lg:p-8 max-w-7xl w-full mx-auto">
        <!-- Existing breadcrumb and RenderBody stay unchanged. -->
    </div>
</main>
```

- [ ] **Step 3: Replace inline Admin drawer behavior with the shared script**

Remove the final inline toggle script, load `~/js/role-shell.js`, and retain the separate synchronous `AdminSidebarState` restore/bind block.

- [ ] **Step 4: Run all shell tests**

Run: `node --test Tests\Ui\RoleShellParity.Tests.cjs Tests\Ui\TeacherShell.Tests.cjs Tests\Ui\StudentShell.Tests.cjs Tests\Ui\AdminSidebarState.Tests.cjs Tests\Ui\StaticAssets.Tests.cjs`

Expected: all tests pass.

- [ ] **Step 5: Commit Admin adoption**

```powershell
git add Views/Shared/_AdminLayout.cshtml Tests/Ui/RoleShellParity.Tests.cjs
git commit -m "refactor: align admin shell with teacher"
```

### Task 5: Build and browser verification

**Files:**
- Modify: only files above if verification exposes a shell-specific defect.

- [ ] **Step 1: Run build and design checks**

Run:

```powershell
dotnet build DuAnTotNghiep.csproj -p:UseAppHost=false
npm run design:lint
git diff --check
```

Expected: build has 0 errors; design lint has 0 errors; diff check has no whitespace errors.

- [ ] **Step 2: Restart the local application**

Run the built application at `http://127.0.0.1:5117`, reusing the port only when it belongs to this workspace.

- [ ] **Step 3: Verify the role route matrix**

Check `/Teacher`, `/Student`, `/Dashboard`, and `/Admin` at `1280x720`, `768x1024`, and `390x844`. Measure sidebar width, account-strip height, computed background/text/border colors, active state, document overflow, and Noto Sans.

- [ ] **Step 4: Verify mobile behavior**

For Admin and Student, open the drawer, confirm overlay visibility and `aria-expanded`, press Escape, and confirm the drawer closes and body scrolling is restored.

- [ ] **Step 5: Leave the web application open for review**

Leave the in-app browser on the Student overview and report the running URL plus Student, Teacher, and Admin seed credentials.
