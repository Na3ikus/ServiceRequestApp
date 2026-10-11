# Design system — Stage 3b: Ticket lists

**Date:** 2026-10-11
**Status:** Approved in chat
**Scope:** `/tickets`, `/my-tickets`, `/developer` and the shared list
components (`TicketTable`, `KanbanBoard`, `KanbanCard`, new `TicketFilters`).
Stage 3 decisions from the 3a spec apply (no heroes, translated lowercase
`# headings`, single hue, usability first). Times use the browser's zone
(`BaseComponent.ToLocal`, 3b97723).

## Pages

- **`/tickets`** (admin/developer): `PageHeader` `~/tickets`; actions
  `SegmentedControl` `table | kanban` (remembered in `settings.defaultView`
  as now) and `.btn-accent` "new ticket". Then `# overview` → `DashboardStats`,
  the filter bar and the list. SignalR refresh stays.
- **`/my-tickets`** (user): `~/my-tickets`, "new ticket", filter bar, table
  (no Kanban, no author column). 30 s refresh stays.
- **`/developer`**: `~/developer`, view switch + "new ticket"; the old
  gradient figure cards become the same mono figures as `DashboardStats`
  (assigned / in progress / completed) through a shared `StatFigure`
  element that `DashboardStats` also uses. No assignee column.

## `TicketTable`

- Real `<table>`: `#`, title, type, status, priority, author, assignee,
  created. `ShowAuthor` / `ShowAssignee` keep hiding columns.
- Sort by clicking a header button (`aria-sort` on the `th`, `↑`/`↓`);
  default created, newest first; clicking the active column flips it.
  Priority/status sort by enum order.
- Title cell is a real link (`/ticket/{id}`); the whole row is clickable.
  Under the title: custom tags as colour dot + name, and the translated
  triage mark (`[критичність не виставлено]`, `danger`) for admin/developer.
- Sticky header, row padding from the density setting (`--row-py`),
  dashed `line` row borders, hover `surface`. Date mono 13px muted,
  `dd MMM yyyy, HH:mm`, UI culture, user's zone.
- Below `md`: `TicketLog` rows instead of the table.

## `TicketFilters`

One row: search `.input-field`, type / priority / status selects (same
options as now, including "open or in progress" and "closed or resolved"),
"reset" ghost button only while a filter is active, and a mono counter
`shown / total`. Shown on all three pages and for both table and Kanban.
The matching logic moves out of `TicketList` into a static, unit-tested
`TicketFilter` (search over title, description, product, author login).

## Kanban

- Same 8 columns, drag-and-drop JS and drag permissions.
- Column header `# status` (mono 13, lowercase) + count; no coloured dots.
  Column body `surface` with `line` border; drag-over shows an `accent`
  border (`kanban-drag-over`); dragged card dims.
- Card: `canvas`, `line` border, 8px radius; `#id` + `TypeTag` and
  `PriorityTag`; title (2 lines); tags as dots; footer author and assignee
  `Avatar Sm` without `ShowTitle` (fixes the duplicate name announcement);
  translated triage mark.

## Empty and loading states

- No tickets: `EmptyState` with a "new ticket" action.
- Filters match nothing: `EmptyState` "nothing found" with "reset filters".
- Loading: static `surface` bars.

## Testing

bUnit: table sort (default, toggle, `aria-sort`), columns per flags, row
links and triage mark; `TicketFilter` unit tests; filter bar counter and
reset; pages without `page-hero`/gradients and with path headers; Kanban
column headers and no duplicate avatar names. Build 0 warnings, all tests,
`app.css` regenerated.

Out of scope: ticket details/create (3c), statistics (3f).
