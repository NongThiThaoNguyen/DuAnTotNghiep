# Classic Luxury Role UI Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade the Admin, Student, and Teacher workspaces into one cohesive classic luxury interface while preserving each role's workflow.

**Architecture:** Establish a shared product/design contract first, then move role layouts onto a shared role-shell CSS layer. Use `google/design.md` as the design-system source of truth, `taste-skill` as the anti-generic design filter, and `impeccable` as the audit/polish workflow for the actual Razor surfaces.

**Tech Stack:** ASP.NET Core MVC/Razor, Tailwind v4 compiled CSS, existing Bootstrap usage in Teacher area, jQuery/site.js for current layout interactions, `@google/design.md` for DESIGN.md linting, Playwright for visual checks.

---

## Design Direction

**Register:** product UI. The interface serves repeated tasks: administration, learning, teaching, reviewing, editing, and tracking.

**Aesthetic:** classic luxury academic, not ornamental vintage. Think quiet university archive, premium notebook, refined editorial admin console. The surface should feel mature, calm, and trustworthy.

**Shared visual language:**
- Canvas: cool ivory/off-white, not beige craft paper.
- Ink: deep charcoal/navy-black.
- Accent: antique brass or muted oxblood used sparingly for active states and primary actions.
- Surfaces: flat by default, with hairline borders and restrained shadow only for overlays.
- Typography: one readable sans for product UI, with optional restrained serif only for page titles or empty-state headings if it does not hurt scanability.
- Shape: 8px for dense controls, 12px for larger panels, no 24px+ rounded cards.
- Motion: 150-220ms state transitions only. No page-load choreography.

**Role nuance:**
- Admin: command desk. Dense, serious, table-first, strong hierarchy, minimal decoration.
- Student: study room. Calmer spacing, progress-oriented cards, warm encouragement without fake metrics.
- Teacher: atelier. Creation/review workspace, clearer forms, calmer content panels, remove purple gradient/glass overrides.

## Current Findings

- `Views/Shared/_AdminLayout.cshtml` is a 920-line layout with large inline CSS, Roboto, slate/blue tokens, Bootstrap-like utility redefinitions, and table/button/card styles.
- `Views/Shared/_StudentLayout.cshtml` is a 543-line Tailwind/Razor layout with Roboto, a collapsible sidebar, many embedded SVG strings, and blue active states.
- `Views/Shared/_TeacherLayout.cshtml` is a 1207-line layout that imports Bootstrap, Font Awesome, Be Vietnam Pro, many `!important` overrides, purple brand sync, gradients, glass toast styling, and animated form cards.
- The three role layouts are visually inconsistent: different font sources, component rules, color systems, icon systems, and interaction behavior.

## Files

- Create: `PRODUCT.md`
- Create: `DESIGN.md`
- Create: `.impeccable/design.json`
- Create: `wwwroot/css/role-shell.css`
- Modify: `Views/Shared/_AdminLayout.cshtml`
- Modify: `Views/Shared/_StudentLayout.cshtml`
- Modify: `Views/Shared/_TeacherLayout.cshtml`
- Modify: `Views/Shared/_Navbar.cshtml` if shared top navigation remains visible in Student/Admin shells
- Modify selectively: high-impact role landing pages:
  - `Areas/Admin/Views/Home/Index.cshtml`
  - `Areas/Student/Views/Home/Index.cshtml`
  - first Teacher dashboard/landing view identified during implementation
- Test/verify: `npm run design:lint`, `dotnet build`, Playwright screenshots at desktop and mobile widths.

## Task 1: Product and Design System Contract

**Files:**
- Create: `PRODUCT.md`
- Create: `DESIGN.md`
- Create: `.impeccable/design.json`

- [ ] **Step 1: Write `PRODUCT.md`**

Capture the product register and constraints:

```markdown
# Product

## Register

product

## Users
Students learning English through AI-assisted paths, teachers creating and reviewing learning material, and administrators managing users, content, reports, and system settings.

## Product Purpose
AI Study English helps students follow structured learning paths while giving teachers and administrators clear operational control. Success means users can understand where they are, what needs attention, and what action to take next without decorative noise.

## Brand Personality
Classic, scholarly, composed, and trustworthy. The interface should feel like a refined academic workspace: calm surfaces, precise controls, and confident hierarchy.

## Anti-references
Do not use generic AI SaaS gradients, purple glow systems, glassmorphism as decoration, fake metrics, fake testimonials, identical three-card feature grids, oversized rounded cards, or mixed icon styles. Do not make Admin, Student, and Teacher feel like separate products.

## Design Principles
1. Shared system, role-specific emphasis.
2. Dense where users manage data, spacious where users study or compose.
3. Classic luxury through restraint, typography, borders, and proportion.
4. State must be obvious: active, disabled, warning, success, error, and loading states cannot rely on color alone.
5. Preserve existing workflows and URLs while improving the shell around them.

## Accessibility & Inclusion
Target WCAG AA contrast, visible focus states, 44px touch targets on mobile, reduced-motion-friendly transitions, and color-blind-safe status indicators with text or icon support.
```

- [ ] **Step 2: Write `DESIGN.md`**

Use `google/design.md` as the parseable contract. Include exactly the six required sections: Overview, Colors, Typography, Elevation, Components, Do's and Don'ts. Use hex values in frontmatter for lint compatibility.

Design tokens:

```yaml
---
name: AI Study English Classic Role Shell
description: A cohesive classic luxury product interface for Admin, Student, and Teacher workspaces.
colors:
  canvas: "#F7F5EF"
  surface: "#FFFEFA"
  surface-muted: "#EFEBE1"
  ink: "#171A1F"
  ink-muted: "#5D6673"
  line: "#D8D2C4"
  brass: "#9A6A2F"
  brass-deep: "#6F4A1F"
  oxblood: "#7A2634"
  success: "#2F6B4F"
  warning: "#9A6A2F"
  danger: "#9B2F32"
  info: "#315F7D"
typography:
  display:
    fontFamily: "Be Vietnam Pro, Arial, sans-serif"
    fontSize: "2rem"
    fontWeight: 700
    lineHeight: 1.15
    letterSpacing: "-0.015em"
  title:
    fontFamily: "Be Vietnam Pro, Arial, sans-serif"
    fontSize: "1.125rem"
    fontWeight: 700
    lineHeight: 1.35
  body:
    fontFamily: "Be Vietnam Pro, Arial, sans-serif"
    fontSize: "0.9375rem"
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: "Be Vietnam Pro, Arial, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 700
    lineHeight: 1.2
    letterSpacing: "0.04em"
rounded:
  control: "8px"
  panel: "12px"
  pill: "999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "16px"
  lg: "24px"
  xl: "32px"
components:
  button-primary:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.surface}"
    rounded: "{rounded.control}"
    padding: "10px 16px"
  button-secondary:
    backgroundColor: "{colors.surface}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
    padding: "10px 16px"
  nav-active:
    backgroundColor: "{colors.surface-muted}"
    textColor: "{colors.ink}"
    rounded: "{rounded.control}"
---
```

- [ ] **Step 3: Write `.impeccable/design.json`**

Mirror the DESIGN.md narrative and include sidecar-only metadata: color display names, shadow tokens, transition tokens, breakpoints, and sample components. Include sample snippets for Primary Button, Secondary Button, Role Sidebar Link, Table, Card, Input, Badge, and Toast.

- [ ] **Step 4: Validate design contract**

Run:

```powershell
npm run design:lint
```

Expected: DESIGN.md parses. If warnings appear for schema nuance, fix token names/values instead of ignoring them.

## Task 2: Shared Role Shell CSS

**Files:**
- Create: `wwwroot/css/role-shell.css`
- Modify: `Views/Shared/_AdminLayout.cshtml`
- Modify: `Views/Shared/_StudentLayout.cshtml`
- Modify: `Views/Shared/_TeacherLayout.cshtml`

- [ ] **Step 1: Create shared CSS layer**

Create `wwwroot/css/role-shell.css` with CSS custom properties and shared classes:

```css
:root {
  --rs-canvas: #F7F5EF;
  --rs-surface: #FFFEFA;
  --rs-surface-muted: #EFEBE1;
  --rs-ink: #171A1F;
  --rs-ink-muted: #5D6673;
  --rs-line: #D8D2C4;
  --rs-brass: #9A6A2F;
  --rs-brass-deep: #6F4A1F;
  --rs-oxblood: #7A2634;
  --rs-success: #2F6B4F;
  --rs-warning: #9A6A2F;
  --rs-danger: #9B2F32;
  --rs-info: #315F7D;
  --rs-radius-control: 8px;
  --rs-radius-panel: 12px;
  --rs-shadow-raised: 0 10px 28px rgb(23 26 31 / 0.08);
  --rs-transition: 180ms ease;
}

.role-shell {
  min-height: 100dvh;
  background: var(--rs-canvas);
  color: var(--rs-ink);
  font-family: "Be Vietnam Pro", Arial, sans-serif;
}

.role-sidebar {
  background: var(--rs-ink);
  color: var(--rs-surface);
  border-right: 1px solid rgb(255 254 250 / 0.1);
}

.role-content {
  background: var(--rs-canvas);
}

.role-panel,
.card {
  background: var(--rs-surface);
  border: 1px solid var(--rs-line);
  border-radius: var(--rs-radius-panel);
  box-shadow: none;
}

.role-nav-link {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  border-radius: var(--rs-radius-control);
  color: rgb(255 254 250 / 0.72);
  transition: background-color var(--rs-transition), color var(--rs-transition);
}

.role-nav-link:hover,
.role-nav-link.is-active {
  background: rgb(255 254 250 / 0.1);
  color: var(--rs-surface);
}

.btn-primary,
.rs-btn-primary {
  background: var(--rs-ink);
  border-color: var(--rs-ink);
  color: var(--rs-surface);
  border-radius: var(--rs-radius-control);
}

.btn-primary:hover,
.rs-btn-primary:hover {
  background: var(--rs-brass-deep);
  border-color: var(--rs-brass-deep);
}

.form-control,
.form-select {
  border-color: var(--rs-line);
  border-radius: var(--rs-radius-control);
}

.form-control:focus,
.form-select:focus {
  border-color: var(--rs-brass);
  box-shadow: 0 0 0 3px rgb(154 106 47 / 0.16);
}

.table {
  border-color: var(--rs-line);
  background: var(--rs-surface);
}

.table thead th {
  background: var(--rs-surface-muted);
  color: var(--rs-ink);
}

@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    transition-duration: 0.01ms !important;
    animation-duration: 0.01ms !important;
  }
}
```

- [ ] **Step 2: Link `role-shell.css` in all role layouts**

In each role layout head, load after `site.css`:

```razor
<link rel="stylesheet" href="~/css/role-shell.css" asp-append-version="true" />
```

- [ ] **Step 3: Add shell class to each body**

Use:

```razor
<body class="role-shell ...">
```

Keep existing layout classes needed for flex behavior.

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build
```

Expected: build succeeds with no Razor syntax errors.

## Task 3: Admin Workspace Upgrade

**Files:**
- Modify: `Views/Shared/_AdminLayout.cshtml`
- Modify: `Areas/Admin/Views/Home/Index.cshtml`

- [ ] **Step 1: Replace blue active states with role-shell active classes**

Change `NavClass` to return a shared `role-nav-link` class and an `is-active` flag:

```csharp
return "admin-nav-link role-nav-link group flex items-center gap-3 px-4 py-3 text-sm font-medium transition-all " +
    (isActive ? "is-active" : "");
```

- [ ] **Step 2: Remove or neutralize old blue values in inline CSS**

Replace key admin CSS values:

```css
.text-primary { color: var(--rs-brass); }
.btn-primary { background: var(--rs-ink); color: var(--rs-surface); border-color: var(--rs-ink); }
.btn-primary:hover { background: var(--rs-brass-deep); border-color: var(--rs-brass-deep); }
.btn-outline-primary { background: var(--rs-surface); color: var(--rs-brass-deep); border-color: var(--rs-line); }
.btn-outline-primary:hover { background: var(--rs-surface-muted); color: var(--rs-ink); }
.table thead th { background: var(--rs-surface-muted); color: var(--rs-ink); }
```

- [ ] **Step 3: Refine Admin dashboard cards**

In `Areas/Admin/Views/Home/Index.cshtml`, reduce decorative gradients and oversized shadows. Use `role-panel`, semantic status colors, and tighter data hierarchy.

- [ ] **Step 4: Admin visual checks**

Check these screens:
- Admin dashboard
- User index
- Audit logs index
- Any create/edit form

Verify: no purple/blue gradient hero, no fake luxury decoration, dense tables remain readable.

## Task 4: Student Workspace Upgrade

**Files:**
- Modify: `Views/Shared/_StudentLayout.cshtml`
- Modify: `Areas/Student/Views/Home/Index.cshtml`

- [ ] **Step 1: Move sidebar to shared dark classic shell**

Change the Student sidebar root from white/slate to:

```razor
<aside id="mobileSidebar" class="role-sidebar fixed inset-y-0 left-0 z-50 w-70 transform -translate-x-full transition-transform duration-300 ease-in-out lg:translate-x-0 lg:static lg:z-0 lg:h-[calc(100vh-4rem)] flex flex-col justify-between shrink-0 overflow-y-auto">
```

- [ ] **Step 2: Apply `role-nav-link` to Student menu links**

For active menu links, use `role-nav-link is-active`. For inactive links, use `role-nav-link`.

- [ ] **Step 3: Reduce disabled-menu clutter**

Keep disabled items visible only when they communicate a near-term capability. Replace repeated `"Sắp ra mắt"` badges with a calmer muted style:

```razor
<span class="text-[10px] font-semibold text-[color:rgb(255_254_250_/_0.55)] border border-[color:rgb(255_254_250_/_0.16)] px-1.5 py-0.5 rounded badge-text shrink-0">@sub.BadgeText</span>
```

- [ ] **Step 4: Refine Student dashboard**

In `Areas/Student/Views/Home/Index.cshtml`, keep progress and next-action sections, but remove AI-generic stats and decorative gradient blocks. Use study-focused panels:
- Today
- Current path
- Next lesson
- Recent feedback
- Skill progress

- [ ] **Step 5: Student visual checks**

Check:
- Student dashboard
- LearningPath index/detail
- Onboarding level/skills/study time
- Placement test intro/result

Verify: mobile sidebar remains usable, collapsed desktop sidebar remains functional, progress cards do not look like generic SaaS cards.

## Task 5: Teacher Workspace Upgrade

**Files:**
- Modify: `Views/Shared/_TeacherLayout.cshtml`
- Modify: first Teacher dashboard/landing view found during implementation

- [ ] **Step 1: Keep Bootstrap compatibility but override purple system**

Remove or replace the purple sync block that maps `.bg-blue-600`, `.bg-indigo-600`, `.btn-primary`, and related classes to `#6C63FF`.

Use:

```css
.btn-primary-ailearn {
  background: var(--rs-ink) !important;
  color: var(--rs-surface) !important;
  border: 1px solid var(--rs-ink) !important;
  border-radius: var(--rs-radius-control) !important;
  box-shadow: none !important;
}

.btn-primary-ailearn:hover {
  background: var(--rs-brass-deep) !important;
  border-color: var(--rs-brass-deep) !important;
  transform: none !important;
  box-shadow: none !important;
}
```

- [ ] **Step 2: Remove decorative glass and gradient form card rules**

Replace `.toast-notification` glass styling and `.main-content .bg-white.rounded-2xl` gradient/accent-bar styling with role-shell panel styling:

```css
.toast-notification {
  background: var(--rs-surface) !important;
  border: 1px solid var(--rs-line) !important;
  border-radius: var(--rs-radius-panel) !important;
  box-shadow: var(--rs-shadow-raised) !important;
}

.main-content .bg-white.rounded-2xl {
  border: 1px solid var(--rs-line) !important;
  box-shadow: none !important;
  background: var(--rs-surface) !important;
  animation: none !important;
}

.main-content .bg-white.rounded-2xl::after {
  display: none !important;
}
```

- [ ] **Step 3: Refine form controls**

Keep Teacher forms readable and fast:

```css
.main-content input:focus,
.main-content select:focus,
.main-content textarea:focus {
  border-color: var(--rs-brass) !important;
  box-shadow: 0 0 0 3px rgb(154 106 47 / 0.16) !important;
  transform: none !important;
}
```

- [ ] **Step 4: Teacher visual checks**

Check:
- Teacher dashboard/landing
- A create/edit content form
- Table/list pages
- Modal flow

Verify: no purple glow, no gradient bars, Bootstrap controls still work, TinyMCE remains usable.

## Task 6: Cross-Role Consistency Pass

**Files:**
- Modify: `wwwroot/css/role-shell.css`
- Modify role layouts as needed

- [ ] **Step 1: Normalize font loading**

Use one font source across role layouts. Prefer existing `Be Vietnam Pro` for Vietnamese product UI. Remove duplicate Roboto imports from role layouts after confirming no page depends on Roboto-specific measurements.

- [ ] **Step 2: Normalize icon treatment**

Do not rewrite every icon immediately. Set shared icon size/color rules:

```css
.role-sidebar svg,
.role-nav-link svg {
  width: 1.05rem;
  height: 1.05rem;
  flex-shrink: 0;
  stroke-width: 1.8;
}
```

- [ ] **Step 3: Normalize status palette**

Map success, warning, danger, info to DESIGN.md tokens. Avoid using accent brass for error/success states.

- [ ] **Step 4: Normalize focus states**

Confirm buttons, links, inputs, sidebar toggles, dropdown triggers, and modal close buttons have visible `:focus-visible`.

## Task 7: Verification and Impeccable Audit

**Files:**
- No planned source files unless checks reveal defects

- [ ] **Step 1: Run design lint**

```powershell
npm run design:lint
```

Expected: DESIGN.md parses successfully.

- [ ] **Step 2: Run build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 3: Start app**

Use the project's normal run command:

```powershell
dotnet run
```

Expected: app starts and logs a localhost URL.

- [ ] **Step 4: Capture desktop screenshots**

Use Playwright at 1440x1000 for:
- Admin dashboard
- Student dashboard
- Teacher dashboard

Expected: all three share the same shell language, top/sidebar proportions, color system, and control styling.

- [ ] **Step 5: Capture mobile screenshots**

Use Playwright at 390x844 for the same three surfaces.

Expected: no horizontal overflow, sidebar/drawer works, text does not overlap.

- [ ] **Step 6: Run Impeccable-style audit checklist**

Check:
- No generic AI purple/blue gradients.
- No glassmorphism-as-default.
- No fake metrics/testimonials.
- No identical three-card scaffold where information density calls for lists/tables.
- No oversized card radius above 16px except true pills.
- No decorative shadows paired with decorative borders.
- No low-contrast muted text.
- Role identity is nuanced, not separate visual systems.

## Task 8: Rollout Order

Execute in this order:

1. `PRODUCT.md`, `DESIGN.md`, `.impeccable/design.json`
2. `role-shell.css`
3. Admin layout
4. Student layout
5. Teacher layout
6. Three dashboard/landing surfaces
7. Cross-role consistency pass
8. Build, lint, screenshots, polish

This order reduces risk because shared tokens land before role-specific edits, and Teacher's heavier Bootstrap/override stack is handled after the cleaner Admin/Student shells establish the target language.

## Acceptance Criteria

- Admin, Student, and Teacher clearly belong to the same product.
- The look is classic, luxurious, and academic through restraint, not decoration.
- Existing navigation, links, routes, Razor sections, dropdowns, modals, and sidebar interactions still work.
- Teacher no longer uses the purple/glow/glass visual system.
- Student and Admin no longer rely on bright blue as the dominant identity color.
- DESIGN.md and `.impeccable/design.json` exist and can guide future UI work.
- `dotnet build` passes.
- Visual checks pass on desktop and mobile.
