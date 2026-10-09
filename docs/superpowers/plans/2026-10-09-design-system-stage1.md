# Design System Stage 1 (Foundation & Settings) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Lay the token/font/build foundation of the "ops terminal" redesign and ship a rebuilt, fully working Settings page.

**Architecture:** Design tokens are CSS variables in `Styles/input.css`, exposed as Tailwind colors. One client script `prefs.js` owns every preference (schema, validation, migration, apply-before-paint); `IPreferencesService` is the typed C# wrapper used by Blazor. The Settings page is rewritten on four small token-based controls. Tailwind compiles as part of `dotnet build`.

**Tech Stack:** .NET 10 Blazor Server, Tailwind CSS 3.4.17 (npm), NUnit + Moq + FluentAssertions + bUnit, Node 22 built-in test runner (`node --test`) for `prefs.js`.

**Spec:** `docs/superpowers/specs/2026-10-09-design-system-stage1-design.md`

## Global Constraints

- Work on branch `redesign/stage-1` off `master`.
- After any change to `Styles/`, `tailwind.config.js`, or Tailwind classes in `.razor`/`.cs`, the compiled CSS must be rebuilt (Task 1 makes `dotnet build` do it; before Task 1 lands run `npm run build:css` in `ServiceDeskSystem/`).
- `wwwroot/css/app.css` is generated — never hand-edit it. `wwwroot/app.css` is hand-written.
- No `JS.InvokeVoidAsync("eval", …)` in new or rewritten code.
- Tokens (dark / light): canvas `#000000`/`#ffffff`; surface `#0a0a0a`/`#fafafa`; line `#1f1f1f`/`#e5e5e5`; fg `#ededed`/`#171717`; muted `#7a7a7a`/`#737373`; accent-ink = accent / `#000000`; on-accent `#000000`; danger `#ff4d4d`/`#dc2626`.
- Accents (dark / light): lime `#c6f432`/`#c6f432` (default); amber `#ffb000`/`#ffc233`; phosphor `#33ff99`/`#5cffad`; ice `#4de8f0`/`#7af0f5`.
- High contrast: muted `#a3a3a3`/`#404040`; line `#3a3a3a`/`#a3a3a3`.
- Density: `--row-py` `0.625rem`/`0.375rem`, `--cell-px` `0.75rem`/`0.5rem` (comfortable/compact).
- Type scale at 100%: body 15px/1.5 Geist; secondary 13px/1.45; path heading Geist Mono 500 20px; section heading Geist Mono 13px; mono data 13px; kbd 12px. Nothing below 12px.
- Radius 6px controls / 8px panels; 1px borders; no gradients, blur or shadows in new code.
- Every user-visible string goes through `L.Translate` with keys present in all 5 languages (en, uk, de, es, fr).
- Accessibility: segmented controls `role="radiogroup"` + arrow keys; toggles `<button role="switch" aria-checked="true|false">` (render the string explicitly — Blazor drops `false` boolean attributes).

## Review Focus

1. **Corrupt / hand-edited localStorage** (e.g. `settings.fontSize = "abc"`, `settings.accentColor = "<script>"`): app must load with defaults, never throw, never write the bad value into the DOM. → Task 3 test `normalize rejects unknown values`.
2. **Theme `system` with OS dark on a cold load**: no light flash, and `IsDarkMode` in C# matches the page after OS theme flips while the app is open. → Task 4 manual check + Task 5 test `OnSystemThemeChanged updates IsDarkMode`.
3. **User with pre-redesign settings** (accent `custom`/`cyan`, `animations=false`, `fontSize=95`, card/hover/bgTexture keys): migrated once, obsolete keys gone, no errors. → Task 3 tests `migrate …`.
4. **Desktop notifications denied by the browser**: toggle reverts to off and a hint is shown; no repeated permission prompts. → Task 7 test `Desktop toggle reverts when permission denied`.
5. **Prerender / JS unavailable** (Blazor prerender, disconnected circuit): Settings and `PreferencesService.LoadAsync` return defaults instead of throwing. → Task 5 test `LoadAsync returns defaults when JS throws`.

---

### Task 1: Tailwind build wired into `dotnet build` and Docker

**Files:**
- Modify: `ServiceDeskSystem/ServiceDeskSystem.csproj`
- Modify: `Dockerfile.web`
- Modify: `.dockerignore`
- Modify: `README.md` (prerequisites: Node 22+)

**Interfaces:**
- Produces: MSBuild property `SkipTailwindBuild` (`true` skips); target `BuildTailwindCss`.

- [ ] **Step 1: Add target `BuildTailwindCss`** in the csproj: `BeforeTargets="BeforeBuild"`, `Condition="'$(SkipTailwindBuild)' != 'true'"`, `Inputs="@(TailwindInput)"` where `TailwindInput` = `Styles/**/*.css;tailwind.config.js;Components/**/*.razor;Components/**/*.cs`, `Outputs="wwwroot/css/app.css"`. Body: `Exec Command="npm --version"` with `ContinueOnError="true"` capturing exit code → `Error Text="Node.js/npm is required to build CSS. Install Node 22+ or build with -p:SkipTailwindBuild=true."` when non-zero; `Exec Command="npm ci"` when `node_modules` doesn't exist; `Exec Command="npm run build:css"`.
- [ ] **Step 2: Verify incremental behavior.** Run `dotnet build ServiceDeskSystem` twice. Expected: first prints Tailwind "Done in …ms"; second skips `BuildTailwindCss` (no Tailwind output). Then `touch ServiceDeskSystem/Styles/input.css` and build again → Tailwind runs.
- [ ] **Step 3: Verify skip flag.** `dotnet build ServiceDeskSystem -p:SkipTailwindBuild=true` → no Tailwind output, build succeeds.
- [ ] **Step 4: Docker.** In `Dockerfile.web` add stage `FROM node:22-alpine AS css`, `WORKDIR /src/ServiceDeskSystem`, copy `ServiceDeskSystem/package*.json` → `npm ci`, copy `ServiceDeskSystem/` → `npm run build:css`. In the build stage, after `COPY . .`, `COPY --from=css /src/ServiceDeskSystem/wwwroot/css/app.css ./ServiceDeskSystem/wwwroot/css/app.css`, and add `-p:SkipTailwindBuild=true` to both `dotnet build` and `dotnet publish`. Add `**/node_modules` to `.dockerignore`.
- [ ] **Step 5: Verify Docker.** `docker compose build web` (service name per `docker-compose.yml`). Expected: success, css stage logs Tailwind "Done".
- [ ] **Step 6: README** — add Node.js 22+ to prerequisites and one line: "`dotnet build` compiles Tailwind; use `-p:SkipTailwindBuild=true` to skip."
- [ ] **Step 7: Commit** — `build: compile Tailwind CSS as part of dotnet build and Docker`.

---

### Task 2: Tokens, fonts, type scale

**Files:**
- Modify: `ServiceDeskSystem/Styles/input.css`
- Modify: `ServiceDeskSystem/tailwind.config.js`
- Modify: `ServiceDeskSystem/Components/App.razor:15-17`

**Interfaces:**
- Produces: Tailwind colors `canvas surface line fg muted accent accent-ink on-accent danger` (all `rgb(var(--x) / <alpha-value>)`); `font-sans` = Geist stack, `font-mono` = Geist Mono stack; CSS utility `.mark`; CSS vars `--row-py`, `--cell-px`; DOM contract `html.dark`, `html[data-accent|data-contrast|data-density|data-motion]`.

- [ ] **Step 1: Verify Cyrillic.** Open `https://fonts.googleapis.com/css2?family=Geist&family=Geist+Mono` in a browser and check for `/* cyrillic */` blocks. If missing for either, substitute Inter (sans) / JetBrains Mono (mono) in every step below and note it in the commit message.
- [ ] **Step 2: Tokens in `input.css`** inside `@layer base`: `:root` (light values), `.dark` (dark values), one `[data-accent="…"]` and `.dark[data-accent="…"]`-equivalent rule per accent (selector: `html[data-accent="amber"]` and `html.dark[data-accent="amber"]`), `html:not([data-accent])` = lime, high-contrast overrides, density vars, `html[data-motion="reduced"] *, …::before, …::after { animation-duration:.01ms!important; animation-iteration-count:1!important; transition-duration:.01ms!important; }`. All color values as space-separated RGB channels (e.g. `--canvas: 255 255 255`). Values: Global Constraints.
- [ ] **Step 3: Base styles** in `@layer base`: `body { background: rgb(var(--canvas)); color: rgb(var(--fg)); font-family: theme(fontFamily.sans); font-size: 15px; line-height: 1.5; }`; `table th, table td { padding: var(--row-py) var(--cell-px); }`.
- [ ] **Step 4: `.mark` utility** in `@layer utilities`: default (light) `background: rgb(var(--accent)); color: rgb(var(--on-accent)); padding: 0 .25em; border-radius: 3px`; `.dark .mark { background: transparent; color: rgb(var(--accent)); padding: 0; }`.
- [ ] **Step 5: Re-theme Quill + autofill blocks** already in `input.css` to tokens (replace every hex with `rgb(var(--…))`: backgrounds → surface/canvas, borders → line, text → fg, icons → muted, hover/active `#3b82f6` → accent-ink, picker radius 12px → 8px, remove picker box-shadow). Delete `.lang-slider*` after `Grep "lang-slider" ServiceDeskSystem/Components` returns nothing.
- [ ] **Step 6: Tailwind config** — remove `brand`; add the 9 token colors; `fontFamily.sans: ['Geist','Inter','system-ui','sans-serif']`, `fontFamily.mono: ['"Geist Mono"','"JetBrains Mono"','ui-monospace','monospace']`; exclude `node_modules` from `content` (replace `./**/*.html` and `./**/*.cs` with `./Components/**/*.cs`, `./wwwroot/**/*.html`).
- [ ] **Step 7: `App.razor`** — replace the five-family link with `family=Geist:wght@400;500;600&family=Geist+Mono:wght@400;500&display=swap`.
- [ ] **Step 8: Verify.** `dotnet build ServiceDeskSystem`; Grep `wwwroot/css/app.css` for `--canvas:` and `.mark` → both present. Run app, open `/login` in both themes: body font is Geist, no console 404s for fonts.
- [ ] **Step 9: Commit** — `feat(ui): add design tokens, Geist fonts and type scale`.

---

### Task 3: `prefs.js` — schema, normalize, migrate (pure, unit-tested)

**Files:**
- Create: `ServiceDeskSystem/wwwroot/js/prefs.js`
- Create: `ServiceDeskSystem/wwwroot/js/__tests__/prefs.test.js`
- Modify: `ServiceDeskSystem/package.json` (script `"test:js": "node --test wwwroot/js/__tests__/"`)

**Interfaces:**
- Produces (browser global `window.prefs`, also `module.exports` when `typeof module !== 'undefined'`):
  - `SCHEMA`: `{ [name]: { key: string, def: string, values: string[] } }` for names
    `theme`(`theme`; light/dark/system; `system`), `accent`(`settings.accentColor`; lime/amber/phosphor/ice; `lime`), `scale`(`settings.fontSize`; 90/100/110/120; `100`), `contrast`(`settings.contrastMode`; standard/high; `standard`), `reducedMotion`(`settings.reducedMotion`; true/false; def computed from OS, schema def `false`), `density`(`settings.tableDensity`; comfortable/compact), `contentWidth`(`settings.contentWidth`; narrow/standard/wide), `dateFormat`(`settings.dateFormat`; `DD/MM/YYYY`/`MM/DD/YYYY`/`YYYY-MM-DD`), `defaultView`(`settings.defaultView`; TABLE/KANBAN), `sound`(`settings.soundEnabled`; true/false; `true`), `desktopNotifications`(`settings.desktopNotifications`; true/false; `false`), `toastPosition`(`settings.notifPosition`; top-right/top-center/bottom-right/bottom-center), `shortcuts`(`settings.keyboardShortcuts`; true/false; `true`).
  - `normalize(name: string, raw: string|null): string` — returns raw if allowed, else default. Unknown name → throws `Error`.
  - `migrate(storage: {getItem,setItem,removeItem}): void` — idempotent; marker key `prefs.migrated = "1"`.
  - `OBSOLETE_KEYS: string[]` — the 10 keys listed in the spec plus `settings.animations`.

- [ ] **Step 1: Write failing tests** (`node:test` + `node:assert/strict`, in-memory storage stub):
  - `normalize returns allowed value` — `normalize('accent','amber') === 'amber'`.
  - `normalize rejects unknown values` — `normalize('accent','custom') === 'lime'`, `normalize('scale','abc') === '100'`, `normalize('accent','<script>') === 'lime'`, `normalize('accent', null) === 'lime'`.
  - `normalize throws on unknown name`.
  - `migrate maps legacy accent` — `custom`, `blue`, `cyan`, `slate` → stored `lime`; `amber` kept.
  - `migrate converts animations=false to reducedMotion=true` and removes `settings.animations`; `animations=true` → no `reducedMotion` key written.
  - `migrate snaps fontSize` — 85→90, 95→100 (tie rounds up), 105→110, 115→120, `abc`→ key removed.
  - `migrate removes all OBSOLETE_KEYS`.
  - `migrate is idempotent` — running twice on a migrated store leaves it unchanged (marker present).
- [ ] **Step 2: Run** `npm run test:js` in `ServiceDeskSystem/` → FAIL (module not found).
- [ ] **Step 3: Implement** the schema, `normalize`, `migrate` in `prefs.js` (IIFE, no DOM access in these functions).
- [ ] **Step 4: Run** `npm run test:js` → all PASS.
- [ ] **Step 5: Commit** — `feat(prefs): add preference schema, validation and legacy migration`.

---

### Task 4: `prefs.js` runtime — apply before paint, replace legacy init

**Files:**
- Modify: `ServiceDeskSystem/wwwroot/js/prefs.js`
- Modify: `ServiceDeskSystem/wwwroot/js/theme.js`
- Modify: `ServiceDeskSystem/wwwroot/js/sidebar.js:91-128` (delete startup IIFE; hotkey code untouched)
- Modify: `ServiceDeskSystem/wwwroot/js/notification.js`
- Modify: `ServiceDeskSystem/Components/Layout/MainLayout.Notifications.cs:282`
- Modify: `ServiceDeskSystem/Components/App.razor` (load `js/prefs.js` as the first `<script>` in `<head>`, before `theme.js`)
- Test: `ServiceDeskSystem/wwwroot/js/__tests__/prefs.test.js`

**Interfaces:**
- Consumes: Task 3 `SCHEMA`, `normalize`, `migrate`.
- Produces on `window.prefs`:
  - `init(): void` — `migrate(localStorage)`, `applyAll()`, subscribe `matchMedia('(prefers-color-scheme: dark)')` and `('(prefers-reduced-motion: reduce)')` change events (re-apply theme only when stored theme is `system`; motion only when no stored `reducedMotion`). Called at the bottom of `prefs.js`.
  - `get(name): string` — normalized stored value; for `reducedMotion` with no stored key returns OS value.
  - `getAll(): Record<string,string>` — every schema name.
  - `set(name, value): boolean` — false (and no write) if value not allowed; else persist, apply, return true.
  - `reset(): void` — remove every schema key (including `theme`), then `applyAll()`.
  - `resolveTheme(): 'light'|'dark'`.
  - `onSystemThemeChange(dotNetRef): void` — on OS change while theme is `system`, calls `dotNetRef.invokeMethodAsync('OnSystemThemeChanged', isDark)`.
  - `apply(name, value)` mapping: theme → toggle `html.dark` via `resolveTheme` (adds `theme-transitioning` for 220ms only when called from `set`); accent → `data-accent`; scale → `documentElement.style.fontSize = value + '%'`; contrast → `data-contrast`; reducedMotion → `data-motion` = `reduced`|`full`; density → `data-density`; contentWidth → `data-content-width`; toastPosition → `data-notif-position`; others: no DOM effect.
- Produces on `window.notificationManager`: `notifyNew(title: string, body: string): Promise<void>` — plays sound iff `prefs.get('sound') === 'true'`; shows `new Notification(title, { body })` iff `prefs.get('desktopNotifications') === 'true'`, `Notification.permission === 'granted'` and `document.hidden`. `requestDesktopPermission(): Promise<string>` returns the resulting permission.

- [ ] **Step 1: Failing tests** with a minimal fake `document.documentElement` (object with `classList` Set-backed, `setAttribute/getAttribute`, `style`) and fake `matchMedia`:
  - `set rejects invalid value without writing` — `set('density','huge') === false`, storage unchanged.
  - `set applies attribute` — `set('density','compact')` → attribute `data-density="compact"`.
  - `theme system follows OS` — OS dark + stored `system` → `html.dark` present after `init()`.
  - `reset restores defaults` — after setting accent amber, `reset()` → `get('accent') === 'lime'`, `data-accent="lime"`.
- [ ] **Step 2: Run** `npm run test:js` → new tests FAIL.
- [ ] **Step 3: Implement** runtime functions (guard DOM access so the module still loads under Node).
- [ ] **Step 4: Run** `npm run test:js` → PASS.
- [ ] **Step 5: Slim `theme.js`** — `themeManager.getTheme()` → `prefs.get('theme')`, `setTheme(t)` → `prefs.set('theme', t)`; delete `restoreAllVisualSettings` and the `initialize()` call at the bottom.
- [ ] **Step 6: Delete the startup IIFE in `sidebar.js`** (lines 91–128). Keep everything else.
- [ ] **Step 7: Notifications** — add `notifyNew` / `requestDesktopPermission` to `notification.js` (keep `playNewNotificationSound` as the internal sound function). Change `MainLayout.Notifications.cs:282` to `InvokeVoidAsync("notificationManager.notifyNew", <newest notification title>, <newest notification message>)` using the notification object already in scope there.
- [ ] **Step 8: Manual check** (run app): set OS to dark, clear `theme` key, hard-reload `/` → no white flash; flip OS theme → page follows live; in DevTools set `settings.accentColor='custom'`, reload → `data-accent="lime"`, key rewritten; DevTools console shows no errors.
- [ ] **Step 9: Commit** — `feat(prefs): apply preferences before first paint and drop legacy init code`.

---

### Task 5: `IPreferencesService` + `ThemeService` on `prefs`

**Files:**
- Create: `ServiceDeskSystem.Application/Services/Preferences/IPreferencesService.cs`
- Create: `ServiceDeskSystem.Application/Services/Preferences/PreferencesService.cs`
- Create: `ServiceDeskSystem.Application/Services/Preferences/UserPreferences.cs`
- Create: `ServiceDeskSystem.Application/Services/Preferences/PreferenceSchema.cs`
- Modify: `ServiceDeskSystem.Application/Services/Theme/ThemeService.cs`
- Modify: `ServiceDeskSystem.Application/DependencyInjection.cs:58`
- Modify: `ServiceDeskSystem.Tests/Frontend/Components/ComponentTestHost.cs` (register `Mock<IPreferencesService>` returning `UserPreferences.Defaults`)
- Test: `ServiceDeskSystem.Tests/Backend/Unit/PreferencesServiceTests.cs`, `ServiceDeskSystem.Tests/Backend/Unit/ThemeServiceTests.cs`

**Interfaces:**
- Consumes: Task 4 `prefs.getAll`, `prefs.set`, `prefs.reset`, `prefs.get('theme')`, `prefs.resolveTheme`, `prefs.onSystemThemeChange`.
- Produces:
  - `public sealed record UserPreferences(string Accent, int Scale, bool HighContrast, bool ReducedMotion, string Density, string ContentWidth, string DateFormat, string DefaultView, bool Sound, bool DesktopNotifications, string ToastPosition, bool Shortcuts)` with `public static UserPreferences Defaults { get; }` (lime, 100, false, false, comfortable, standard, `DD/MM/YYYY`, TABLE, true, false, top-right, true).
  - `public static class PreferenceSchema` — `const string` names matching `prefs.js` (`Accent = "accent"`, `Scale = "scale"`, … excluding theme) and `IReadOnlyDictionary<string, IReadOnlyList<string>> Allowed`.
  - `public interface IPreferencesService { event EventHandler? Changed; UserPreferences Current { get; } Task<UserPreferences> LoadAsync(); Task SetAsync(string name, string value); Task ResetAsync(); string FormatDate(DateTime value); }`
  - `ThemeService`: `[JSInvokable] public void OnSystemThemeChanged(bool isDark)`; registers itself via `prefs.onSystemThemeChange` in `InitializeAsync`; implements `IDisposable` to dispose its `DotNetObjectReference`.

- [ ] **Step 1: Failing tests — `PreferencesServiceTests`** (Moq `IJSRuntime`; mock `InvokeAsync<Dictionary<string,string>>("prefs.getAll", …)`):
  - `LoadAsync maps JS values` — `{accent:"amber", scale:"110", contrast:"high", reducedMotion:"true", sound:"false", …}` → record with Accent `amber`, Scale `110`, HighContrast `true`, ReducedMotion `true`, Sound `false`.
  - `LoadAsync falls back per field on invalid values` — `scale:"abc"`, `accent:"custom"` → `100`, `lime`; other fields unaffected.
  - `LoadAsync returns defaults when JS throws` — `JSException`/`InvalidOperationException` → `UserPreferences.Defaults`, no throw.
  - `SetAsync rejects unknown name` and `SetAsync rejects disallowed value` → `ArgumentException`, `prefs.set` never invoked.
  - `SetAsync updates Current and raises Changed` — `SetAsync("density","compact")` → `Current.Density == "compact"`, event raised once, `prefs.set` invoked with `("density","compact")`.
  - `ResetAsync restores defaults and raises Changed`.
  - `FormatDate` `[TestCase("DD/MM/YYYY","09/10/2026")] [TestCase("MM/DD/YYYY","10/09/2026")] [TestCase("YYYY-MM-DD","2026-10-09")]` for `new DateTime(2026,10,9)`.
- [ ] **Step 2: Failing tests — `ThemeServiceTests`:** `OnSystemThemeChanged updates IsDarkMode when preference is system` (raises `ThemeChanged`); `OnSystemThemeChanged ignored when preference is explicit light`; `SetTheme calls prefs.set with theme`.
- [ ] **Step 3: Run** `dotnet test ServiceDeskSystem.Tests --filter "FullyQualifiedName~PreferencesServiceTests|FullyQualifiedName~ThemeServiceTests"` → FAIL (types missing).
- [ ] **Step 4: Implement** the four Preferences files and the `ThemeService` changes (replace its `eval`/`localStorage` calls with `prefs.get('theme')`, `prefs.resolveTheme`, `prefs.set('theme', …)`; public `IThemeService` API unchanged). `FormatDate` uses `CultureInfo.InvariantCulture`.
- [ ] **Step 5: Register** `services.AddScoped<IPreferencesService, PreferencesService>();` next to `ThemeService`; add the mock to `ComponentTestHost`.
- [ ] **Step 6: Run** the filtered tests → PASS; then full `dotnet test ServiceDeskSystem.Tests` → no regressions.
- [ ] **Step 7: Commit** — `feat(prefs): add IPreferencesService and route ThemeService through prefs.js`.

---

### Task 6: Settings controls

**Files:**
- Create: `ServiceDeskSystem/Components/Pages/Settings/Controls/SettingRow.razor`
- Create: `ServiceDeskSystem/Components/Pages/Settings/Controls/SegmentedControl.razor` (+ `.razor.cs`)
- Create: `ServiceDeskSystem/Components/Pages/Settings/Controls/ToggleSwitch.razor`
- Create: `ServiceDeskSystem/Components/Pages/Settings/Controls/AccentPicker.razor`
- Test: `ServiceDeskSystem.Tests/Frontend/Components/Settings/SettingsControlsTests.cs`

**Interfaces:**
- Produces:
  - `SettingRow` — `[Parameter] string Label`, `string? Description`, `RenderFragment ChildContent`. Row: flex, `justify-between`, `py-3`, `border-b border-dashed border-line`; label 15px `text-fg`, description 13px `text-muted`.
  - `SegmentedControl` — `[Parameter] string Label` (aria-label), `IReadOnlyList<(string Value, string Text)> Options`, `string Value`, `EventCallback<string> ValueChanged`. Renders `div role="radiogroup"` of `button role="radio" aria-checked="true|false" tabindex` (roving: 0 on selected, -1 others); ArrowRight/ArrowDown → next (wraps), ArrowLeft/ArrowUp → previous, Home/End; selection fires `ValueChanged`. Style: `font-mono text-[13px] border border-line rounded-md`, selected `bg-fg text-canvas`.
  - `ToggleSwitch` — `[Parameter] string Label`, `bool Value`, `EventCallback<bool> ValueChanged`, `bool Disabled`. `button role="switch" aria-checked="true|false" aria-label=Label`; on → `bg-accent`, knob `bg-on-accent`.
  - `AccentPicker` — `[Parameter] string Value`, `EventCallback<string> ValueChanged`. A `SegmentedControl`-like radiogroup of 4 swatches (26×26, `rounded-md`, selected `outline outline-1 outline-fg outline-offset-2`), each `aria-label` = translated accent name (`settings.accent.lime` etc.); swatch color via `style="background: <hex>"` using the dark-theme hex from Global Constraints.

- [ ] **Step 1: Failing bUnit tests:**
  - `SegmentedControl marks selected option` — Value `compact` → that button has `aria-checked="true"`, other `aria-checked="false"` (attribute present, not omitted).
  - `SegmentedControl click raises ValueChanged`.
  - `SegmentedControl ArrowRight selects next and wraps at end`.
  - `ToggleSwitch renders aria-checked false explicitly` and `click raises ValueChanged(true)`.
  - `ToggleSwitch Disabled does not raise`.
  - `AccentPicker renders four radios with labels` and `click amber raises ValueChanged("amber")`.
- [ ] **Step 2: Run** `dotnet test ServiceDeskSystem.Tests --filter FullyQualifiedName~SettingsControlsTests` → FAIL.
- [ ] **Step 3: Implement** the four components (token classes only).
- [ ] **Step 4: Run** → PASS.
- [ ] **Step 5: Commit** — `feat(settings): add token-based settings controls`.

---

### Task 7: Settings page rewrite

**Files:**
- Rewrite: `ServiceDeskSystem/Components/Pages/Settings/Settings.razor`, `Settings.razor.cs`
- Modify: `ServiceDeskSystem/ServiceDeskSystem.csproj` (`<Version>1.0.0</Version>`)
- Modify: `ServiceDeskSystem/wwwroot/js/sidebar.js` (add `showHotkeysHelp`)
- Modify: language files under `ServiceDeskSystem.Application/Services/Localization/LanguagePack/{lang}/settings.json` (new keys, all 5 languages)
- Test: `ServiceDeskSystem.Tests/Frontend/Components/Settings/SettingsPageTests.cs`

**Interfaces:**
- Consumes: Task 5 `IPreferencesService`, `IThemeService`; Task 6 controls; existing `ILocalizationService.SetLanguage`, `IEmailSender.CheckConnectionAsync`, `health/db` endpoint; `notificationManager.requestDesktopPermission` (Task 4).
- Produces: route `/settings` unchanged.

Structure (mockup `settings-page-v2.html`): `max-w-3xl`; heading `<h1 class="font-mono text-[20px] font-medium"><span class="text-muted">~/</span>settings</h1>`; sections with `<h2 class="font-mono text-[13px] text-muted mt-6 mb-1"># appearance</h2>` etc. Rows:
- `# appearance`: Theme (segmented light/dark/system → `Theme.SetTheme`), Accent (`AccentPicker`), Interface scale (segmented 90/100/110/120 %), High contrast (toggle), Reduce motion (toggle).
- `# layout`: Density, Content width, Sidebar (segmented full/compact → `Theme.SetSidebarCollapsedAsync`, with kbd hint `Ctrl+B`).
- `# locale`: Language (segmented of `L.SupportedLanguages` codes → `L.SetLanguage`), Date format (description shows `Prefs.FormatDate(DateTime.Today)`).
- `# tickets`: Default view.
- `# notifications`: Sound, Desktop notifications, Toast position.
- `# keyboard`: Shortcuts toggle + button "show all" (`<kbd>?</kbd>`) that calls a new `sidebarManager.showHotkeysHelp()` (add to `sidebar.js`: `document.dispatchEvent(new CustomEvent('show-hotkeys-help'))`) via `JS.InvokeVoidAsync`; no `eval`.
- `# system [admin]` (only when `CurrentUser.Role == Admin`): Database `[ok]`/`[down]` (`.mark` on ok, `text-danger` on down), SMTP same with message as description, Version = `AssemblyInformationalVersionAttribute` of the web assembly, "re-check" button.
- Footer: "reset to defaults" → confirm via existing `Modal` component → `Prefs.ResetAsync()` + `Theme.SetTheme("system")`.

Desktop toggle on → `requestDesktopPermission()`; result `granted` → `SetAsync("desktopNotifications","true")`; otherwise keep off and show description hint key `settings.desktopDenied`.

- [ ] **Step 1: Failing bUnit tests** (`ComponentTestHost`, plus mocks for `IPreferencesService`, `IHttpClientFactory`, `IEmailSender`):
  - `Renders all sections for admin` — headings `# appearance`, `# layout`, `# locale`, `# tickets`, `# notifications`, `# keyboard`, `# system` present.
  - `Hides system section for non-admin` (`UserRole.User`).
  - `Clicking compact density calls SetAsync("density","compact")`.
  - `Clicking amber accent calls SetAsync("accent","amber")`.
  - `Clicking dark theme calls Theme.SetTheme("dark")`.
  - `Desktop toggle reverts when permission denied` — JSInterop setup `requestDesktopPermission` → `"denied"`; `SetAsync("desktopNotifications", …)` never called; toggle `aria-checked="false"`; hint key `settings.desktopDenied` rendered.
  - `Reset asks for confirmation before calling ResetAsync` — click reset → `ResetAsync` not called; confirm → called once.
  - `Renders defaults when LoadAsync returns Defaults` (prerender path).
- [ ] **Step 2: Run** `dotnet test ServiceDeskSystem.Tests --filter FullyQualifiedName~SettingsPageTests` → FAIL.
- [ ] **Step 3: Implement** the page and code-behind (one `OnInitializedAsync`: auth redirect as today, `Prefs.LoadAsync()`, admin health check; one handler per control that awaits `Prefs.SetAsync(PreferenceSchema.X, value)`). No `eval`, no per-setting `Apply*`.
- [ ] **Step 4: Localization keys** — add every key the page uses to `{lang}/settings.json` for en, uk, de, es, fr (section titles stay literal `# appearance` etc. — they're part of the terminal style and not translated; labels/descriptions are translated). Accent names: `settings.accent.lime|amber|phosphor|ice`.
- [ ] **Step 5: Run** filtered tests → PASS; full `dotnet test` → PASS.
- [ ] **Step 6: Manual check** — run app as admin and as user, both themes, `uk` and `en`: every control changes the UI immediately and survives reload; density changes the Admin users table rows; reduce motion stops the notification bell animation; desktop notification shows when tab hidden and a notification arrives; denied flow shows the hint.
- [ ] **Step 7: Commit** — `feat(settings): rebuild settings page on preferences service`.

---

### Task 8: Localization cleanup

**Files:**
- Modify: `ServiceDeskSystem.Application/Services/Localization/LocalizationService.cs:88` (sort files: `.OrderBy(f => f, StringComparer.Ordinal)`)
- Modify: `LanguagePack/{lang}.json`, `{lang}/settings.json`, `{lang}/system.json` for all 5 languages
- Test: `ServiceDeskSystem.Tests/Backend/Unit/LocalizationFilesTests.cs`

**Interfaces:**
- Consumes: Task 7's final key set.

- [ ] **Step 1: Failing tests** (read JSON files from `Path.Combine(AppContext.BaseDirectory, "Localization", "LanguagePack")`, flatten nested objects to dotted keys the same way `LoadJsonFileAsync` does):
  - `NoKeyDefinedInMoreThanOneFile` `[TestCase]` per language — lists offending keys in the failure message.
  - `SettingsKeysHaveParityAcrossLanguages` — the set of `settings.*` keys in each language equals the `en` set.
  - `NoObsoleteSettingsKeys` — none of the keys for removed settings exist (prefixes: `settings.surface`, `settings.cardStyle`, `settings.hoverStyle`, `settings.bgTexture`, `settings.borderRadius`, `settings.fontFamily`, `settings.sidebarColor`, `settings.pageTransition`, `settings.badgeStyle`, `settings.customAccent`, `settings.animations`; verify exact key names in `en/settings.json` before writing the list).
- [ ] **Step 2: Run** `dotnet test ServiceDeskSystem.Tests --filter FullyQualifiedName~LocalizationFilesTests` → FAIL (duplicates/obsolete keys exist).
- [ ] **Step 3: Clean files** — delete obsolete keys; for each duplicate keep one definition (prefer `{lang}/settings.json` for `settings.*`), choosing the value currently displayed by the app (the one loaded last under today's unsorted order on Windows — check by running the app once before editing). Add the sort in `LocalizationService`.
- [ ] **Step 4: Run** filtered tests → PASS; existing `LocalizationServiceTests` → PASS.
- [ ] **Step 5: Commit** — `chore(i18n): remove obsolete settings keys, dedupe and load files deterministically`.

---

### Task 9: Delete dead and conflicting CSS

**Files:**
- Modify: `ServiceDeskSystem/wwwroot/app.css`
- Modify: `ServiceDeskSystem/Components/Layout/NavMenu.razor.css:335-393`

- [ ] **Step 1: Re-map sections** — Grep `wwwroot/app.css` for section comment headers; confirm each range from the spec's CSS list against current line numbers.
- [ ] **Step 2: For each block to delete, prove it's unused** — Grep its class names / `data-*` attributes in `ServiceDeskSystem/Components` and `ServiceDeskSystem/wwwroot/js`. Only blocks with zero hits (or hits only in code already removed by Tasks 4/7) are deleted. Blocks: old accent system; settings swatches/preview/slider; card/hover/texture/mesh; font/radius/transition/badge/custom-accent/old contrast variants (keep `data-content-width` and `data-notif-position`); old settings controls; `html[data-surface="solid"]` overrides; `admin-tab-btn`/`admin-stat-card`/`admin-quick-nav`.
- [ ] **Step 3: Delete** those blocks and the four `data-sidebar-color` schemes in `NavMenu.razor.css`.
- [ ] **Step 4: Verify** — `Grep 'data-(card-style|hover-style|bg-texture|surface|radius|font|transition|badge-style|sidebar-color)'` across `ServiceDeskSystem/Components`, `ServiceDeskSystem/wwwroot/app.css`, `ServiceDeskSystem/wwwroot/js` → no matches. `dotnet build` succeeds. Visual pass in both themes over: `/`, `/tickets`, `/ticket/{id}`, `/create-ticket`, `/admin` (each tab), `/audit-logs`, `/statistics`, `/calendar`, `/profile`, `/settings`, `/login` — no page lost layout (missing spacing/columns); color changes are expected.
- [ ] **Step 5: Commit** — `chore(css): remove styles for removed appearance options`.

---

### Task 10: Final verification

- [ ] **Step 1:** Clean clone build: `git clean -xdf ServiceDeskSystem/node_modules ServiceDeskSystem/bin ServiceDeskSystem/obj` (confirm with user first — destructive), then `dotnet build` → Tailwind runs via `npm ci`.
- [ ] **Step 2:** `dotnet test` (whole solution) and `npm run test:js` → all PASS.
- [ ] **Step 3:** `docker compose build` → success.
- [ ] **Step 4:** Manual checklist from the spec's "Testing & verification → Manual in browser" section, in `uk` and `en`, both themes; additionally the five Review Focus items.
- [ ] **Step 5:** Request whole-branch code review (superpowers:requesting-code-review), address findings, then superpowers:finishing-a-development-branch.
