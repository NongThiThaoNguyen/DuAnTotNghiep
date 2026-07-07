# Student Classic UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unify every Student learning screen under the same classic academic role shell already approved for Admin.

**Architecture:** Both `_StudentLayout` and `_AILearnLayout` render one shared Student sidebar and the same account component. `student-shell.css` extends the existing `role-shell.css` tokens and overrides the legacy Tailwind/Bootstrap page styles without changing controllers, models, forms, or URLs.

**Tech Stack:** ASP.NET Core Razor MVC, Tailwind utility output, Bootstrap 5, vanilla CSS, Node built-in test runner.

---

### Task 1: Lock the shared shell contract

**Files:**
- Create: `Tests/Ui/StudentShell.Tests.cjs`
- Test: `Views/Shared/_StudentLayout.cshtml`
- Test: `Views/Shared/_AILearnLayout.cshtml`

- [ ] Write assertions that both layouts use `_StudentSidebar`, `role-shell.css`, `student-shell.css`, Be Vietnam Pro, and no legacy shared navbar.
- [ ] Run `node --test Tests/Ui/StudentShell.Tests.cjs` and confirm it fails on the old layouts.

### Task 2: Build the shared Student navigation

**Files:**
- Create: `Views/Shared/_StudentSidebar.cshtml`
- Create: `Views/Shared/_StudentAccountStrip.cshtml`
- Modify: `Views/Shared/_StudentLayout.cshtml`
- Modify: `Views/Shared/_AILearnLayout.cshtml`

- [ ] Render only working Student destinations: today, overview, learning path, study plan, progress, placement, courses, quiz, AI Tutor, notes, achievements, and profile.
- [ ] Use route-aware active states and the existing synchronous sidebar-state helper.
- [ ] Replace both legacy headers with one mobile top bar and one desktop account strip.

### Task 3: Apply the classic Student visual system

**Files:**
- Create: `wwwroot/css/student-shell.css`
- Modify: `DESIGN.md`

- [ ] Extend the approved canvas, surface, ink, brass, border, and semantic status tokens.
- [ ] Override legacy purple gradients, Poppins/Roboto typography, oversized rounding, broad shadows, and inconsistent controls.
- [ ] Add structural mobile drawer behavior and 40-44px touch targets.

### Task 4: Refine the daily Student dashboard

**Files:**
- Modify: `Views/Dashboard/Index.cshtml`
- Modify: `wwwroot/css/dashboard.css`

- [ ] Remove emoji-heavy and generic AI-SaaS copy.
- [ ] Prioritize next task, progress, study metrics, recent activity, and AI help using the shared component vocabulary.
- [ ] Keep all existing model fields and target URLs intact.

### Task 5: Verify every Student destination

**Files:**
- Test: `Tests/Ui/StudentShell.Tests.cjs`

- [ ] Run the Node shell tests and `dotnet build`.
- [ ] Audit all real Student navigation routes at desktop and 390px mobile widths.
- [ ] Confirm one active item, no horizontal overflow, visible account identity, working mobile drawer, and consistent shell CSS.
- [ ] Run `npm run design:lint` and `git diff --check`.
