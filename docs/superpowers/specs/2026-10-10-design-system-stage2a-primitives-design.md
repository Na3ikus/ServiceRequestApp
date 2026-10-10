# Design system — Stage 2a: Primitives

**Date:** 2026-10-10
**Status:** Draft — awaiting review
**Scope:** Stage 2a of the "ops terminal" (B2) redesign of ServiceDeskSystem.
**Builds on:** `docs/superpowers/specs/2026-10-09-design-system-stage1-design.md`
(tokens, fonts, `.mark`, Settings controls). Work continues on branch
`redesign/stage-1`; nothing is merged to `master` until the app looks coherent.

## Goal

Stage 2 (shell & core components) is split bottom-up — small building blocks
first, the shell last — so the shell is later assembled from finished,
tested parts:

- **2a — Primitives** (this document): tags, avatar, kbd, page header,
  panel, empty state; replace the `BaseComponent` color helpers; delete
  `ticket-status.css`.
- **2b — Composite components:** Modal, Toast, CommandPalette,
  HotkeysModal, Breadcrumb, LanguageDropdown.
- **2c — Shell:** NavMenu (`sd_`, lowercase mono nav with `›`), MainLayout
  and top bar, Login/Register, ReconnectModal.

After 2a: ticket status / priority / type and user avatars look the same
everywhere and follow the token rules; no Tailwind color helpers or
gradient avatars remain; the remaining primitives exist and are tested,
ready for 2b/2c and Stage 3.

**Not in 2a:** page heroes, gradient buttons, statistics cards, page layouts
(Stage 3); everything listed under 2b and 2c. `PageHeader`, `Panel` and
`EmptyState` are adopted only on Settings in 2a; other pages adopt them in
Stage 3.

## Visual decisions (approved, mockups in `.superpowers/brainstorm/1235-1791624435/content/`)

### Tags — outline chip (`tags-style.html`, option C)

Geist Mono 13px, 1px border, radius 6px, padding `1px 7px`, no fill.
Default border is `line`, so the table stays quiet; only attention states
get a contrasting border.

| Tone | Dark theme | Light theme |
|---|---|---|
| `Accent` | border + text `accent` | border `#000`, text on accent fill (`.mark`) |
| `Danger` | border + text `danger` | same |
| `Active` | border `line`, text `fg` | same |
| `Muted` | border `line`, text `muted` | same |

Tag text is **translated** (existing keys `status.*`, `priority.*`,
`ticketType.*`), not raw tokens.

Tone mapping (`TicketTones`):

| Tone | Status | Priority | Type |
|---|---|---|---|
| Accent | `New` | — | — |
| Danger | — | `Critical` | — |
| Active | `Open`, `InProgress`, `Testing`, `CodeReview` | `High` | `Bug` |
| Muted | `Resolved`, `Done`, `Closed` | `Medium`, `Low` | `Support`, `Project`, `Consultation` |

### Avatar — square with border (`avatar-style.html`, option A)

Square, radius 6px, background `surface`, 1px border `line`, initials in
Geist Mono 500, color `fg`. Monochrome for every user. Role is shown as a
text tag (`RoleTag`), never as avatar color.

### PageHeader and Panel (`page-primitives.html`)

- **PageHeader:** route path in Geist Mono 500 20px, `~/` muted; path
  segments with a link are clickable (muted, dashed underline) and replace
  the Breadcrumb; actions row on the right; optional subtitle in 13px muted
  below.
- **Panel:** 1px border `line`, radius 8px, background `surface`; optional
  header row (`# title` in Geist Mono 13px muted, meta on the right,
  bottom border `line`); body padding `12px 14px`.

### EmptyState — terminal line (option A)

One mono 13px muted line, left-aligned: `> {message} — {action}` with an
optional `Kbd` hint, e.g. `> no tickets yet — create one [N]`. Message and
action text are translated; `> ` and the key are literal.

## Components

All in `ServiceDeskSystem/Components/UI/Primitives/` (namespace
`ServiceDeskSystem.Components.UI.Primitives`, added to `_Imports.razor`).
Token classes only; no gradients, blur or shadows.

| Component | Parameters | Notes |
|---|---|---|
| `Tone` (enum) | `Accent`, `Danger`, `Active`, `Muted` | |
| `Tag` | `Tone Tone = Muted`, `RenderFragment ChildContent`, `string? Title` | renders `<span>`; class per tone |
| `TicketTones` (static class) | `Tone For(TicketStatus)`, `Tone For(TicketPriority)`, `Tone For(TicketType)` | single source of the mapping above; exhaustive `switch` with a throwing default |
| `StatusTag` | `TicketStatus Status` | `Tag` + `L.Translate("status.*")` |
| `PriorityTag` | `TicketPriority Priority` | `Tag` + `priority.*` |
| `TypeTag` | `TicketType Type` | `Tag` + `ticketType.*` |
| `RoleTag` | `UserRole Role` | `Tag` (Muted) + existing `admin.roleAdmin` / `admin.roleDeveloper` / `admin.roleUser` |
| `Avatar` | `string? Name`, `AvatarSize Size = Md` (`Sm` 24px / `Md` 28px / `Lg` 40px), `bool ShowTitle` | initials rule below; `aria-hidden="true"` unless `ShowTitle`, then `title`=Name and `role="img"` + `aria-label`=Name |
| `Kbd` | `RenderFragment ChildContent` | `<kbd>`, Geist Mono 12px, 1px border `line`, radius 4px, padding `0 5px`, text `muted` |
| `PageHeader` | `IReadOnlyList<PathSegment> Segments`, `string? Subtitle`, `RenderFragment? Actions` | `PathSegment(string Text, string? Href = null)`; segments joined by muted `/`; last segment gets `aria-current="page"`; heading is `<h1>` |
| `Panel` | `string? Title`, `RenderFragment? Meta`, `RenderFragment ChildContent`, `string? Class` | header row only when `Title` or `Meta` is set |
| `EmptyState` | `string Message`, `string? ActionText`, `EventCallback OnAction`, `string? Hotkey` | action is a `<button>` styled as text (`fg`, underline on hover); no button when `ActionText` is null |

**Avatar initials:** split `Name` on whitespace, `.`, `_`, `-`; two or more
parts → first letter of the first two parts; one part → its first two
letters; uppercase with the invariant culture; empty or null → `?`.
Examples: `Nazar Hutsol` → `NH`, `admin` → `AD`, `Олена Коваль` → `ОК`,
`j.doe` → `JD`.

**Moved from Settings:** `SegmentedControl`, `ToggleSwitch`, `SettingRow`
move from `Components/Pages/Settings/Controls/` to
`Components/UI/Primitives/` (namespace updated; behavior unchanged).
`AccentPicker` stays in Settings (single use).

## Page changes

Only tags, avatars and helper-driven color are replaced. Page heroes,
gradient buttons and statistics cards stay until Stage 3.

| File | Change |
|---|---|
| `Features/Tickets/TicketTable.razor` | 40px gradient priority circle → ticket id `#1042` in Geist Mono 13px `muted`, fixed width; type badge → `TypeTag`; status dot + text → `StatusTag`; priority → `PriorityTag` |
| `Pages/Index.razor` | same as TicketTable for its ticket list |
| `Features/Kanban/KanbanCard.razor` | type, priority → tags; both gradient avatars (assignee / unassigned) → `Avatar Size=Sm ShowTitle` |
| `Pages/Tickets/TicketDetails.razor` | header, mobile and side-panel badges → `StatusTag` / `PriorityTag` / `TypeTag` (incl. `.ticket-priority-badge` site); author avatar and comment avatars → `Avatar`; internal comment marked with `Tag` (Muted) text `L.Translate("details.internalNote")` instead of an amber avatar |
| `Pages/Tickets/WorkLogsSection.razor` | gradient avatar → `Avatar Size=Sm` |
| `Layout/MainLayout.razor` | user-menu gradient avatar → `Avatar Size=Lg` (only shell change in 2a) |
| `Pages/Admin/Components/UsersTab.razor(.cs)` | `avatar-admin/dev/user` role classes → `Avatar` + `RoleTag` |
| `Pages/Statistics/StatisticsPage.razor` | type in the distribution table → `TypeTag` |
| `Pages/Settings/Settings.razor` | `~/settings` heading → `PageHeader`; controls from the new namespace |

**Deleted:**

- From `BaseComponent`: `GetStatusBadgeClass`, `GetPriorityBadgeClass`,
  `GetTicketTypeBadgeClass`, `GetPriorityColorClass`, `GetStatusDotColor`,
  `GetAvatarGradient`. `GetStatusText` / `GetPriorityText` /
  `GetTicketTypeText` are removed too if no caller remains after the tag
  replacement (grep decides); otherwise they stay.
- `wwwroot/css/ticket-status.css` and its `<link>` in `App.razor`.
- CSS rules `.avatar-admin`, `.avatar-dev`, `.avatar-user`,
  `.status-badge*`, `.priority-badge*`, `.ticket-priority-badge` in
  `wwwroot/app.css` and page `.razor.css` / `Styles/*.css` files — each
  only after a grep shows no remaining usage.

## Testing & verification

- **Unit (NUnit):** `TicketTones` — one `[TestCase]` per enum value of
  `TicketStatus`, `TicketPriority`, `TicketType` asserting the table above;
  plus a test that iterates `Enum.GetValues` for each enum and asserts
  `For` does not throw (catches new enum members without a tone).
- **Component (bUnit):**
  - `StatusTag` / `PriorityTag` / `TypeTag` render the translated key and
    the tone's class (`New` → accent class, `Critical` → danger class).
  - `RoleTag` renders the role key.
  - `Avatar` initials for `Nazar Hutsol`, `admin`, `Олена Коваль`,
    `j.doe`, empty, null; size classes; `ShowTitle` adds `title` and
    `aria-label`, otherwise `aria-hidden="true"`.
  - `Kbd` renders `<kbd>`.
  - `PageHeader`: linked segments render `<a href>`, others plain text;
    last segment has `aria-current="page"`; subtitle and actions render.
  - `Panel`: header row only with `Title`/`Meta`.
  - `EmptyState`: action button invokes `OnAction`; no button without
    `ActionText`; hotkey renders as `Kbd`.
  - Existing `SettingsControlsTests` / `SettingsPageTests` updated for the
    new namespace and still pass; `SettingsPageTests` asserts the heading
    comes from `PageHeader`.
- **Grep checks:** no `GetStatusBadgeClass|GetPriorityBadgeClass|
  GetTicketTypeBadgeClass|GetPriorityColorClass|GetStatusDotColor|
  GetAvatarGradient|ticket-status.css|avatar-(admin|dev|user)` left in
  `ServiceDeskSystem/`.
- **Build:** solution builds with 0 warnings; `npm run test:js` still passes.
- **Manual in browser** (both themes, `uk` + `en`): tickets table, my
  tickets, home, kanban, ticket details (incl. internal comment), admin
  users tab, statistics, settings; tickets table at 375px width (chips must
  wrap, not overflow); pages listed by the `ticket-status.css` audit below.

## Risks

- **Deleting `ticket-status.css` recolors other elements.** It overrides
  stock Tailwind classes (`bg-purple-200`, `bg-blue-200`, `bg-yellow-200`,
  `text-*-900`, …) globally with `!important`. Before deleting, grep every
  class it overrides across `Components/**` and list the files that use
  them outside the replaced badges; those pages go on the manual check
  list. Colors there revert to stock Tailwind — acceptable until Stage 3.
- **Mixed look until Stage 3.** New tags sit inside old heroes and cards.
  Expected; 2b/2c/3 follow on the same branch before any merge.
- **Wider chips in dense rows.** Outline chips are wider than a dot +
  text; the ticket table must wrap the badge row on narrow screens.
