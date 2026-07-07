---
name: AI Study English Classic Role Shell
description: A cohesive classic luxury product interface for Admin, Student, and Teacher workspaces.
colors:
  primary: "#171A1F"
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
  success-bg: "#E8F1EB"
  warning: "#9A6A2F"
  warning-bg: "#F3E9D4"
  danger: "#9B2F32"
  danger-bg: "#F4E2E2"
  info: "#315F7D"
  info-bg: "#E4EDF2"
typography:
  display:
    fontFamily: "Noto Sans, Arial, sans-serif"
    fontSize: "2rem"
    fontWeight: 700
    lineHeight: 1.15
    letterSpacing: "-0.015em"
  title:
    fontFamily: "Noto Sans, Arial, sans-serif"
    fontSize: "1.125rem"
    fontWeight: 700
    lineHeight: 1.35
  body:
    fontFamily: "Noto Sans, Arial, sans-serif"
    fontSize: "0.9375rem"
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: "Noto Sans, Arial, sans-serif"
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
    backgroundColor: "{colors.primary}"
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

# Design System: AI Study English Classic Role Shell

## 1. Overview

**Creative North Star: "The Quiet Academic Command Room"**

The interface is a classic product workspace for people who need to study, teach, and administer without visual friction. It should feel like a refined academic operations room: precise, calm, and slightly formal, with depth coming from hierarchy and proportion rather than decorative effects.

Admin screens are the densest expression of the system. Student screens can breathe more, and Teacher screens can make creation flows feel composed, but all three roles share the same canvas, ink, border, action, focus, and status vocabulary.

**Key Characteristics:**
- Restrained classic luxury, not vintage ornament.
- Hairline borders and tonal layering instead of heavy card shadows.
- Ivory role sidebars, charcoal primary controls, and brass accent used only for action or state.
- Standard product UI behavior: clear focus, hover, disabled, error, success, and loading states.

## 2. Colors

The palette is a cool ivory and charcoal system with antique brass reserved for emphasis and action.

### Primary
- **Charcoal Ink:** Primary text, dark sidebars, primary buttons, and high-importance controls.
- **Antique Brass:** Active states, focus rings, selected navigation, and small action emphasis.
- **Deep Brass:** Hover state for primary actions.

### Secondary
- **Oxblood:** Rare critical emphasis for destructive or high-risk administrative moments.

### Tertiary
- **Institution Blue:** Informational state only, never as the dominant product identity.

### Neutral
- **Ivory Canvas:** Page background for role workspaces.
- **Warm Surface:** Cards, tables, forms, modals, and dropdowns.
- **Archive Tint:** Table headers, selected rows, and sidebar section separators.
- **Stone Line:** Borders, dividers, table rules, and input strokes.
- **Muted Ink:** Secondary body text and metadata.

### Named Rules
**The Brass Rarity Rule.** Brass appears on less than ten percent of a screen. Its scarcity gives it authority.

**The No Purple Glow Rule.** Purple gradients, glow shadows, and glass overlays are prohibited across role workspaces.

## 3. Typography

**Display Font:** Noto Sans, with Arial and sans-serif fallback.
**Body Font:** Noto Sans, with Arial and sans-serif fallback.
**Label/Mono Font:** Use the body family unless a data-heavy table requires the existing mono stack.

**Character:** The type is highly legible for Vietnamese diacritics, calm enough for dashboards, and steady across long student/admin workflows. Product labels remain readable; display scale is reserved for workspace page titles.

### Hierarchy
- **Display** (700, 2rem, 1.15): Role dashboard headings and major page titles.
- **Title** (700, 1.125rem, 1.35): Panel headings, card headings, and table group titles.
- **Body** (400, 0.9375rem, 1.6): Main explanatory text, list content, and form help.
- **Label** (700, 0.75rem, 0.04em): Table headers, section labels, and compact metadata.

### Named Rules
**The Product Type Rule.** No decorative display fonts in buttons, labels, tables, or form controls.

## 4. Elevation

The system is flat by default. Depth is conveyed through tonal surfaces, hairline borders, and measured spacing. Shadows are reserved for overlays, dropdowns, and temporary floating UI.

### Shadow Vocabulary
- **Raised Overlay** (`0 10px 28px rgb(23 26 31 / 0.08)`): Dropdowns, modals, and toasts only.
- **Command Hover** (`0 1px 2px rgb(23 26 31 / 0.08)`): Optional hover feedback for compact icon actions.

### Named Rules
**The Surface Honesty Rule.** A panel may have a border or a shadow, but not both as decoration. Most panels use only a border.

## 5. Components

### Role Shell Contract
- **Source of truth:** Admin and Student workspace tokens live in `wwwroot/css/role-shell.css`. Page-specific styles should consume `--rs-*` variables instead of hardcoded blue, purple, gray, radius, z-index, or font values.
- **Layout tokens:** Use `--rs-content-max`, `--rs-space-page-x`, `--rs-space-page-y`, and `--rs-hit-area` for page width, page padding, and touch targets.
- **Layering:** Use `--rs-z-overlay`, `--rs-z-sidebar`, and `--rs-z-modal` for drawers, overlays, and modal surfaces.
- **Accessibility:** Role layouts should include `.rs-skip-link` pointing to `#mainContent`.

### Public Auth Shell
- **Style:** Auth uses the same canvas, surface, ink, brass, border, radius, focus, and Noto Sans tokens as Admin, Teacher, and Student.
- **Structure:** `_AuthLayout` owns the public auth frame. Login and Register render only their card content and never declare a standalone HTML document.
- **Forms:** Inputs use visible labels, inline validation, browser autocomplete, 44px touch targets, password visibility controls, and submit loading state.
- **Visual restraint:** Auth must not use purple or blue gradients, dark mesh backgrounds, glass cards, floating decorative icons, social login buttons without backend support, emoji-led headings, or page-level scripts.

### Public Landing Shell
- **Style:** Public pages use the same Noto Sans, canvas, surface, ink, brass, border, radius, focus, and status vocabulary as Admin, Teacher, Student, and Auth.
- **Structure:** `_Layout` is a top-navigation public shell. It must not use a role sidebar, because public visitors are browsing, not managing a workspace.
- **Content:** Landing copy must describe real product flows for Student, Teacher, and Admin. It must not use fake metrics, fake testimonials, dead links, generic AI SaaS promises, or visual sections that cannot be verified in the app.
- **Assets:** Hero and role visuals should be real screenshots from the running product whenever possible. If screenshots are stale, recapture them before shipping.
- **Responsive:** Public navigation collapses below 768px into a keyboard-friendly menu with Escape support and no horizontal overflow.

### Buttons
- **Shape:** Controlled rectangular form with gently softened corners (8px).
- **Primary:** Charcoal fill, warm surface text, compact product padding (10px 16px).
- **Hover / Focus:** Deep brass hover; brass focus ring at 3px with sufficient contrast.
- **Secondary / Ghost:** Warm surface fill, stone border, charcoal text, archive tint hover.

### Chips
- **Style:** Tonal backgrounds with semantic ink. Use text labels, never color-only status.
- **State:** Selected chips use brass text and archive tint, not saturated fills.

### Cards / Containers
- **Corner Style:** Panel radius (12px).
- **Background:** Warm surface against ivory canvas.
- **Shadow Strategy:** Flat by default; raised overlay only for floating UI.
- **Border:** Stone line at 1px.
- **Internal Padding:** 16px for dense panels, 24px for dashboard panels.

### Inputs / Fields
- **Style:** Warm surface or ivory fill, stone border, 8px radius.
- **Focus:** Brass border and 3px brass ring.
- **Error / Disabled:** Error uses danger token with text support. Disabled controls reduce contrast without hiding labels.

### Navigation
- **Style:** Light ivory role sidebars with charcoal text. Active links use a warm surface and brass inset line. Section labels are muted and compact. Mobile drawers use the same palette and maintain 44px touch targets.

### Student Role Shell
- **Style:** Student uses the same role shell as Admin with a calmer density. The sidebar, mobile top bar, account strip, focus states, and active navigation must share the same canvas, surface, ink, brass, and border tokens.
- **Dashboard:** The daily Student dashboard prioritizes the next learning action, weekly study metrics, learning path progress, recent activity, and access to the learning assistant. It must avoid purple gradients, emoji-led greetings, heavy shadows, and generic AI-SaaS language.
- **Onboarding:** Student onboarding uses `.onboarding-shell` and the same role tokens. Selection cards, step progress, fields, and validation states should be defined in shared CSS instead of per-view `<style>` blocks.

### Teacher Role Shell
- **Style:** Teacher is a composed creation and review workspace with medium density. It uses Noto Sans and the same canvas, surface, ink, brass, border, focus, and status tokens as Admin and Student.
- **Structure:** `_TeacherLayout` composes focused sidebar, account, and modal partials. Teacher-specific presentation lives in `teacher-shell.css`; drawer and modal behavior live in dedicated JavaScript files instead of inline layout scripts.
- **Responsive:** Desktop keeps an independently scrolling sidebar and main content area. Below 1024px, navigation becomes an accessible drawer with an overlay, Escape support, 44px controls, and no horizontal document overflow.
- **Interactions:** AJAX modal behavior is opt-in through `data-teacher-modal`. Teacher pages must not infer behavior from URL fragments or rewrite action markup after render.
- **Visual restraint:** Teacher must not use purple as product identity, decorative gradients, glow, glass surfaces, animated accent bars, or page-level `<style>` blocks.

### Data Tables
- **Style:** Flat surface with archive-tint headers, charcoal labels, muted metadata, and row hover that does not shift layout.

## 6. Do's and Don'ts

### Do:
- **Do** use the same canvas, surface, ink, brass, border, and status tokens across Admin, Student, and Teacher.
- **Do** keep Admin dense and table-first, with compact rows and clear action grouping.
- **Do** preserve existing route structure, Razor sections, breadcrumbs, forms, and sidebar behavior.
- **Do** use brass for focus and active state, not for every icon or badge.
- **Do** keep motion to 150-220ms state feedback and respect reduced motion.

### Don't:
- **Don't** use generic AI SaaS gradients, purple glow systems, glassmorphism as decoration, fake metrics, fake testimonials, identical three-card feature grids, oversized rounded cards, or mixed icon styles.
- **Don't** make Admin, Student, and Teacher look like three separate products.
- **Don't** use blue or purple as the dominant identity color in role workspaces.
- **Don't** pair broad decorative shadows with borders on every panel.
- **Don't** hide state behind color alone; pair status colors with text or icons.
