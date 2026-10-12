# Design System Stage 2a (Primitives) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the terminal-style primitives (tags, avatar, kbd, page header, panel, empty state), use tags and avatars everywhere, and delete the `BaseComponent` color helpers and `ticket-status.css`.

**Architecture:** Small Razor components in `Components/UI/Primitives/` built only on Stage 1 tokens. One static class `TicketTones` owns the enum → tone mapping; `Tag` renders a tone; `StatusTag`/`PriorityTag`/`TypeTag`/`RoleTag` wrap it with translated text. Pages swap helper-built `<span>`s for these components; then the helpers and their CSS are deleted.

**Tech Stack:** .NET 10 Blazor Server, Tailwind CSS 3.4.17 (`darkMode: 'class'`), NUnit + Moq + FluentAssertions + bUnit (`ComponentTestHost`).

**Spec:** `docs/superpowers/specs/2026-10-10-design-system-stage2a-primitives-design.md`

## Global Constraints

- Work on branch `redesign/stage-1`; do not merge to `master`.
- `dotnet build ServiceDeskSystem` compiles Tailwind; `wwwroot/css/app.css` is generated — commit it, never hand-edit it.
- Token classes only (`canvas surface line fg muted accent accent-ink on-accent danger`); no gradients, blur or shadows in new or touched markup.
- Nothing below 12px; mono text 13px except `Kbd` 12px.
- Radius: 6px (`rounded-md`) for tags/avatars/controls, 8px (`rounded-lg`) for panels, 4px (`rounded`) for kbd.
- Every user-visible string through `L.Translate`, keys present in en/uk/de/es/fr (`LocalizationFilesTests` enforces parity for `settings.*`; new non-settings keys must be added to all 5 languages by hand, in one file each).
- bUnit: index `FindAll(...)` results via `.ToList()` (AngleSharp version skew — Stage 1 ruling).
- Solution builds with 0 warnings (StyleCop is on: member order — fields, properties, constructors, methods; static before instance).
- Boolean ARIA attributes rendered as explicit `"true"`/`"false"` strings.

## Review Focus

1. **Name edge cases in `Avatar`** (`null`, `""`, `"   "`, `"A"`, `"  Nazar   Hutsol "`, Cyrillic, `j.doe`): never throws, never renders empty → Task 3 test `Initials_cases`.
2. **A new enum member without a tone** (e.g. a future `TicketStatus.Blocked`): CI fails before a page throws at render → Task 2 test `For_covers_every_enum_value`.
3. **Long translated statuses in narrow rows** (`de` "In Bearbeitung", `uk` "Перевірка коду") at 375px: chips wrap, never overflow → Task 4 markup uses `flex-wrap` + Task 7 manual check.
4. **Light theme accent chip** (`New`): black border, black text on lime fill — readable; dark theme lime text on black → Task 2 test `Accent_tone_classes` asserts both class sets.
5. **Internal comments shown only to admin/developer** stay filtered after the comment markup change → Task 5 keeps the existing `Where(c => !c.IsInternal || IsAdminOrDeveloper)` untouched; manual check as a `User`.

---

### Task 1: Move Settings controls to `Components/UI/Primitives` + `Kbd`

**Files:**
- Move: `Components/Pages/Settings/Controls/{SegmentedControl.razor,SegmentedControl.razor.cs,ToggleSwitch.razor,SettingRow.razor}` → `Components/UI/Primitives/`
- Create: `Components/UI/Primitives/Kbd.razor`
- Modify: `Components/_Imports.razor` (add `@using ServiceDeskSystem.Components.UI.Primitives`)
- Modify: `Components/Pages/Settings/Settings.razor` (drop the `Controls` using where no longer needed; replace both inline `<kbd class=…>` with `<Kbd>`)
- Modify: `ServiceDeskSystem.Tests/Frontend/Components/Settings/SettingsControlsTests.cs` (namespace import)
- Test: `ServiceDeskSystem.Tests/Frontend/Components/Primitives/PrimitivesTests.cs`

**Interfaces:**
- Produces: namespace `ServiceDeskSystem.Components.UI.Primitives` with `SegmentedControl`, `ToggleSwitch`, `SettingRow` (parameters unchanged from Stage 1), and `Kbd` (`RenderFragment ChildContent`) rendering `<kbd class="font-mono text-[12px] text-muted border border-line rounded px-[5px]">`.
- `AccentPicker` stays in `Components/Pages/Settings/Controls/`.

- [ ] **Step 1: Failing test** `Kbd_renders_kbd_element` — `RenderComponent<Kbd>(p => p.AddChildContent("Ctrl+B"))` → `cut.Find("kbd").TextContent == "Ctrl+B"`.
- [ ] **Step 2: Run** `dotnet test ServiceDeskSystem.Tests --filter FullyQualifiedName~PrimitivesTests` → FAIL (type missing).
- [ ] **Step 3: Move** the three controls with `git mv`, update their namespace (`.razor.cs` file-scoped namespace; `.razor` files get it from the folder). Create `Kbd.razor`. Update `_Imports.razor`, `Settings.razor`, `SettingsControlsTests.cs`.
- [ ] **Step 4: Run** filter `FullyQualifiedName~PrimitivesTests|FullyQualifiedName~Settings` → PASS (Stage 1 Settings tests unchanged in logic).
- [ ] **Step 5: Commit** — `refactor(ui): move settings controls to shared primitives and add Kbd`.

---

### Task 2: `Tone`, `TicketTones`, `Tag`, `StatusTag`, `PriorityTag`, `TypeTag`, `RoleTag`

**Files:**
- Create: `Components/UI/Primitives/Tone.cs`, `TicketTones.cs`, `Tag.razor`, `StatusTag.razor`, `PriorityTag.razor`, `TypeTag.razor`, `RoleTag.razor`
- Test: `ServiceDeskSystem.Tests/Frontend/Components/Primitives/TagTests.cs`, `ServiceDeskSystem.Tests/Backend/Unit/TicketTonesTests.cs`

**Interfaces:**
- Produces:
  - `public enum Tone { Accent, Danger, Active, Muted }`
  - `public static class TicketTones { public static Tone For(TicketStatus s); public static Tone For(TicketPriority p); public static Tone For(TicketType t); }` — mapping per spec table; `_ => throw new ArgumentOutOfRangeException(...)`.
  - `Tag`: `[Parameter] Tone Tone = Tone.Muted`, `RenderFragment? ChildContent`, `string? Title`. Renders `<span data-tone="accent|danger|active|muted" class="inline-flex items-center whitespace-nowrap font-mono text-[13px] leading-[1.45] border rounded-md px-[7px] py-px {tone}">`. Tone classes:
    - Accent: `border-black bg-accent text-on-accent dark:border-accent dark:bg-transparent dark:text-accent`
    - Danger: `border-danger text-danger`
    - Active: `border-line text-fg`
    - Muted: `border-line text-muted`
  - `StatusTag(TicketStatus Status)`, `PriorityTag(TicketPriority Priority)`, `TypeTag(TicketType Type)`, `RoleTag(UserRole Role)` — inject `ILocalizationService L`; keys: `status.new|open|inProgress|testing|codeReview|resolved|done|closed`, `priority.low|medium|high|critical`, `ticketType.support|bug|project|consultation`, `admin.roleAdmin|roleDeveloper|roleUser`. `RoleTag` is always `Tone.Muted`.

- [ ] **Step 1: Failing unit tests** `TicketTonesTests`:
  - `[TestCase(TicketStatus.New, Tone.Accent)]`, `Open/InProgress/Testing/CodeReview → Active`, `Resolved/Done/Closed → Muted`.
  - `[TestCase(TicketPriority.Critical, Tone.Danger)]`, `High → Active`, `Medium/Low → Muted`.
  - `[TestCase(TicketType.Bug, Tone.Active)]`, `Support/Project/Consultation → Muted`.
  - `For_covers_every_enum_value` — `foreach (var v in Enum.GetValues<TicketStatus>())` (and the other two) `FluentActions.Invoking(() => TicketTones.For(v)).Should().NotThrow()`.
- [ ] **Step 2: Failing bUnit tests** `TagTests`:
  - `StatusTag_new_is_accent_with_translated_text` — `Status=New` → `span[data-tone=accent]`, text `status.new`.
  - `PriorityTag_critical_is_danger` → `data-tone="danger"`, text `priority.critical`.
  - `TypeTag_bug_is_active` → `data-tone="active"`, text `ticketType.bug`.
  - `RoleTag_developer_is_muted` → `data-tone="muted"`, text `admin.roleDeveloper`.
  - `Accent_tone_classes` — `Tag Tone=Accent` class contains `bg-accent`, `text-on-accent`, `border-black`, `dark:text-accent`, `dark:bg-transparent`.
  - `Tag_title_renders_attribute` — `Title="x"` → `title="x"`.
- [ ] **Step 3: Run** `dotnet test ServiceDeskSystem.Tests --filter "FullyQualifiedName~TicketTonesTests|FullyQualifiedName~TagTests"` → FAIL (types missing).
- [ ] **Step 4: Implement** the seven files.
- [ ] **Step 5: Run** → PASS.
- [ ] **Step 6: Commit** — `feat(ui): add tone-based tags for ticket status, priority, type and role`.

---

### Task 3: `Avatar`

**Files:**
- Create: `Components/UI/Primitives/Avatar.razor`, `Components/UI/Primitives/AvatarSize.cs`
- Test: `ServiceDeskSystem.Tests/Frontend/Components/Primitives/AvatarTests.cs`

**Interfaces:**
- Produces:
  - `public enum AvatarSize { Sm, Md, Lg }`
  - `Avatar`: `[Parameter] string? Name`, `AvatarSize Size = AvatarSize.Md`, `bool ShowTitle`. `public static string Initials(string? name)` (public for tests and reuse).
  - Markup: `<span data-avatar class="inline-flex shrink-0 items-center justify-center rounded-md border border-line bg-surface font-mono font-medium text-fg {size}">`; size classes `Sm: h-6 w-6 text-[12px]`, `Md: h-7 w-7 text-[12px]`, `Lg: h-10 w-10 text-[13px]`. `ShowTitle` → `title=Name role="img" aria-label=Name`; otherwise `aria-hidden="true"`.
  - Initials rule (spec): split on whitespace, `.`, `_`, `-` (remove empty entries); ≥2 parts → first char of parts 0 and 1; 1 part → first two chars (or one if length 1); `ToUpperInvariant()`; null/blank → `?`.

- [ ] **Step 1: Failing tests**
  - `Initials_cases` — `[TestCase("Nazar Hutsol","NH")] [TestCase("admin","AD")] [TestCase("Олена Коваль","ОК")] [TestCase("j.doe","JD")] [TestCase("A","A")] [TestCase("  Nazar   Hutsol ","NH")] [TestCase("",  "?")] [TestCase("   ","?")] [TestCase(null,"?")]` → `Avatar.Initials(name) == expected`.
  - `Size_classes` — `Sm` has `h-6`, `Md` has `h-7`, `Lg` has `h-10`.
  - `Hidden_from_screen_readers_by_default` — `aria-hidden="true"`, no `title`.
  - `ShowTitle_exposes_name` — `title="Nazar Hutsol"`, `aria-label="Nazar Hutsol"`, `role="img"`, no `aria-hidden`.
- [ ] **Step 2: Run** `--filter FullyQualifiedName~AvatarTests` → FAIL.
- [ ] **Step 3: Implement.**
- [ ] **Step 4: Run** → PASS.
- [ ] **Step 5: Commit** — `feat(ui): add monochrome initials avatar`.

---

### Task 4: `PageHeader`, `Panel`, `EmptyState`; Settings uses `PageHeader`

**Files:**
- Create: `Components/UI/Primitives/PathSegment.cs`, `PageHeader.razor`, `Panel.razor`, `EmptyState.razor`
- Modify: `Components/Pages/Settings/Settings.razor` (replace the `<h1>…~/settings</h1>` with `<PageHeader Segments="…" />`, segment `("settings")`)
- Test: `ServiceDeskSystem.Tests/Frontend/Components/Primitives/LayoutPrimitivesTests.cs`; modify `SettingsPageTests.cs`

**Interfaces:**
- Produces:
  - `public sealed record PathSegment(string Text, string? Href = null);`
  - `PageHeader`: `IReadOnlyList<PathSegment> Segments`, `string? Subtitle`, `RenderFragment? Actions`. Markup: wrapper `flex flex-wrap items-baseline justify-between gap-4 mb-4`; `<h1 class="font-mono text-[20px] font-medium text-fg">` = `<span class="text-muted">~/</span>` + segments joined by `<span class="text-muted">/</span>`; segment with `Href` → `<a href class="text-muted border-b border-dashed border-line hover:text-fg">`; last segment wrapped in `<span aria-current="page">`; actions in `<div class="flex items-center gap-2">`; subtitle `<p class="text-[13px] text-muted mt-1">`.
  - `Panel`: `string? Title`, `RenderFragment? Meta`, `RenderFragment? ChildContent`, `string? Class`. `<section class="rounded-lg border border-line bg-surface {Class}">`; header (only if `Title` or `Meta`) `<div class="flex items-center justify-between px-3.5 py-2 border-b border-line font-mono text-[13px] text-muted">` with `# {Title}`; body `<div class="px-3.5 py-3">`.
  - `EmptyState`: `string Message`, `string? ActionText`, `EventCallback OnAction`, `string? Hotkey`. `<p class="font-mono text-[13px] text-muted px-3.5 py-7">&gt; {Message}` + (if `ActionText`) ` — <button type="button" class="text-fg hover:underline">{ActionText}</button>` + (if `Hotkey`) `<Kbd>`.

- [ ] **Step 1: Failing tests** (`LayoutPrimitivesTests`):
  - `PageHeader_links_segments_with_href` — segments `[("tickets","/tickets"),("1042")]` → one `a[href='/tickets']` text `tickets`; `[aria-current=page]` text `1042`; `h1` text starts with `~/`.
  - `PageHeader_renders_subtitle_and_actions`.
  - `Panel_header_only_with_title_or_meta` — no title/meta → no `.border-b` header; `Title="details"` → text `# details`.
  - `EmptyState_action_invokes_callback` — click button → callback invoked once.
  - `EmptyState_without_action_has_no_button` — and renders `> ` prefix.
  - `EmptyState_hotkey_renders_kbd`.
  - In `SettingsPageTests`: `Heading_is_path_header` — `h1` text `~/settings`, `[aria-current=page]` text `settings`.
- [ ] **Step 2: Run** `--filter "FullyQualifiedName~LayoutPrimitivesTests|FullyQualifiedName~SettingsPageTests"` → FAIL.
- [ ] **Step 3: Implement** the components and the Settings change.
- [ ] **Step 4: Run** → PASS.
- [ ] **Step 5: Commit** — `feat(ui): add page header, panel and empty state primitives`.

---

### Task 5: Replace tags and avatars on pages

**Files (modify):**
- `Components/Features/Tickets/TicketTable.razor` — remove the priority circle (`div.w-10.h-10…@GetPriorityColorClass`) and put `<span class="w-14 shrink-0 font-mono text-[13px] text-muted">#@ticket.Id</span>` in its place; type/status/priority spans → `<TypeTag>`, `<StatusTag>`, `<PriorityTag>`; the badge row keeps `flex flex-wrap`.
- `Components/Pages/Index.razor` — same treatment for its ticket list (lines around 141 and 158).
- `Components/Features/Kanban/KanbanCard.razor` — type/priority → tags; author and assignee gradient circles → `<Avatar Name="@Ticket.Author?.Login" Size="AvatarSize.Sm" ShowTitle="true" />` / `@Ticket.Developer.Login`.
- `Components/Pages/Tickets/TicketDetails.razor` — every `GetStatusBadgeClass|GetPriorityBadgeClass|GetTicketTypeBadgeClass` span (lines ~104, 123, 126, 169, 458, 491) → tag components; author avatar (~156) → `<Avatar Name="@Ticket.Author?.Login" Size="AvatarSize.Sm" />`; comment avatar (~329) → `<Avatar Name="@comment.Author?.Login" Size="AvatarSize.Sm" />` (no internal color); the existing `@if (comment.IsInternal)` badge after the date → `<Tag Tone="Tone.Muted">@L.Translate("details.internalNote")</Tag>`. Leave the comment filter and timeline/surface colors untouched (Stage 3).
- `Components/Pages/Tickets/WorkLogsSection.razor` (~109) → `<Avatar Name="@log.User?.Login" Size="AvatarSize.Sm" />`.
- `Components/Layout/MainLayout.razor` (~224) → `<Avatar Name="@AuthService.CurrentUser?.Login" Size="AvatarSize.Lg" />`.
- `Components/Pages/Admin/Components/UsersTab.razor(.cs)` — `div.auser-avatar @avatarClass` → `<Avatar Name="…" />` where `Name` is the same name the current initials helper reads (first + last name, falling back to `Login` when both are empty) + `<RoleTag Role="user.Role" />` where the role is displayed; delete `GetAvatarClass` and the now-unused initials helper.
- `Components/Pages/Statistics/StatisticsPage.razor` (~381) → `<TypeTag Type="item.Type" />`.

**Interfaces:**
- Consumes: Task 2 tags, Task 3 `Avatar`/`AvatarSize`, `Tone`.

- [ ] **Step 1: Failing test** — `ServiceDeskSystem.Tests/Frontend/Components/Primitives/HelperUsageTests.cs` `No_page_uses_color_helpers`: read every `*.razor`/`*.razor.cs` under the web project's `Components` folder (locate via `AppContext.BaseDirectory` → walk up to the repo root containing `ServiceDeskSystem.sln`) and assert none contains `GetStatusBadgeClass(`, `GetPriorityBadgeClass(`, `GetTicketTypeBadgeClass(`, `GetPriorityColorClass(`, `GetStatusDotColor(`, `GetAvatarGradient(`, `avatar-admin`, `avatar-dev`, `avatar-user` — excluding `Components/UI/Base/BaseComponent.cs` (cleaned in Task 6). Failure message lists `file: token`.
- [ ] **Step 2: Run** `--filter FullyQualifiedName~HelperUsageTests` → FAIL listing the 7 files.
- [ ] **Step 3: Edit** the files above.
- [ ] **Step 4: Run** `HelperUsageTests` → PASS; full `dotnet test ServiceDeskSystem.Tests` → PASS; `dotnet build ServiceDeskSystem` → 0 warnings.
- [ ] **Step 5: Commit** — `feat(ui): use tag and avatar primitives across pages`.

---

### Task 6: Delete helpers, `ticket-status.css` and dead badge CSS

**Files:**
- Modify: `Components/UI/Base/BaseComponent.cs` (delete the six helpers; delete `GetStatusText`/`GetPriorityText`/`GetTicketTypeText` only if `git grep` finds no caller)
- Delete: `wwwroot/css/ticket-status.css`; modify `Components/App.razor:14` (remove its `<link>`)
- Modify: `wwwroot/app.css`, `Components/Pages/Tickets/Styles/TicketStyles.css`, `Components/Pages/Tickets/TicketDetails.razor.css`, `Components/Pages/Admin/**/*.css` (remove `.avatar-admin|dev|user`, `.auser-avatar` if unused, `.status-badge*`, `.priority-badge*`, `.ticket-priority-badge` rules with zero usages)
- Test: extend `HelperUsageTests`

- [ ] **Step 1: Audit** — list every class selector in `ticket-status.css`; `git grep` each in `ServiceDeskSystem/Components`; write the files that use any of them (outside the replaced badges) into the ledger as the manual-check list for Task 7.
- [ ] **Step 2: Failing tests** — remove the `BaseComponent.cs` exclusion from `No_page_uses_color_helpers`; add `Ticket_status_css_is_gone` (file `ServiceDeskSystem/wwwroot/css/ticket-status.css` does not exist and `App.razor` does not contain `ticket-status.css`).
- [ ] **Step 3: Run** `--filter FullyQualifiedName~HelperUsageTests` → FAIL.
- [ ] **Step 4: Delete** the helpers, the file, the link and the dead CSS (each CSS rule only after its class has zero hits in `Components/**` and `wwwroot/js/**`).
- [ ] **Step 5: Run** filtered → PASS; full `dotnet test ServiceDeskSystem.Tests` → PASS; `dotnet build ServiceDeskSystem.sln --no-incremental` → 0 warnings, 0 errors.
- [ ] **Step 6: Commit** — `chore(ui): remove color helpers and ticket-status.css`.

---

### Task 7: Final verification

- [ ] **Step 1:** `dotnet test ServiceDeskSystem.sln` and `npm --prefix ServiceDeskSystem run test:js` → all PASS.
- [ ] **Step 2:** Grep `bg-gradient` in the files touched by Task 5 and confirm none remains inside tag/avatar markup (page heroes/buttons may remain — Stage 3).
- [ ] **Step 3:** Manual browser checklist for the user (both themes, `uk` + `en`): tickets, my tickets, home, kanban, ticket details as admin and as `User` (internal comments hidden for `User`), admin → users, statistics, settings; tickets table at 375px; every page on the Task 6 audit list.
- [ ] **Step 4:** Whole-branch review of the 2a range, address findings, ledger; do not merge (2b next).
