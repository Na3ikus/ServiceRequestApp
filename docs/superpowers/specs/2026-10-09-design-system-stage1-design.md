# Design system — Stage 1: Foundation & Settings

**Date:** 2026-10-09
**Status:** Draft — awaiting review
**Scope:** Stage 1 of 4 of the "ops terminal" (B2) redesign of ServiceDeskSystem.

## Goal

ServiceDeskSystem is an exhibition project whose UI reads as a generic AI-made
Tailwind template (indigo gradients, glassmorphism, mesh backgrounds, Inter,
dozens of cosmetic options). The redesign gives it a minimal Linear/Vercel-like
"ops terminal" identity: black/white surfaces, monospace data, one phosphor
accent used only where attention is needed.

Stage 1 lays the foundation every later stage builds on and rebuilds the
Settings page. After Stage 1:

- Design tokens exist and are usable as Tailwind classes.
- Fonts are Geist + Geist Mono; the other five font families are gone.
- The CSS build cannot be forgotten: `dotnet build` and Docker compile Tailwind.
- The Settings page is rebuilt in the new style with a reduced, fully working
  set of preferences, persisted and applied before first paint.
- Dead and conflicting CSS for removed features is deleted.

**Not in Stage 1** (later stages): restyling the shell, pages and components;
replacing the `BaseComponent` color helpers; deleting `ticket-status.css`;
SLA timers and SLA date fixes; path headings and kbd hints outside Settings.
Pages other than Settings will look visually mixed between Stage 1 and Stage 2;
that is expected.

### Overall stage plan (for context)

1. **Foundation & Settings** — this document.
2. **Shell & core components** — MainLayout, NavMenu, Login/Register, Modal,
   Toast, CommandPalette, HotkeysModal; `StatusTag`/`PriorityTag`/`TypeTag`,
   `PageHeader`, `Panel`, `Kbd`, `Avatar`, `EmptyState`; replace
   `BaseComponent` color helpers; delete `ticket-status.css`.
3. **Pages** — tickets, dashboards, calendar, profile, admin, audit logs,
   statistics (Chart.js on tokens + re-render on theme change).
4. **Signature features** — SLA date correctness (backend), live SLA timers,
   route-path headings everywhere, kbd hints, command palette (i18n, roles,
   ticket-by-ID).

## Visual system (approved)

- **Direction:** B2 "full terminal". Lowercase monospace navigation with `›`
  active marker; page heading is the real route path (`~/settings`), not
  translated; status as bracket tags (`[ok]`, `[at-risk]`); dashed row
  dividers; logo `sd_` with an accent-colored cursor.
- **Fonts:** Geist (UI text), Geist Mono (navigation, headings, IDs, numbers,
  tags, kbd, segmented controls).
- **Type scale** (at 100% interface scale; the user found the 13/11px mockup
  too small):
  | Role | Font | Size / line-height |
  |---|---|---|
  | Body, control labels | Geist | 15px / 1.5 |
  | Descriptions, secondary | Geist | 13px / 1.45 |
  | Path heading (`~/settings`) | Geist Mono 500 | 20px |
  | Section heading (`# appearance`) | Geist Mono | 13px |
  | Mono data: IDs, tags, segmented options, timers | Geist Mono | 13px |
  | Kbd chips | Geist Mono | 12px — absolute minimum anywhere |

  Nothing in the UI goes below 12px. Monospace text reads smaller than sans at
  the same size, so mono is never set smaller than 13px except kbd chips.
- **Shape:** radius 6px (controls) / 8px (panels); 1px borders; no gradients,
  no blur, no shadows except a subtle one on modals.
- **Color rule:** color appears only where attention is needed.

### Tokens

Defined as CSS custom properties holding space-separated RGB channels, so
Tailwind opacity modifiers work (`bg-accent/20`).

| Token | Tailwind name | Dark | Light | Use |
|---|---|---|---|---|
| `--canvas` | `canvas` | `#000000` | `#ffffff` | page background |
| `--surface` | `surface` | `#0a0a0a` | `#fafafa` | sidebar, panels, inputs |
| `--line` | `line` | `#1f1f1f` | `#e5e5e5` | borders, dividers |
| `--fg` | `fg` | `#ededed` | `#171717` | primary text |
| `--muted` | `muted` | `#7a7a7a` | `#737373` | secondary text, IDs |
| `--accent` | `accent` | phosphor color | light variant of phosphor color | accent fill / dark-theme accent text |
| `--accent-ink` | `accent-ink` | = `--accent` | `#000000` | text that must read "accented" on canvas |
| `--on-accent` | `on-accent` | `#000000` | `#000000` | text placed on an accent fill |
| `--danger` | `danger` | `#ff4d4d` | `#dc2626` | SLA breached, destructive actions |

Accent values (`html[data-accent]`):

| Accent | Dark `--accent` | Light `--accent` |
|---|---|---|
| `lime` (default) | `#c6f432` | `#c6f432` |
| `amber` | `#ffb000` | `#ffc233` |
| `phosphor` | `#33ff99` | `#5cffad` |
| `ice` | `#4de8f0` | `#7af0f5` |

Light theme uses the accent as a highlighter fill under black text; it is never
used as text color on white. A `.mark` utility encodes this:
dark → `color: accent`, light → `background: accent; color: on-accent`.

High contrast (`html[data-contrast="high"]`): `--muted` moves to
`#a3a3a3` (dark) / `#404040` (light); `--line` to `#3a3a3a` / `#a3a3a3`.

Density (`html[data-density]`): `--row-py` = `0.625rem` (comfortable) /
`0.375rem` (compact); `--cell-px` = `0.75rem` / `0.5rem`. Stage 1 applies them to
all `<table>` cells globally; div-based lists adopt them as they are restyled.

Reduced motion (`html[data-motion="reduced"]`): all animations and transitions
set to `0.01ms`, matching the existing `prefers-reduced-motion` block.

Tailwind config: `theme.extend.colors` maps each token name to
`rgb(var(--token) / <alpha-value>)`; `fontFamily.sans` = Geist stack,
`fontFamily.mono` = Geist Mono stack. The unused `brand` palette is removed.
Stock Tailwind palettes stay available because pages still use them until
Stage 2/3.

**Font verification step:** before switching, confirm Geist and Geist Mono
include Cyrillic on Google Fonts. If either does not, fall back to Inter /
JetBrains Mono for that role (both have Cyrillic). Ukrainian is a primary
language of the app, so this must not regress.

## Preferences

### Final list

| Setting | Values | Default | Storage key | Effect |
|---|---|---|---|---|
| Theme | `light` / `dark` / `system` | `system` | `theme` | `.dark` class; `system` follows OS **and reacts to OS changes** |
| Accent | `lime` / `amber` / `phosphor` / `ice` | `lime` | `settings.accentColor` | `data-accent` |
| Interface scale | `90` / `100` / `110` / `120` | `100` | `settings.fontSize` | `html` font-size % |
| High contrast | bool | `false` | `settings.contrastMode` (`high`/`standard`) | `data-contrast` |
| Reduce motion | bool | OS preference | `settings.reducedMotion` | `data-motion` |
| Density | `comfortable` / `compact` | `comfortable` | `settings.tableDensity` | `data-density` |
| Content width | `narrow` / `standard` / `wide` | `standard` | `settings.contentWidth` | `data-content-width` (existing CSS) |
| Sidebar | `full` / `compact` | `full` | `sidebarCollapsed` | existing `ThemeService` |
| Language | en / uk / de / es / fr | existing | existing | existing `LocalizationService` |
| Date format | `DD/MM/YYYY` / `MM/DD/YYYY` / `YYYY-MM-DD` | `DD/MM/YYYY` | `settings.dateFormat` | `IPreferencesService.FormatDate` |
| Default ticket view | `TABLE` / `KANBAN` | `TABLE` | `settings.defaultView` | existing TicketList read |
| Sound | bool | `true` | `settings.soundEnabled` | `notification.js` checks it before playing |
| Desktop notifications | bool | `false` | `settings.desktopNotifications` | browser Notification API, only when tab hidden |
| Toast position | 4 positions | `top-right` | `settings.notifPosition` | `data-notif-position` (existing CSS) |
| Keyboard shortcuts | bool | `true` | `settings.keyboardShortcuts` | existing `sidebar.js` check |

Plus: "Show all shortcuts" button (opens HotkeysModal), "Reset to defaults",
and the admin-only **system** section (DB health, SMTP health, version,
re-check). Version comes from the web assembly's informational version, not a
hardcoded string.

Existing storage keys are kept where the setting survives, so current users
keep their choices. The theme default changes from `light` to `system` (only
affects users who never picked a theme).

### Removed

Surface style, card style, hover style, background texture, badge style,
border radius, sidebar color, page transition, font family, custom hex accent,
accent presets other than the four above.

### Migration (runs once at boot, in `prefs.js`)

- `settings.accentColor` not in the four accents (incl. `custom`, `blue`, …) → `lime`.
- `settings.animations === "false"` → `settings.reducedMotion = "true"`; then delete `settings.animations`.
- `settings.fontSize` (85–115) → nearest of 90/100/110/120.
- Delete obsolete keys: `settings.cardStyle`, `settings.hoverStyle`,
  `settings.bgTexture`, `settings.surfaceStyle`, `settings.borderRadius`,
  `settings.fontFamily`, `settings.sidebarColor`, `settings.pageTransition`,
  `settings.badgeStyle`, `settings.customAccentHex`.
- Remove the matching obsolete `data-*` attributes.

## Architecture

### `wwwroot/js/prefs.js` (new) — single source of truth on the client

- A schema: for each preference, storage key, `html` attribute (or apply
  function), allowed values, default.
- `prefs.init()` — runs synchronously in `<head>` before first paint: migration,
  resolve `system` theme via `matchMedia`, apply every preference, subscribe to
  OS color-scheme and reduced-motion changes.
- `prefs.get(name)`, `prefs.set(name, value)` — validate against the schema,
  persist, apply. Invalid values are rejected (no string interpolation into
  `eval`).
- `prefs.reset()` — clears all schema keys and re-applies defaults.
- Replaces the visual-settings code in `theme.js`
  (`restoreAllVisualSettings`) and `sidebar.js` (the startup IIFE). `theme.js`
  keeps only what `ThemeService` calls, delegating to `prefs`. Hotkey code in
  `sidebar.js` stays untouched.

### `IPreferencesService` (new, `ServiceDeskSystem.Application/Services/Preferences/`)

Scoped, wraps `IJSRuntime` the same way `ThemeService` does.

- Typed read of all preferences (`LoadAsync` returns a `UserPreferences` record).
- `SetAsync(name, value)` → `prefs.set`; `ResetAsync()` → `prefs.reset`.
- `event Changed` so components (e.g., TicketList, later SLA timers) can react.
- `string FormatDate(DateTime value)` using the date-format preference. Stage 1
  provides it; pages switch to it as they are restyled in Stage 3.
- Registered next to `ThemeService` in DI.

`ThemeService` keeps its public API; internally it calls `prefs` for the theme
so there is one code path.

### Settings page

- `Settings.razor` / `.razor.cs` rewritten from scratch (currently 862 + 700 lines).
  Code-behind loads once via `IPreferencesService.LoadAsync`, every control
  calls `SetAsync`. No per-setting `Set*/Apply*` methods, no `eval`.
- Layout per the mockup (`.superpowers/brainstorm/994-1791576188/content/settings-page.html`):
  path heading `~/settings`; single column; sections `# appearance`,
  `# layout`, `# locale`, `# tickets`, `# notifications`, `# keyboard`,
  `# system [admin]`; rows with label + optional description + control;
  "reset to defaults" at the bottom with a confirmation.
- Small local components (in `Components/Pages/Settings/`): `SettingRow`,
  `SegmentedControl`, `ToggleSwitch`, `AccentPicker`. They are built on tokens
  and are reusable in Stage 2, where they may move to `Components/UI/`.
- Accessibility: segmented controls are `role="radiogroup"` with arrow-key
  navigation; toggles are `<button role="switch" aria-checked>`; every control
  has a label.
- Desktop notifications: enabling requests `Notification.requestPermission()`;
  if denied, the toggle reverts and shows a hint.

### CSS

- **`Styles/input.css`:** add `@layer base` with token definitions
  (`:root`, `.dark`, `[data-accent=…]`, `[data-contrast=high]`,
  `[data-density]`, `[data-motion=reduced]`), base `body` colors and font
  from tokens, `.mark` utility. Re-theme the Quill and autofill blocks to
  tokens (they're in this file already). Delete `.lang-slider*` (contains a
  blue→purple gradient) once a grep confirms no `.razor` file uses it.
- **`wwwroot/app.css`** (hand-written, 3650 lines) — delete:
  - accent swatches, theme preview cards, live preview, font slider (≈1809–2205);
  - card style, hover style, background texture, mesh (≈2302–2435);
  - font / radius / transition / badge / custom-accent variants and the old
    `data-contrast="high"` rules (≈2687–2871); keep content width and toast
    positions. High contrast is now handled by token overrides;
  - old settings-page controls (≈2872–2988);
  - `html[data-surface="solid"]` overrides (≈3336–3612);
  - old accent system (≈1613–1660), replaced by tokens;
  - `admin-tab-btn` / `admin-stat-card` / `admin-quick-nav` after confirming no usages.

  Line ranges are from the audit and must be re-checked before deleting; each
  removal is verified with a grep for its class/attribute names in
  `Components/**` and `wwwroot/js/**`.
  "DARK MODE FIXES" blocks (≈1142–1496) and `page-hero` rules stay until the
  pages they serve are restyled (Stage 2/3).
- **`NavMenu.razor.css`:** delete the four `data-sidebar-color` schemes.
- **`App.razor`:** replace the five-family Google Fonts link with Geist +
  Geist Mono; load `prefs.js` first in `<head>`.

### Build

- **csproj:** an MSBuild target `BuildTailwindCss` runs before
  `Build`: `npm ci` when `node_modules` is missing, then `npm run build:css`.
  Inputs/outputs declared (`Styles/**`, `tailwind.config.js`, `**/*.razor`,
  `**/*.cs` → `wwwroot/css/app.css`) so incremental builds skip it. Skippable
  with `-p:SkipTailwindBuild=true`. If `npm` is not on PATH, the build fails
  with a clear message instead of silently shipping stale CSS.
- **`Dockerfile.web`:** new `node:22-alpine` stage that copies
  `ServiceDeskSystem/`, runs `npm ci && npm run build:css`, and the .NET stage
  copies the generated `wwwroot/css/app.css` in and builds with
  `-p:SkipTailwindBuild=true`.
- **`.dockerignore`:** add `**/node_modules`.
- **`run-all.ps1` / `.bat`:** nothing needed; they call `dotnet build`.
- `wwwroot/css/app.css` stays committed (keeps `git clone && dotnet run`
  working for people without Node).

### Localization

- New `settings.*` keys for the rebuilt page in all 5 languages
  (en / uk / de / es / fr).
- Delete obsolete `settings.*` keys (~45–50 per language) from the root
  `{lang}.json`, `{lang}/settings.json` and `{lang}/system.json`.
- Fix duplicate keys with different values across files (e.g.
  `settings.animationsDesc` in `en/system.json` vs `en/settings.json`): each
  key lives in exactly one file.
- `LocalizationService` loads subdirectory files in sorted order so loading is
  deterministic.

## Testing & verification

- **Unit (NUnit):**
  - `PreferencesService`: `LoadAsync` maps JS values to the record and falls
    back to defaults on invalid values; `FormatDate` for all three formats;
    `SetAsync` rejects unknown names/values.
  - Localization: no key defined in more than one file per language; every key
    used by the Settings page exists in all 5 languages.
- **Component (bUnit):** Settings renders all sections; admin-only section is
  hidden for non-admins; clicking a segmented option calls `SetAsync` with the
  right arguments.
- **Build:** `dotnet build` from clean regenerates `wwwroot/css/app.css` and it
  contains the token variables and `bg-canvas`; `docker compose build web`
  succeeds.
- **Manual in browser** (both themes, at least uk + en):
  - every setting changes the UI immediately and survives reload;
  - no light flash on reload with OS dark + theme `system`; switching OS theme
    updates the page live;
  - density visibly changes table rows;
  - reduce motion stops animations;
  - reset restores defaults;
  - legacy localStorage values (e.g. accent `custom`, `animations=false`)
    migrate correctly;
  - desktop notification permission flow (granted and denied).

## Risks

- **Mixed look between stages.** Pages keep their old classes until Stage 2/3.
  Mitigation: ship Stage 1 and 2 close together; Stage 1 is mostly invisible
  outside Settings, fonts and base colors.
- **CSS deletion breaks something still in use.** Mitigation: grep every
  removed selector's class/attribute in `Components/**` and `wwwroot/js/**`
  before deleting; visual pass over all pages after.
- **Node becomes a build prerequisite.** Mitigation: clear error message,
  `SkipTailwindBuild` escape hatch, committed CSS output, README note.
