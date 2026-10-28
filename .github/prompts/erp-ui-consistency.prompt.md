---
name: ERP UI Consistency
description: "Apply and maintain the user's visual and interaction standards across this ERP project's UI."
argument-hint: "Describe the page or UI change to implement"
agent: agent
---

Apply the requested UI change in this ASP.NET Core ERP project while following these project-wide requirements.

## Source of Truth

- Treat `wwwroot/css/styles.css` as the sole authoritative application stylesheet. Both shared layouts should load it. Do not introduce competing theme files or duplicate its global rules in inline styles.
- Follow its existing tokens, components, typography, spacing, responsive behavior, and interaction states. The intended visual direction is elegant, professional, polished, and noticeable without sacrificing the dense, practical workflows of an ERP.
- Use Poppins for body text and Ubuntu for headings. Keep the overall typography compact; the global base font size is 14px, with clear hierarchy for headings, labels, data, and controls.
- Keep the authenticated sidebar in light mode, fixed to the viewport on desktop, and independently scrollable. Preserve a visible, accessible sidebar toggle in the sticky navbar. Keep mobile/off-canvas behavior usable.

## Overview Pages and Dashboard

- Keep exactly four KPI cards per row on desktop across Dashboard, Sales, Purchases, Inventory, Accounts, and Payroll overview pages. Preserve responsive two-column/mobile layouts. KPI cards across modules should share consistent dimensions, readable contrast, typography, spacing, icons, and hover behavior.
- Dashboard indicators and charts should use real data from existing modules, remain dynamically refreshable, and avoid placeholder metrics. Keep chart titles, subtitles, tooltips, and legends readable and aligned.

## Navigation and Payroll Settings

- Use the app's existing sidebar tree/dropdown pattern for nested module links. Payroll Settings should be reachable from the Payroll dropdown, show an expanded/active state when selected, and expose its configuration sections as dropdown links rather than a horizontal tab bar.
- Keep the section selector consistent with the app's dropdown styling and clearly indicate the active section. Preserve the current `?tab=` navigation, selected holiday year, and each section's existing save/delete behavior and return destination.

## Tables and Controls

- Keep all six DataTables actions visually distinct: Copy (amber), CSV (cyan), Excel (green), PDF (red), Print (indigo), and Columns (berry). Keep the column-visibility collection and its items legible in normal, hover, active, focus, and pressed states; never pair white text with a white background.
- Style Select2 with the same control dimensions, typography, border, focus, dropdown, and option colors as the project. Its arrow must remain at the right edge of single-select fields, including on Products filters.
- Keep intl-tel-input flags, dial codes, phone inputs, country dropdown, search, and validation feedback aligned with the same form-control system.
- Align Products toolbar actions and filters intentionally: actions form a clear row; filters and clear-filter control form a balanced grid on desktop and reflow cleanly on small screens. Style its Import dropdown to match the app's menus and support long labels.
- Style every file upload consistently, including avatar/profile images and CSV imports. Constrain table profile/avatar images to fixed dimensions with `object-fit: cover`; a source image must never expand a row or page.

## Implementation Discipline

- Inspect the current target files and nearby implementation before editing; preserve user changes and existing behavior. Prefer shared, scoped CSS for cross-page consistency and avoid unnecessary markup duplication.
- Keep selectors scoped to the owning component when possible. Verify other dropdowns, active/hover states, and responsive behavior when changing shared rules.
- After editing, run focused diagnostics and a project build when available. Report any existing warnings or validation limitations; do not claim visual verification unless the affected page was actually rendered and checked.

Do not include or perform database model-schema export/reuse work as part of this prompt unless explicitly requested separately.
