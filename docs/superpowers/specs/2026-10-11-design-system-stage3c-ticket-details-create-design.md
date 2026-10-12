# Design system — Stage 3c: Ticket details and create

**Date:** 2026-10-11
**Status:** Approved in chat (layout and create page checked as HTML mockups)
**Scope:** `/ticket/{id}` (`TicketDetails` and its sections) and
`/create-ticket`. Stage 3 decisions from the 3a spec apply (no heroes,
`PageHeader` path, translated lowercase `# headings`, single hue, usability
first). Times use the browser's zone (`BaseComponent.ToLocal`,
`IUserTimeZone`, 3b97723).

## `/ticket/{id}`

### Header

- `PageHeader` path `~/tickets/42`; `tickets` links to the role's list
  (`/tickets` for admin/developer, `/my-tickets` for users). The "back to
  tickets" card is removed.
- Actions on the right (they wrap under the title on phones):
  - Status transitions (only for the assignee, `CanManageTicketStatus`, same
    rules as now): the main forward action is `.btn-accent`, other forward
    actions are plain buttons, reject/reopen are danger ghost buttons.
  - "take" / "release" (replaces `AssignmentActions`, same rules).
  - "delete" (danger ghost, `CanManageTicket`) now opens a confirmation
    `Modal`; after deleting, navigate to the role's list.
- Under the header: `#42` mono 13 muted + `TypeTag StatusTag PriorityTag`,
  then the title (24px, semibold).
- Triage notice (admin/developer, priority not assessed): the Home style row
  with a 2px `accent` left border on `surface`; no amber, no pulse.

### Layout

Two columns on `lg`: content `minmax(0,1fr)` and a 300px properties panel
(`sticky top`). Below `lg` the panel comes right after the title block.

### Left column (sections split by `line`, headings `# …` mono 13 muted)

1. `# description` — sanitized rich text, no nested boxes.
2. `# steps to reproduce` — only when filled.
3. `# analytical note` — admin/developer only; ghost "edit", `.input-field`
   textarea, cancel/save; empty text muted.
4. `# work log` — `WorkLogsSection` restyled: total on the right of the
   heading (mono), rows with dashed `line` borders (date mono 13 muted,
   text, user, duration mono), add row with `.input-field` controls. Dates
   via `ToLocal`.
5. `# attachments` — `AttachmentsSection` restyled: rows with name, size,
   author, date (mono 13 muted), "+ file" ghost button. Same permissions.
6. `# activity` — merged feed (below) and the comment composer.

### Activity feed

- Events sorted by time, a mono time column (`HH:mm`; date shown on the
  first event of each day as a muted separator row):
  - **created** — system row `@author created · type · priority`.
  - **status change** — system row `@user status old → new`; rows backfilled
    from the audit log have no actor and render without `@user`.
  - **comment** — block: `line` border, 8px radius, `Avatar Sm`, login,
    date mono 13 muted, edit/delete ghost buttons (same permissions as now,
    inline `RichTextEditor` for edit). Internal comments: dashed border,
    `surface` background, `[internal]` muted tag; visible to admin/developer
    only.
- System rows are mono 13 muted with the user and the new status in `fg`;
  technical words (`created`, `status`) stay English (decision 5).
- Assignment changes are not stored as history, so they are not in the feed.
- Composer: `RichTextEditor` in a `line` box; footer with a
  `SegmentedControl` `public | internal` (admin/developer only) and
  `.btn-accent` "send" (disabled while empty or sending).
- Data: new `ITicketStatusHistoryRepository.GetByTicketAsync(int ticketId)`
  (ordered by `ChangedAt`, includes `ChangedBy`), exposed as
  `ITicketService.GetStatusHistoryAsync(int ticketId)`. A static `TicketActivityFeed.Build(ticket, history,
  includeInternal)` returns the ordered event list (record types for the
  three kinds).

### Properties panel (`surface`, `line`, 8px; rows label 13 muted → value, dashed separators)

- status (`StatusTag`), priority (select for admin/developer with the
  existing critical confirmation and "save priority" button shown only when
  changed or unassessed; `PriorityTag` otherwise), assignee (`Avatar Sm` +
  login, or "unassigned"), author, product, created (mono 13).
- start / due: `datetime-local` `.input-field` for admin/developer with
  "save dates" when changed; read-only mono text otherwise.
  **Time zone fix:** the inputs show the user's local time; on save the
  values are converted from the user's zone to UTC (`IUserTimeZone`), and
  stored UTC values are converted back for display. Due dates set by SLA
  (already UTC) and manual ones are then consistent.
- environment, version (mono) — only when filled.
- tags: colour dot + name, `×` for admin/developer; "+ tag" opens a small
  popover list of unassigned tags (replaces the wall of tag buttons). There
  is no shared popover component yet; use a simple local dropdown (button +
  absolutely positioned list, closes on pick, outside click or Esc).

### States

- Loading: static `surface` bars (no pulse skeleton cards).
- Not found: `EmptyState` with a link to the role's list.
- 30 s auto-refresh stays and keeps unsaved edits, as now.

### Components

- New: `TicketPropertiesPanel`, `TicketActivity` (feed + composer),
  `TicketStatusActions` (header buttons). Transition rules move into a
  static `TicketTransitions` (`For(status)` → ordered actions with key,
  target status and kind `primary | normal | danger`).
- Restyled: `WorkLogsSection`, `AttachmentsSection`.
- Removed: `AssignmentActions`; `TicketDetails.razor.css` if nothing in it is
  still used.
- `TicketDetails` stays the page that loads data and wires callbacks.

## `/create-ticket`

- `PageHeader` path `~/tickets/new`, subtitle `create.subtitle`.
- One form (no type-selection step). Two columns on `lg`: form and a 280px
  aside; aside hidden below `lg`.
- Type: `SegmentedControl` `bug | support | consultation | project`
  (translated) with the selected type's description under it. **No type is
  selected by default**; submitting without a type shows a localized
  validation message on the control.
- Fields: title*, description* (`RichTextEditor`), product (required for all
  types except project, hidden for project, as now), priority* (admin/
  developer only; critical shows the 1px `danger` confirmation box with the
  checkbox, as now). Labels 13 muted, `*` in `danger`, "(optional)" muted.
- Bug only: group `# bug details` with steps to reproduce, environment,
  version.
- Footer: ghost "cancel" (navigates to `/` as now) and
  `.btn-accent` "create ticket" with the saving state.
- Aside: preview card `preview · #new` with type/status/priority tags, the
  title as typed, author, product, version; under it two or three muted
  `> ` tips (new localization keys).
- After creating, navigate to `/ticket/{id}` of the new ticket.
- Hard-coded strings move to localization keys: validation messages in
  `TicketCreateModel`, "Please select a product", "Змінити" (removed with the
  type step).

## Localization

New keys in en/uk/de/es/fr for: activity feed words that are translated
(day separators use the UI culture), section headings, delete confirmation,
create page tips, type-required and validation messages, "+ tag", "+ file",
panel labels that do not have keys yet.

## Testing

- Unit: `TicketTransitions` (actions per status); `TicketActivityFeed`
  (merge order, internal filtering, actor-less status rows, created event);
  zone conversion for start/due (local → UTC on save, UTC → local on load).
- Repository (InMemory): `GetByTicketAsync` order and `ChangedBy` include.
- bUnit: details page without `page-hero`/gradients, path header link per
  role; status buttons only for the assignee; delete opens a `Modal` and
  navigates after confirm; panel editing only for admin/developer; feed
  renders system rows and comments, internal hidden for users.
  Create page: no submit without type, bug group only for bug, product
  hidden for project, priority only for admin/developer, critical
  confirmation, redirect to `/ticket/{id}`.
- Build 0 warnings, all tests, `wwwroot/css/app.css` regenerated.
- Manual: both themes, uk + en, User / Developer / Admin, 375px and 1440px.

Out of scope: storing assignment history; per-user time zones beyond the
browser zone; calendar/audit/statistics time conversion (3e–3f).
