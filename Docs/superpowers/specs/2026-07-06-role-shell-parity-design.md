# Admin and Student Role Shell Parity Design

## Goal

Make the Admin and Student workspace shell visually identical to the approved Teacher shell while preserving each role's menu, routes, labels, notifications, page content, and business behavior.

## Scope

The change covers only:

- shared colors, typography, borders, radii, focus states, spacing, and motion;
- desktop sidebar width, structure, navigation rhythm, active state, and logout area;
- desktop account header height, context block, notification control, profile identity, and avatar treatment;
- mobile top bar, drawer, overlay, Escape behavior, scroll containment, and responsive breakpoints.

Page bodies, forms, tables, dashboards, controllers, models, URLs, and role-specific navigation destinations are outside this change unless a shell wrapper must be adjusted to preserve layout.

## Architecture

`wwwroot/css/role-shell.css` becomes the source of truth for the shared shell contract. It will expose reusable `.rs-*` primitives for the frame, sidebar body/footer, brand, navigation sections and links, account strip, mobile top bar, drawer overlay, and profile identity.

Teacher remains the visual reference. `teacher-shell.css`, `student-shell.css`, and the Admin layout may keep role-specific page styles, but shell dimensions and visual state must consume the shared primitives instead of redefining competing values.

Student continues to compose `_StudentSidebar` and `_StudentAccountStrip`. Admin will extract its shell markup from the large `_AdminLayout.cshtml` into focused partials where needed, without moving unrelated Admin compatibility styles in this iteration.

## Shared Visual Contract

- Sidebar width: `17.25rem` on desktop.
- Account header minimum height: `4.5rem`.
- Font: Noto Sans with Arial and sans-serif fallbacks.
- Canvas: ivory `--rs-canvas`; surfaces: `--rs-surface`; muted surfaces: `--rs-surface-muted`.
- Text: charcoal `--rs-ink`; metadata: `--rs-ink-muted`.
- Borders: `--rs-line`; active/focus accent: brass tokens.
- Active navigation: warm surface, charcoal text, and brass inset line.
- Controls: shared `--rs-hit-area`, `--rs-radius-control`, focus ring, hover, and disabled behavior.
- Desktop sidebar and main content scroll independently without document-level horizontal overflow.

Role-specific labels remain different: Teacher uses teaching language, Student uses learning language, and Admin uses administration language.

## Responsive Behavior

At widths below `1024px`, all three roles use the same mobile top bar and off-canvas drawer behavior. The drawer uses the shared sidebar width up to the available viewport, a shared overlay and z-index scale, a visible menu control, Escape-to-close behavior, and body-scroll locking while open.

The mobile header keeps only the menu control, compact brand, and profile control. The desktop account strip is hidden at the mobile breakpoint. No shell element may cause horizontal document overflow at `390px`.

## Data and Interaction Flow

No application data flow changes. Existing role-specific claims, notification counts, profile links, active-route detection, and logout targets remain the source of displayed content.

Shell JavaScript may be shared or adapted behind explicit element IDs and classes. It may only control drawer visibility, accessibility attributes, overlay state, Escape handling, and sidebar scroll restoration. It must not rewrite navigation content or infer behavior from arbitrary URLs.

## Error and Compatibility Handling

- Missing user names fall back to the existing role labels and initials.
- Existing notification failures retain current service behavior; shell changes do not introduce new network requests.
- Admin utility and compatibility CSS remains available to page bodies.
- Student and Admin active-route logic remains unchanged except for class names required by shared primitives.
- Existing dirty workspace changes are preserved and unrelated files are not reformatted.

## Testing and Acceptance

Static UI contract tests will verify that Admin, Student, and Teacher use the shared dimensions, Noto Sans, token palette, active state, partial structure, and responsive drawer contract.

Browser verification will cover Admin and Student at desktop, tablet, and `390x844`, checking:

- identical header/sidebar geometry and colors to Teacher;
- correct role-specific menu content and exactly one active item;
- working notification/profile/logout destinations;
- independently scrolling sidebar and main content;
- working drawer, overlay, Escape behavior, and focus visibility;
- no horizontal overflow or console errors.

The application must build successfully, all existing UI tests must pass, and the local server must be left running for review.
