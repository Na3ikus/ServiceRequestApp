# Design system — Stage 3f: Statistics

**Date:** 2026-10-12
**Status:** Approved in chat (HTML mockup `stats.html` in `.superpowers/brainstorm/604-1791834631/content/`)
**Scope:** The `/statistics` page (`Components/Pages/Statistics/StatisticsPage.razor(.cs)`)
and a new statistics query in the application layer. Branch `redesign/stage-1`.
The Stage 3 decisions in the 3a spec apply (no heroes, translated `# headings`,
single-hue charts, `danger` only for critical/breaches); the admin path tabs
from 3e-1 and `TicketTable`-style sortable headers from 3b are reused.

## Decisions (agreed in chat)

1. **No Chart.js.** All charts are server-rendered HTML/CSS (no JS). Remove
   `wwwroot/js/chartInterop.js` and both `<script>` tags in `App.razor`
   (Chart.js CDN and `chartInterop.js`); nothing else uses them.
2. **Two path tabs** `overview` / `team` (same look as the admin tabs) and one
   shared period `SegmentedControl` `7d / 30d / 90d` (default 30d) in the
   header actions. The old third tab, the `PRO` badge, the medals and the
   separate "top developers" list go away.
3. **Honest metrics.** Resolution is taken from `TicketStatusHistory`, not
   from `DueDate`; period numbers really use the period; days are the user's
   days (browser zone, `IUserTimeZone`).

## Metric definitions

- **Done statuses:** `Resolved`, `Closed`, `Done`. **Open** = any other status.
- **Resolved at** = `ChangedAt` of the *first* history row whose `NewStatus`
  is a done status. A ticket that is done now but has no such row (old data
  the 3a backfill could not reconstruct) is not counted in period numbers
  and not in average times.
- **Period** = the last N days in the user's zone, today included
  (`7d` = today + 6 days before), converted to UTC bounds for the query.
- **Snapshot** numbers ignore the period and describe the current state:
  open, critical open, by status, by priority / type / tag (open tickets
  only), workload columns.
- **Created in period** = `CreatedAt` within the period.
- **Resolved in period** = resolved at within the period.
- **Average resolution time (period)** = mean of `resolved at − CreatedAt`
  over tickets resolved in the period; shown as `1д 6г`, `18г`, `6г 30хв`
  (localized units, `—` when there is no data).
- **Resolved / created** = resolved in period ÷ created in period, rounded
  percent; `—` when nothing was created.
- **Workload** = in progress × 2 + other active (same formula as today);
  in progress = `InProgress`, `CodeReview`, `Testing`; active = assigned and open.
- **Time in period** = sum of work log minutes by the developer whose work
  date falls in the period (check the `WorkLog` date field; use the user's
  day boundaries). **Average per ticket** = time in period ÷ closed in period.
- **Closed in period (developer)** = tickets assigned to the developer and
  resolved in the period.

## Application layer

- New method on `ITicketStatisticsService`:
  `Task<StatisticsReportDto> GetStatisticsReportAsync(int days, TimeZoneInfo zone)`
  returning snapshot, period figures, daily trend, distributions, products
  and team rows (one record per section; shapes are up to the implementer).
  Developers are joined by user id; the row carries the display name
  (person first + last name, else login), login and id.
- Load data with as few queries as reasonable (tickets with tags/product,
  done-status history rows for those tickets, users with person, work logs
  in the period). No N+1 per developer (the old efficiency code queries work
  logs per developer — do not copy that).
- Keep `GetExtendedAnalyticsAsync`, `GetEmployeeEfficiencyAsync`,
  `GetTopDevelopersAsync` and the count methods: the API
  (`AnalyticsController`, `TicketsController`) and other pages still use
  some of them. Remove only what becomes unused after the page change
  (check with grep; if `GetEmployeeEfficiencyAsync` / `EmployeeEfficiencyDto`
  have no remaining users, remove them).
- Unit tests (backend) for the definitions above: period bounds in a non-UTC
  zone (e.g. UTC+14 and UTC−10), first done transition wins (reopened then
  resolved again), done tickets without history excluded from period numbers,
  snapshot distributions count open tickets only, workload formula, work log
  time inside/outside the period, developers with no tickets still listed
  when their role is Developer (as today).

## Page

### Header

`PageHeader` with path `~/statistics` and `stats.subtitle`; actions: the
period `SegmentedControl`. Below it the path tabs `overview` / `team`
(tab state in the component; a query string is optional, not required).
Non-developers/non-admins keep today's behaviour (redirect to `/`); no
access-denied card is needed.

### `overview`

1. **Figures** (`StatFigure`): `# відкрито`, `# критичні відкриті` (`danger`
   when > 0), `# створено · 30d`, `# вирішено · 30d`,
   `# сер. час вирішення · 30d`, `# вирішено / створено · 30d`. The period
   suffix follows the selected period and stays English (`7d/30d/90d`).
2. **Trend** `# створено і вирішено · 30d`: one column pair per day
   (created = `accent/35`, resolved = `accent`), heights relative to the
   busiest day, min 1 px, a bottom rule, 5 muted date labels (first, three
   evenly spaced, `сьогодні`), a legend on the right of the heading. Each day
   has a `title` tooltip (`12 жовт: створено 3, вирішено 2`). The chart is
   `role="img"` with an `aria-label` summary (totals for the period).
   90d keeps one pair per day (columns get thinner).
3. **Distributions** in three columns (`lg`), one column on small screens:
   - `# за статусом · зараз`: all statuses in workflow order; open statuses
     in `accent`, done statuses muted (label and bar);
   - `# за пріоритетом · відкриті`: Critical label and bar in `danger`,
     others accent;
   - `# за типом · відкриті`: count + muted percent;
   - `# теги · відкриті`: top 8 by count; tag colours are NOT used (single
     hue). Empty → muted `stats.noData` line.
   Bars are relative to the largest value in that list; label | track | value.
4. **Products** `# продукти · 30d`: table (terminal table style used in
   admin) with product, open (snapshot), created (period), resolved
   (period), average resolution time with a small accent bar relative to the
   slowest product. Sortable headers; default sort by average time desc.
   Products with nothing open/created/resolved are hidden.

### `team`

1. **Figures:** `# з активними заявками`, `# закрито · 30d`,
   `# залоговано · 30d` (hours).
2. **Table:** developer (`Avatar` + name + muted `@login`), in progress,
   active, closed · period, time · period, average per ticket, workload
   (small accent bar relative to the max + number). Sortable headers
   (`aria-sort`), default closed desc then workload desc. Rows with all zeros
   are muted. A muted mono note under the table explains the workload
   formula.

### States

- Loading: muted mono `loading…` (no skeleton cards).
- Period change reloads the report; keep the previous content visible while
  loading (no flash), guard against out-of-order responses.
- Load failure: error toast as on other pages, page stays usable.

### Cleanup

- Remove the old cards, gradients, icons, canvases, colour tables
  (`StatusOrder` hex colours etc.), `Pct`, count-up animation calls,
  `ShouldRenderCharts`, `OnAfterRenderAsync` chart code, and the
  `DateTime.Now` "data updated" line.
- No `ConfigureAwait(false)` in component code.
- Hard-coded `год`/`хв` and `pts` are replaced by localized formatting.

## Localization

New `stats.*` keys in en, uk, fr, es, de for every new text (tab labels
`overview`/`team` and period tokens stay English like other path tokens).
Remove `stats.*` / `analytics.*` keys that become unused **and are not used
by the API or other pages** (grep first). All five files keep the same key set.

## Testing

bUnit (`StatisticsPageTests`), with the service mocked:
- non-developer is redirected and the service is not called;
- figures render the report values; critical uses `danger` only when > 0;
- trend renders one pair per day with heights relative to the max and the
  aria summary;
- distributions: done statuses muted, Critical in danger, tags single hue;
- products and team tables sort by header click (`aria-sort` toggles);
- period switch calls the service with the new days and the user's zone,
  and a slow earlier response does not overwrite a newer one;
- tab switch shows `team` content;
- duration formatting (`—`, minutes only, hours + minutes, days + hours).

Then a browser check in light and dark themes, all four accents, at phone width.
