# Design system — Stage 3e-2: Audit logs

**Date:** 2026-10-11
**Status:** Approved in chat (HTML mockup `audit.html`, option A)
**Scope:** The `/audit-logs` page (`Components/Pages/Admin/AuditLogs.razor(.cs)`).
Branch `redesign/stage-1`. The Stage 3 decisions in the 3a spec apply; the
admin patterns from 3e-1 (inline rows, `rm … [y/N]` prompt) are reused.

## Goals

- Same "ops terminal" look as the rest of the app: no hero, no glow, no
  coloured metric cards, no modal.
- The journal reads like a log stream (`tail -f`), grouped by day.
- Details and the before/after diff open in place, like editing in admin.
- Times in the browser's time zone (the page still uses `ToLocalTime()`).
- Behaviour stays: live updates, 20 s auto refresh with pause, manual
  refresh, filters, CSV export, clearing with confirmation, link to the
  changed entity.

## Header

- `PageHeader` with path `~/audit-logs` (matches the URL; the route and the
  sidebar link stay) and the `audit.subtitle` text.
- Next to the subtitle, mono 13 px: `● live` (accent dot) when the hub is
  connected, `○ connecting` (muted) otherwise; then `· auto 20s` or
  `· paused`.
- Actions on the right: `pause` / `resume` (outline), `refresh` (outline,
  busy text while running), `export.csv` (outline), `clear…` (outline,
  `danger` text).
- `clear…` turns the actions area into an inline prompt
  `rm audit.log? [y/N]` with `y, clear` (danger) and `N, no`, `N` focused,
  y/n/Esc keys — the same look and behaviour as `DeletePrompt` (extract a
  shared piece if that keeps the code simpler; it must not be a `tr`).

## Figures

A row of `StatFigure`-style mono numbers with `# labels`, no icons:
`# total`, `# today` (in the user's zone), `# critical / warning`
(critical in `danger`, warning in a warning tone; if there is no warning
token, use `text-fg` for warning and keep `danger` only for critical), and
`# users`. Same counts as today.

## Filters

One wrapping row: search input (existing placeholder), severity as a
`SegmentedControl` `all / crit / warn / info`, category `select`, entity
`select`, period as a `SegmentedControl` `today / 7d / 30d / all`, user
`select`. When any filter is set, a muted mono line below shows
`<shown> of <total> · reset`. Filter semantics stay as they are.

## New events bar

When live events arrive while the list is not refreshed, show a full-width
dashed accent bar `↑ N new events — show` above the log; clicking applies
them (existing `ApplyPendingEventsAsync`). No ping animation.

## Log

- Grouped by day in the user's zone: a muted mono heading
  `# today · 11 Oct`, `# yesterday · 10 Oct`, then `# 9 Oct 2026`.
- One row per event, mono 13 px, columns: time `HH:mm:ss` (muted), level
  (`CRIT` danger, `WARN` warning tone, `info` muted), `@user` (`@system`
  when there is no user), action, entity `Name#Id` (muted), summary
  (sans 14 px, muted, truncated). Row hover `bg-surface`. The row is a
  button (`aria-expanded`), Enter/Space toggle it.
- Below `md` the row wraps: time, level and user on the first line, action
  and entity on the second, summary on the third.
- Clicking a row opens a detail block right under it (surface background,
  2 px accent bar on the left); one open at a time, clicking again closes.
  Contents:
  - key/value lines: when (local date and time, then UTC in muted mono),
    who (`@login`, muted `id N · IP` when known), what (action, entity,
    `open` link when `CanNavigateToEntity`);
  - the diff when the payload has one, git style: field name muted, then
    `- old` in `danger` and `+ new` in accent ink, `«null»` for empty;
  - otherwise the summary / raw changes in a mono block, or the existing
    "no changes" text.
- Paging: show the first 50 matching events, then a muted mono
  `↓ show 50 more · <shown> of <matching>` button. Changing a filter resets
  to 50. The page-size select and the page buttons are removed.
- Empty state: `EmptyState` with `audit.noLogs` and a reset button when
  filters are set. Loading: muted mono `loading…`.

## Time zone

All displayed times and the day grouping use `BaseComponent.ToLocal()`;
the "today", "7d" and "30d" filters and `# today` use the user's day
boundaries. The CSV export keeps UTC timestamps (column header says UTC).

## Cleanup

- Remove the hero, metric cards, inspector modal, pagination footer and
  their helpers that become unused (`GetSeverityBadgeClass`,
  `GetActionClass` if unused, page size handling).
- Remove `ConfigureAwait(false)` from the component code.
- Legacy `atab-*`/`amodal-*` CSS stays until Stage 3g.

## Localization

- New keys in en, uk, fr, es, de for any new text (`show more`, `today` /
  `yesterday` headings, `connecting`, `clear` prompt note…); level labels
  `CRIT/WARN/info`, `rm audit.log`, `export.csv` and `ok`-style technical
  tokens stay English.
- Fill the keys fr/es/de are already missing: all `audit.*` keys,
  `login.bruteForceBlocked`, and `common.themeDark` / `common.themeLight`
  (missing in every language except uk). Remove `audit.*` keys that
  become unused.

## Testing

bUnit:
- non-admins see access denied and nothing loads;
- rows grouped by local day with `today` / `yesterday` headings (fixed
  "now" via the time zone service or injected clock if available, else
  dates relative to `DateTime.UtcNow`);
- level, `@system` fallback, entity `Name#Id`;
- clicking a row shows the detail with the diff (`- old` / `+ new`), a
  second click hides it, only one open at a time;
- "show more" adds 50; a filter change resets to 50;
- severity and period filters; reset;
- clear: the prompt does not call the service, `N` closes it, `y` calls it
  once;
- the pending events bar applies the events.

Then a browser check in light and dark themes and at phone width.
