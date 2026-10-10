# Design system — Stage 2c: Shell

**Date:** 2026-10-10
**Status:** Approved in chat — awaiting written-spec review
**Scope:** Stage 2c of the "ops terminal" (B2) redesign of ServiceDeskSystem.
**Builds on:** Stage 1 spec (`2026-10-09-design-system-stage1-design.md`:
tokens, fonts, `.mark`, type scale) and Stage 2a spec
(`2026-10-10-design-system-stage2a-primitives-design.md`: `Tag`, `RoleTag`,
`Avatar`, `Kbd`, `PageHeader`). Work continues on branch `redesign/stage-1`;
nothing is merged to `master` until the app looks coherent.

## Goal

The frame around every page — sidebar, top bar, login/register, reconnect
dialog — moves to the terminal style, so the app looks new even before the
pages themselves are restyled in Stage 3.

**Order change (2026-10-10):** 2c runs before 2b because it gives the most
visible result. `LanguageDropdown` moves from 2b into 2c (the shell uses it).
2b keeps Modal, Toast, HotkeysModal, Breadcrumb, CommandPalette.

**Not in 2c:** page content (heroes, gradient buttons, cards, forms other
than login/register) — Stage 3; Modal, Toast, CommandPalette, HotkeysModal
styling — 2b (HotkeysModal only loses its Ctrl+B row here).

## Visual rules (from Stage 1, binding)

Token classes only (`canvas surface line fg muted accent accent-ink
on-accent danger`); no gradients, no blur, no shadows (popovers and
dropdowns use a 1px `line` border instead). Radius 6px controls, 8px
panels/popovers. Nothing below 12px; mono text 13px except `Kbd` 12px.
Body text Geist 15px; secondary 13px. Light theme never uses `accent` as
text color on white — use `accent-ink` or the `.mark` utility.

## New building blocks

### `Logo` (`Components/UI/Primitives/Logo.razor`)

- Renders `sd` + `_` in Geist Mono 500; `sd` is `fg`, `_` is the cursor in
  `accent-ink` (dark: accent; light: black — consistent with the token).
- Cursor blinks: CSS keyframes, 1.2s step cycle (visible ~0.6s / hidden
  ~0.6s). Under `@media (prefers-reduced-motion: reduce)` it does not blink.
- Parameter `LogoSize Size = Md` — `Md` 20px (sidebar), `Lg` 40px (auth
  screen). Enum `LogoSize` next to it.
- The element has `aria-label` = `L.Translate("nav.serviceDesk")` and
  `role="img"`; inner spans `aria-hidden="true"`.

### `RoutePath` (`Components/Layout/RoutePath.razor` + `.razor.cs`)

- Shows the current route as a path: `~` for the root, otherwise `~/` +
  segments joined by `/`, e.g. `~/tickets/42`, `~/admin`.
- Pure static helper `RoutePath.Segments(string absoluteUri, string baseUri)`
  returns `IReadOnlyList<PathSegment>` (2a record `PathSegment(Text, Href)`):
  strips base URI, query string and fragment; lowercases; splits on `/`,
  drops empty parts; URL-decodes each segment; every segment except the last
  gets `Href` = cumulative relative path (`tickets`, `tickets/42`). The root
  `~` is rendered by the component, not returned as a segment.
- Rendering: Geist Mono 13px; `~/` and linked segments `muted` (hover `fg`),
  last segment `fg` with `aria-current="page"`; separators `/` `muted`.
  Wrapped in `<nav aria-label="@L.Translate("nav.path")">`.
- Subscribes to `NavigationManager.LocationChanged` and re-renders;
  unsubscribes on dispose.
- Long paths truncate with ellipsis (`truncate`, `min-w-0`).

### Form classes (in `wwwroot/app.css`, `@layer components`)

- `.input-field`: `h-9 w-full rounded-md border border-line bg-surface px-3
  text-[15px] text-fg placeholder:text-muted`; focus: `border-fg` and
  `outline-none`; `aria-invalid="true"` → `border-danger`.
- `.btn-accent`: `h-9 rounded-md bg-accent px-4 text-on-accent font-medium
  text-[15px]`; hover `bg-accent/90`; `disabled` → `opacity-50
  cursor-not-allowed`; focus-visible outline 2px `fg` offset 2px.
- `.btn-ghost`: `h-8 rounded-md border border-line px-2 text-muted
  hover:text-fg hover:bg-surface` — used for icon/text buttons in the top bar
  and auth corner (theme toggle, language trigger, menu button).

Stage 3 forms reuse these.

## NavMenu (`Components/Layout/NavMenu.razor(.cs,.css)`)

- Width 240px (`w-60`), background `surface`, right border `line`, full
  height, scrolls vertically when needed.
- Top: `Logo Size=Md` linking to `/`, padding 16px, height 48px aligned with
  the top bar (bottom border `line`).
- Sections: heading `# {section}` in Geist Mono 13px `muted` (translated
  section title, lowercased). Sections are always expanded — the section
  toggle and `ToggleSection`/`IsExpanded` are removed.
- Items: Geist Mono 13px, label lowercased with the current culture
  (`ToLower(CultureInfo.CurrentCulture)`), no icons. Each item renders a
  fixed 1ch slot before the label: active → `›` in `accent-ink`; inactive →
  empty. Active label `fg`; inactive `muted`, hover `fg` + background
  `canvas`. Row height 32px, radius 6px.
- Active state: `NavLink` with `ActiveClass="active"`; the `›` slot is shown
  via CSS for `.active` (so NavLink matching rules stay as they are).
- Footer: `@L.Translate("common.footer")` in 13px `muted`.
- Role-based item lists are unchanged (same `BuildMenu` logic, minus icon
  keys). `GetIconPath`, `IconKey`, `IsCollapsed`, `ToggleCollapseRequested`
  and the collapse button are deleted.
- `NavMenu.razor.css` is rewritten to the minimum needed (or deleted if
  Tailwind classes cover everything); the old color schemes, tooltip and
  collapsed rules go away.

## Removing sidebar collapse

- `IThemeService` / `ThemeService`: delete `IsSidebarCollapsed`,
  `SetSidebarCollapsedAsync` and the `sidebarManager.getCollapsed` call.
- `wwwroot/js/sidebar.js`: delete `getCollapsed` / `setCollapsed`; delete
  the Ctrl+B branch of the hotkey handler (other global hotkeys stay).
- `MainLayout.razor.cs`: delete `HandleSidebarHotkey`,
  `ToggleSidebarCollapseAsync`; mobile drawer open/close stays.
- `Settings.razor(.cs)`: delete the sidebar `SettingRow`, `SidebarOptions`
  and `SetSidebarAsync`.
- `HotkeysModal.razor`: delete the Ctrl+B row.
- Localization: delete the sidebar setting keys (`settings.sidebar` and its
  option keys) and `settings.hotkeySidebar` from all 5 languages — only keys
  with no remaining usage (grep decides).
- `wwwroot/js/prefs.js`: add `'sidebarCollapsed'` to `OBSOLETE_KEYS` so the
  legacy value is removed from localStorage.
- Tests that reference these members are updated.

## Top bar and layout (`Components/Layout/MainLayout.razor(.cs,.css)`)

- Page background `canvas` everywhere; the `app-layout-bg` gradient class
  and its CSS are removed from the layout (delete the rule if unused
  elsewhere).
- Loading state (`!authRestored`): centered `loading…` in Geist Mono 13px
  `muted` (text from `common.loading`).
- Top bar: height 48px, background `canvas`, bottom border `line`, no
  shadow, no blur. Left: menu button (`.btn-ghost`, `lg:hidden`,
  `aria-label` = `L.Translate("nav.openMenu")`) + `RoutePath`. The header
  date text and `GetHeaderDateText` are removed.
- Right side, gap 8px:
  1. `LanguageDropdown` (shared component, see below) — replaces the inline
     `<details>` language menu, `isLanguageDropdownOpen` and
     `ToggleLanguageDropdownAsync`/`SetLanguageAsync` in MainLayout if they
     have no other use.
  2. Theme toggle: `.btn-ghost` with the existing sun/moon SVG (16px,
     `currentColor`), `aria-label` = `L.Translate("theme.toggle")`.
  3. Notifications: `.btn-ghost` with bell SVG; unread count as a mono 12px
     chip `bg-accent text-on-accent rounded px-1` positioned top-right; the
     old `has-new` pulse animation is removed.
  4. User menu (`<details>`): summary = `Avatar Size=Md` + name (`hidden
     sm:inline`, 13px `fg`), keeps `aria-label` with the login. Menu: profile
     link and logout button.
- Popovers (language, notifications, user menu): background `surface`,
  border `line`, radius 8px, no shadow, `z-50`, item rows 32px, radius 6px,
  hover background `canvas`. Logout hover text `danger`.
- Notifications popover:
  - Width 360px (max `100vw - 1.5rem`). Header row: `# notifications` mono
    13px `muted` + count; "mark all read" as text button (13px `muted`,
    hover `fg`).
  - Rows separated by dashed `line` borders. Each row: unread marker (6px
    dot `bg-accent`, or empty slot when read), `#{TicketId}` mono 13px
    `muted`, time mono 13px `muted` on the right, message 13px `fg`; delete
    button (`×`, `muted`, hover `danger`, `aria-label` =
    `notifications.delete`). Type icons and per-type colors are removed.
  - Empty: `EmptyState` with `notifications.empty`.
- Mobile drawer: `aside` fixed, `w-60`, slides in as now; overlay
  `bg-canvas/80` without blur; no shadow.
- `<div id="blazor-error-ui">`: restyled in `app.css` to tokens (`surface`
  background, `line` top border, `fg` text, `danger` accent text for the
  message) — text unchanged.

## LanguageDropdown (`Components/Shared/LanguageDropdown.razor`)

- Trigger: `.btn-ghost` showing the current code in mono 13px (`uk ▾`), no
  globe icon; `aria-haspopup="menu"`, `aria-expanded` as `"true"`/`"false"`.
- Menu: popover rules above; each row `uk` code (mono 13px `muted`) +
  native name (13px `fg`); selected row has the `›` marker in `accent-ink`
  and `aria-current="true"`.
- Behavior unchanged (backdrop click closes, `LanguageChanged` re-render).
  Used by MainLayout, Login and Register.

## Login and Register (`Components/Pages/Auth/*`)

Shared split layout, extracted to `Components/Pages/Auth/AuthShell.razor`
(`RenderFragment ChildContent`, `string Path` e.g. `login`, `string
Subtitle`):

- Full height, background `canvas`. Two columns on `lg+`:
  - **Left (50%)** — background `surface`, right border `line`, padding
    48px, column layout: `Logo Size=Lg` at top; tagline in the middle (Geist
    20px `fg`, max-width 24ch, new key `auth.tagline` in all 5 languages);
    bottom line `@L.Translate("common.footer") © {year}` 13px `muted`.
  - **Right** — top-right corner: `LanguageDropdown` + theme toggle
    (`.btn-ghost`). Centered form column `max-w-[380px]`: heading `~/` +
    `Path` in Geist Mono 500 20px (`~/` muted), subtitle 13px `muted`
    (`login.title` / `register.title`), then `ChildContent`.
- Below `lg`: left column hidden; `Logo Size=Md` shown above the heading.
- Form internals (Login and Register):
  - Labels 13px `muted`, `for` bound to input id; inputs `.input-field`
    (field icons removed); password visibility toggle stays as a `muted`
    icon button inside the field with `aria-label` (`login.showPassword` /
    `login.hidePassword` — new keys if missing).
  - Validation messages 13px `danger`.
  - Error banner: 1px `danger` border, radius 6px, 13px `danger` text, no
    fill, `role="alert"`.
  - Caps Lock warning: 13px `muted` line with `<Kbd>Caps Lock</Kbd>` and the
    existing `auth.capsLock` text.
  - Submit: full-width `.btn-accent`; loading state shows mono `…` after the
    existing "signing in" text (no spinner SVG).
  - Link to the other page: 13px `muted` + `fg` underlined link.
  - Demo accounts (Login only, same `ShouldShowDemoAccounts` condition):
    heading `# demo` mono 13px `muted` + mode label (`Dev Mode`/`Demo`) as
    `Tag Tone=Muted`; three full-width rows (`.btn-ghost` style, left
    aligned): `RoleTag` + login in mono 13px. Clicking fills the form as
    today. No emoji, no per-role colors.

## ReconnectModal (`Components/Layout/ReconnectModal.razor.css`)

CSS only: dialog background `surface`, border `line`, radius 8px, the one
allowed subtle modal shadow; text `fg` 15px; buttons styled like
`.btn-accent`; rejoining animation uses `accent`. Uses the CSS custom
properties (`rgb(var(--accent))` etc.) since it is a scoped stylesheet. Text
stays as is.

## Testing & verification

- **bUnit:**
  - `Logo`: renders `sd` and `_`, size classes for `Md`/`Lg`, `aria-label`.
  - `RoutePath.Segments`: `http://h/` → empty; `http://h/tickets` →
    `[tickets(no href)]`; `http://h/tickets/42?x=1#c` →
    `[tickets(href tickets), 42(no href)]`; `http://h/Admin/` → `[admin]`;
    URL-encoded segment decoded. Component renders `~` for root and marks
    the last segment `aria-current="page"`.
  - `NavMenu`: as `Admin` renders admin/audit items, as `User` renders
    my-tickets and not admin; no `<svg>` inside items; labels lowercased;
    no collapse button.
  - `LanguageDropdown`: trigger shows current code; opening lists all
    supported languages; selected row has `aria-current="true"`.
  - `AuthShell`: renders `~/login` heading and child content.
  - Login: demo rows render `RoleTag` (when enabled).
  - `SettingsPageTests`: no sidebar row.
- **JS (`node --test`):** `prefs.js` removes `sidebarCollapsed` during
  migration.
- **Localization:** new keys (`auth.tagline`, `nav.path`, `nav.openMenu`,
  and any added password-toggle keys) exist in en/uk/de/es/fr; removed keys
  removed from all 5.
- **Grep checks:** no `IsSidebarCollapsed|SetSidebarCollapsedAsync|
  sidebarCollapsed\b(except OBSOLETE_KEYS)|GetIconPath|app-layout-bg|
  bg-gradient|backdrop-blur` in `Components/Layout/**`,
  `Components/Pages/Auth/**`, `Components/Shared/LanguageDropdown.razor`.
- **Build:** solution 0 warnings; `dotnet test` and `npm run test:js` pass;
  `wwwroot/css/app.css` regenerated and committed.
- **Manual in browser** (both themes, `uk` + `en`, accent `lime` + one
  other): login and register at 1440px and 375px; sidebar as Admin,
  Developer, User; route path on home, tickets, ticket details, admin;
  notifications popover with unread/read/empty; language switch from header
  and login; mobile drawer at 375px; reconnect dialog (stop the server).

## Risks

- **Pages still look old inside a new frame.** Expected until Stage 3; the
  `canvas` background next to old gray cards is the visible seam.
- **Lowercasing translated labels.** Some languages (de nouns) read oddly
  lowercased; accepted as part of the terminal style.
- **Removing Ctrl+B.** Users of the hotkey lose it; acceptable since the
  collapsed mode no longer exists.
- **Scoped CSS + tokens.** `.razor.css` files cannot use Tailwind classes;
  they must use `rgb(var(--token))`.
