# Design system — Stage 3a: Home and Profile ("showcase" pages)

**Date:** 2026-10-11
**Status:** Approved in chat
**Scope:** First part of Stage 3 (pages). Two pages done as showcase
versions: Home (`/`) and Profile (`/profile`), plus the backend they need.
Branch `redesign/stage-1`; merge to `master` after the whole of Stage 3.

## Stage 3 decisions (2026-10-11, apply to all Stage 3 parts)

1. No gradient heroes. Every page starts with `PageHeader` (route path,
   subtitle, actions on the right).
2. Ticket detail header path `~/tickets/42` links back to the role's list.
3. Tables / Kanban: terminal style, but usability first.
4. Dashboard numbers: big mono figures with a `# label`, no icon tiles;
   accent on one key element only.
5. Section headings (`# …`) are translated and lowercased; paths and
   technical labels (`~/tickets`, `ok`, `err`) stay English.
6. Charts: single-hue (accent tints + neutral, `danger` for breaches);
   multi-hue only where categories really need it.
7. Merge to `master` after all of Stage 3.

## Part 1 — Backend: durable status history + user activity

### Why

The profile activity map counts four kinds of actions. Ticket creation,
comments and work logs are stored permanently, but status changes exist
only in the audit log, which an admin can clear — the map would change
after a cleanup. Status changes get their own table.

### `TicketStatusHistory` (Domain entity + EF config + migration)

- Fields: `Id`, `TicketId` (FK, cascade), `OldStatus`, `NewStatus`
  (`TicketStatus`, stored with the same string converter as
  `Ticket.Status`), `ChangedByUserId` (nullable FK → `Users`, restrict),
  `ChangedAt` (UTC). Indexes on `TicketId` and `(ChangedByUserId, ChangedAt)`.
- Written by a new MediatR handler `TicketStatusHistoryEventHandler` for
  `TicketStatusChangedEvent` (alongside the existing audit/notification
  handler). Failures are logged, not thrown (the ticket is already saved).
- Repository `ITicketStatusHistoryRepository` on the facade.
- Migration `AddTicketStatusHistory` creates the table and backfills it once
  from `AuditLogs` rows with `Action = 'STATUS_UPDATE'`, `EntityName =
  'Ticket'` and `Changes` = `Status changed from X to Y` (enum names mapped
  to the stored strings, e.g. `InProgress` → `In Progress`), only for tickets
  that still exist. Backfilled rows get `ChangedByUserId = NULL`: until this
  change `UpdateTicketStatusAsync` recorded the assigned developer as the
  actor, so the audit log's user is unreliable.
- `ITicketService.UpdateTicketStatusAsync(ticketId, status, actorUserId)`
  takes the acting user explicitly (Blazor pages pass the current user, the
  API passes the token's user). This also fixes status notifications naming
  the developer instead of the person who changed the status.
- The audit log stays as is (clearable admin journal).

### `IUserActivityService` (Application/Services/Profile)

- `GetDailyActivityAsync(int userId, DateOnly from, DateOnly to)` →
  `IReadOnlyList<DailyActivityDto>` with only active days:
  `Date, TicketsCreated, Comments, StatusChanges, WorkLogs, WorkMinutes`
  and `Total` (= the four counts; every action weighs 1).
- Sources: `Ticket.CreatedAt` (author), `Comment.CreatedAt` (author),
  `TicketStatusHistory.ChangedAt` (changed by), `WorkLog.DateLogged` (user).
  All timestamps, including `DateLogged` (stored as UTC of the picked local
  date), are converted to local time before taking the date. "Local" is the
  server time zone, which matches the users' zone in this deployment.
- New repository methods return timestamps since a date for one user.
- `IWorkLogService.GetTotalTimeSpentForUserAsync(int userId)` (passthrough to
  the existing repository method) for the profile "hours logged" figure.

## Part 2 — Pages

### Shared

- `.cursor-blink` utility in `input.css` (moved from `Logo.razor.css`, same
  1.2s `step-end`, off under reduced motion); `Logo` uses it.
- `AvatarSize.Xl` (64px, text 20).
- `DashboardStats` restyled (also shown on the ticket list): four mono
  figures (`text-40`), `# label`, a 2px bar with the share of total
  (`fg`; critical uses `danger` and is shown in `danger` only when > 0),
  count-up animation kept. No icon tiles, gradients, shadows.

### Home (`/`)

- `PageHeader` path `~`; actions: `.btn-accent` "new ticket" and
  `.btn-ghost` "tickets" (role-aware: `/tickets` or `/my-tickets`).
- Greeting `{welcomeBack} {name}` 20px with a blinking `_` cursor in
  `accent-ink` (`.cursor-blink`), subtitle 13px `muted`.
- `# overview` → `DashboardStats` (admin/developer only, as now).
- Triage notice (admin/developer, when unassessed > 0): row with a 2px
  `accent` left border, text 15px, link "open queue →"; no amber, no pulse.
- `# recent activity` + "view all →": log rows separated by dashed `line`
  borders, whole row is a link (hover `canvas`): `#id` mono muted, title,
  `TypeTag StatusTag PriorityTag`, custom tags as color dot + name (13px),
  `[triage]` in `danger` when unassessed (admin/dev), date mono 13px muted
  (`dd MMM yyyy, HH:mm` in the UI culture). Empty: `EmptyState`. Loading:
  three `surface` bars.

### Profile (`/profile`)

Two columns on `lg` (left 280px card, right form); stacked below.

- `PageHeader` path `profile`; status on the right: `[ok]` mark +
  "saved" after a successful save.
- **Identity card** (`surface`, `line`, 8px): `Avatar Xl`, full name 20px
  (updates live while typing), `@login` mono 13px muted, `RoleTag`, primary
  contact (type + value, 13px).
- **# in numbers** (under the card): mono figures by role —
  User: created / open / resolved; Developer & Admin: assigned / in progress
  / completed / hours logged.
- **# activity** (full width under both columns): `ActivityHeatmap` —
  26 weeks × 7 days ending today, weeks as columns starting Monday, month
  labels above, Mon/Wed/Fri labels on the left (UI culture). Cell 12px,
  gap 3px, radius 2px. Level 0 = `line`; levels 1–4 = `accent` at 25/50/75/100%
  (relative to the busiest day). Each cell has a `title` with the breakdown
  (`12 Oct · 7 actions — tickets 2, comments 3, status 1, work logs 1 (1h 30m)`).
  Summary line `N actions in 6 months` and a less→more legend. Grid is
  `role="img"` with the summary as `aria-label`; cells `aria-hidden`.
- **# recent tickets**: 5 rows in the Home log style (User: authored;
  Developer/Admin: assigned).
- **Form**: `# personal details` (first/last/middle name, bio) and
  `# contacts` (rows: type select + value + `×` remove, "+ add contact"
  ghost button), `.input-field` controls, labels 13px muted, warnings/errors
  as 1px `danger` border rows (`role="alert"`), submit `.btn-accent`
  (saving text). Loading: `surface` skeleton bars.
- Hard-coded Ukrainian fallbacks (`?? "…"`) and error strings move to
  localization keys.

### Settings

Section headings use translated keys (decision 5) instead of literal English.

## Localization

New keys in en/uk/de/es/fr: home/dashboard (`home.overview`,
`dashboard.ofTotal`, …), profile (`profile.personal`, `profile.numbers`,
`profile.activity*`, `profile.recentTickets`, `profile.stat.*`,
`profile.error.*`), settings section headings.

## Testing & verification

- Unit: `UserActivityService` (merges four sources by day, local date,
  range filter, totals); status-history handler writes a row and swallows
  repository errors; `WorkLogService.GetTotalTimeSpentForUserAsync`.
- Repository/EF (InMemory): status history converter round-trip and the
  per-user timestamp queries.
- bUnit: `ActivityHeatmap` (cell count, levels, tooltip, empty state),
  Home (greeting cursor, no `page-hero`, role-aware links, triage row),
  Profile (card updates from the form, numbers per role, no gradients),
  `DashboardStats` (no svg/gradients, critical danger only when > 0).
- Migration reviewed by hand (generated SQL for the backfill).
- Build 0 warnings; all tests; `wwwroot/css/app.css` regenerated.
- Manual: both themes, uk + en, User / Developer / Admin, 375px and 1440px.
