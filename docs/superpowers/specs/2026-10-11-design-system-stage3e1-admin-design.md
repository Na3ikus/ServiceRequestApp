# Design system — Stage 3e-1: Admin

**Date:** 2026-10-11
**Status:** Approved in chat (HTML mockups `admin-nav`, `admin-edit`, `admin-sections`)
**Scope:** The `/admin` page and its six sections. Audit logs are Stage 3e-2;
the legacy `app.css` rules (`atab-*`, `amodal-*`) are removed in Stage 3g.
Branch `redesign/stage-1`. The Stage 3 decisions in the 3a spec apply.

## Goals

- Same "ops terminal" look as the rest of the app: no hero, no glow, no
  icon tiles, no second sidebar.
- Each section has its own URL.
- Edit records in the table row instead of a modal.
- Nothing is deleted without a confirmation (today all five deletes run on
  the first click).
- Fix: a failed role change in Users no longer shows the unsaved role.

## Page and navigation

- Routes: `@page "/admin"` and `@page "/admin/{Section}"`. Sections:
  `products`, `techstacks`, `users`, `tags`, `sla`, `smtp`. `/admin` and an
  unknown section show `products`. The sidebar item `admin` already matches
  by prefix, so it stays active on every section.
- Header: `PageHeader` with path `~/admin/<section>` (`admin` links to
  `/admin`) and the `admin.subtitle` text. Non-admins keep the current
  "access denied" text, restyled as a plain `EmptyState`.
- Tabs: one row of mono links under the header, labelled with the section
  names in English (`products 4 · techstacks 3 · users 12 · tags 6 · sla 5 ·
  smtp ●`). Counts are small and muted. The active tab is `text-fg` with a
  2 px accent underline and `aria-current="page"`. The row scrolls
  horizontally on narrow screens. `smtp` shows a dot instead of a count:
  green ok, `danger` failed, muted unknown/checking.
- Data loading stays as it is (`Admin` loads all five lists once, so the
  tab counts are known). Remove `ConfigureAwait(false)` from the component
  code: continuations must stay on the renderer's dispatcher.
- Delete `Admin.razor.css` (488 lines) together with the hero, the
  sub-sidebar and the mobile nav toggle.

## Shared parts (`Components/Pages/Admin/Components`)

- **`AdminToolbar`:** a search input (left) and an `Actions` slot (right).
  Placeholder `search.label…`.
- **`SortHeader`:** a `th` with a button. It shows `↑`/`↓` on the active
  column and sets `aria-sort` on the `th`, like `TicketTable`.
- **`EditRow`:** a `tr` with one `td colspan=N`, a surface background and a
  2 px accent bar on the left. It takes `ChildContent` (the fields, laid out
  by the caller in a grid), `IsSaving`, `Error`, `OnSave` and `OnCancel`.
  Footer: hint `Enter save · Esc cancel` on the left, `cancel` and
  `save` (accent) on the right. `Enter` in a single-line input saves and
  `Esc` anywhere in the row cancels. The first field gets focus when the row
  opens. A failed save keeps the row open with the values and shows `Error`
  in `danger` above the buttons.
- **`DeletePrompt`:** a `tr` with one `td colspan=N`, a light `danger` tint.
  Text: `rm <kind> "<name>"?` (mono, `rm` in `danger`), then a muted
  translated note ("this cannot be undone" plus an optional detail), then
  `y, delete` (danger outline) and `N`. `N` gets focus when the prompt
  opens, so a stray `Enter` is safe; `y` and `n`/`Esc` keys work while the
  prompt has focus. The service is called only after `y`.
- **Row state per section:** at most one row is open at a time, either in
  edit or in the delete prompt. Opening another row, or `+ <kind>`, closes
  the current one and drops its unsaved changes. `+ <kind>` opens an empty
  `EditRow` at the top of the table.
- Empty and loading states: `EmptyState` with the existing texts and the
  "clear search" button. Loading: muted mono `loading…`.
- Tables: header row with a solid line, rows with dashed lines, row hover
  `bg-surface`, numbers right-aligned in mono. Each table sits in an
  `overflow-x-auto` wrapper. Edit grids collapse to one column below `md`.
- Successful saves and deletes keep their current success toasts. Errors
  show in the row (edit) or as a toast (delete, toggles).

## Sections

### products

Columns: name (sortable), description (muted, truncated), version (mono,
sortable), stack (`Tag`, sortable), tickets (mono, right), actions
(`edit`, `×`). Edit row: name, description, version (mono), stack
(`select`). `×` is disabled with the existing "cannot delete a product with
tickets" title when tickets > 0. Prompt kind: `product`.

### techstacks

Columns: name (sortable), type (`Tag`, sortable), products (mono, right,
sortable), actions. The progress bar is dropped. Edit row: name, type.
`×` disabled when products > 0 (existing title). Prompt kind: `techstack`.

### users

Columns: user (`Avatar`, full name, muted `@login`; sorted by last name as
today), role, active, actions. Search matches login, first and last name.

- Role: `SegmentedControl` `admin / dev / user`, saved on change. When the
  save fails, the control returns to the previous role and the row shows
  `✗ not saved` in `danger` until the next change. Your own row shows a
  `RoleTag` instead (you cannot change your own role, as today).
- Active: `ToggleSwitch`. Inactive users are dimmed. Admins' switch is
  disabled (as today).
- `×` for non-admins only; admins show a muted `—`. Prompt kind: `user`.
  The "user has tickets" error stays a toast.
- No edit row and no `+`: users register through `/register`. The toolbar
  says so in a muted mono note.

### tags

Columns: tag (a chip with the colour dot and name, sortable), colour (mono
hex), tickets (mono, right, sortable), actions. Edit row: name, colour
(native colour input + hex text input), the existing preset swatches as a
row of small round buttons (the picked one has an accent ring), and a live
chip preview. Prompt kind: `tag`; the note mentions the number of tickets
that lose the tag.

### sla

Toolbar actions: `recalculate` (outline, busy text while running, existing
hint as title) and `+ policy` (accent).

Columns: policy (name, `escalates` danger tag when set), priority
(`PriorityTag`, sortable), product (muted, "all products" fallback),
response, resolution, warn at (mono, right), active (`ToggleSwitch`, saves
on change), actions. Inactive policies are dimmed.

Edit row in two grid rows: (1) name, description, priority, product;
(2) response minutes and resolution minutes with the formatted value in the
label (`= 1h`), the warning threshold slider with its percent, and
`ToggleSwitch`es for escalate and active. Prompt kind: `policy`.

### smtp

Two columns (one below `md`):

- `# connection`: a terminal-style log (surface box, mono 13 px). Each check
  adds `$ smtp check` and an `ok: …` (green) or `err: …` (`danger`) line;
  each test email adds `$ smtp send <to> "<subject>"` and its result. The
  log lives in `Admin` for the time the page is open, newest at the bottom,
  with a blinking accent cursor line `$ _`; the automatic check on page load
  is the first entry. Button: `check connection`.
- `# test email`: recipient and subject inputs and `send` (accent).

The log replaces the coloured message boxes and the check/send toasts.

## Localization

New keys in all five languages (en, uk, fr, es, de): the delete note
("this cannot be undone"), the users note about `/register`, `not saved`,
the `EditRow` hint, `check connection` and `send`. Section names, `rm`,
the `$ smtp …` commands and the log's `ok:`/`err:` prefixes stay English
(Stage 3 decision 5); the text after the prefix is translated. Unused keys of the old hero and sub-sidebar
are removed.

## Out of scope

- Audit logs (3e-2), statistics (3f), removing legacy CSS (3g).
- Modal focus trap (issue #2): the admin page no longer uses `Modal`.
- Editing users' names or passwords from the admin page.

## Testing

bUnit, replacing the markup checks in `SlaTabTests`:

- Routing: `/admin/users` renders users; `/admin` and `/admin/nope`
  render products; the active tab has `aria-current`; non-admins see the
  access-denied state and no tabs.
- `EditRow`: opens with the record's values; `Esc` and `cancel` close it
  without calling the service; `Enter` and `save` call it; a failed save
  keeps the row and shows the error; opening another row closes the first.
- `DeletePrompt`: `×` shows the prompt and does not call the service; `N`
  closes it; `y` calls the service once; `×` is disabled when the product
  has tickets or the stack has products.
- Users: a role change calls the service; a failed change reverts the
  control and shows `not saved`; your own row has no control; admins have
  no `×` and a disabled switch.
- SLA: the active switch and `recalculate` call the service; durations in
  compact form; the escalation tag only on escalating policies; search by
  name, priority and product (existing cases).
- Tags: picking a preset updates the hex input and the preview.
- SMTP: the page-load check is the first log entry; a failed send adds an
  `err:` line.

Then a browser check of all six sections in light and dark themes and at
phone width.
