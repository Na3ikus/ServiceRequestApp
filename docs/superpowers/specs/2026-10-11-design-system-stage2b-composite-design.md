# Design system — Stage 2b: Composite components

**Date:** 2026-10-11
**Status:** Approved in chat
**Scope:** Last part of Stage 2 of the "ops terminal" (B2) redesign. Builds on
Stage 1 (tokens, `.mark`, type scale), 2a (primitives: `Tag`, `Kbd`,
`EmptyState`, …) and 2c (shell, `.btn-accent` / `.btn-ghost` / `.input-field`,
rem type tokens `text-12/13/14/15/20/40`). Branch `redesign/stage-1`.

## Goal

The overlays every page shares — dialogs, toasts, the hotkeys help and the
command palette — use the terminal style, so Stage 3 can restyle pages on top
of finished building blocks.

**Decisions (2026-10-11):** `Breadcrumb` is deleted — the top-bar `RoutePath`
already shows the path. `CommandPalette` is only restyled here; its new
features (i18n, roles, ticket-by-ID) stay in Stage 4.

## Visual rules (binding, from Stage 1/2c)

Token classes only; no gradients, no blur. Dialogs may use the one subtle
modal shadow (as `ReconnectModal`); toasts and popovers use a 1px `line`
border instead. Radius 6px controls, 8px panels. Rem type tokens only, nothing
below 12px. Light theme never uses `accent` as text on white — `accent-ink`.

## Part 1 — Modal, HotkeysModal, Toast

### Modal (`Components/Shared/Modal.razor` + `amodal-*` rules in `wwwroot/app.css`)

The `amodal-*` class names stay (admin tab forms and the audit-log inspector
use `amodal-field/label/input/row-2/...` directly); only their rules change,
so every dialog and admin form gets the new look at once.

- Overlay `canvas/80`, no blur. Box: `surface`, 1px `line`, radius 8px,
  max-width 520px, subtle shadow.
- Header: title 15px `fg` medium; subtitle 13px `muted`. Close button `×`
  (ghost, `muted` → `fg`), `aria-label` = `common.close`.
- Body 15px. Labels 13px `muted`; inputs follow `.input-field` (canvas
  background inside the surface box, `line` border, focus `fg` border).
- Error: 1px `danger` border, radius 6px, 13px `danger`, no fill,
  `role="alert"`.
- Footer: top border `line`, cancel = `.btn-ghost` look, save = `.btn-accent`
  look; saving state shows `SavingText` (translations already end in `...`;
  no spinner).
- New parameter `bool Danger` — save button becomes outline `danger`
  (`.btn-danger`, new in `input.css`). Settings "reset" uses it.
- Removed: `HeaderIcon`, `IconColor` (colored icon tiles) and their usages in
  the admin tabs and audit inspector.
- Esc closes the dialog (same rule as overlay click: `OnClose`); the box gets
  focus when it opens.

### HotkeysModal (`Components/Shared/HotkeysModal.razor`)

- Built on `Modal` (title `settings.hotkeysTitle`, subtitle
  `settings.hotkeysDesc`, footer = single `.btn-ghost` close button).
- Rows: action 15px `fg` left, keys as `Kbd` right (`or` 13px `muted`
  between alternatives), rows separated by dashed `line` borders.
- `HotkeysModal.razor.css` is deleted. `Show()` / `Close()` API unchanged.

### Toast (`Components/UI/Elements/ToastContainer.razor` + `toast*` rules)

- Container positioning and `data-notif-position` rules unchanged.
- Each toast: `surface`, 1px `line`, radius 8px, no shadow, min 300 / max
  400px. Left: mono 13px type label — `ok` (`accent-ink`), `err` / `warn`
  (`danger`), `info` (`muted`). Message 13px `fg`. Close `×` `muted` → `fg`
  with `aria-label` = `common.close`. Icons removed.
- Container has `role="status"` / `aria-live="polite"`.
- Slide in/out animation kept (reduced-motion already neutralises it).
- Duplicate legacy `.toast-*` color rules are deleted.

## Part 2 — CommandPalette, Breadcrumb

### CommandPalette (`Components/Shared/CommandPalette.razor` + `cmd-palette-*`)

- Box like `Modal` (surface, line, 8px, subtle shadow), overlay `canvas/80`.
- Input row: mono `>` prompt, input mono 15px no border, `Kbd Esc` right.
- Results mono 14px; selected row: `›` in `accent-ink`, background `canvas`;
  description 13px `muted`; shortcut as `Kbd`. Item icons removed.
- No results: `EmptyState` with `cmd.noResults`.
- Behavior (Ctrl+K, arrows, Enter, filtering) unchanged.

### Breadcrumb

`Components/Shared/Breadcrumb.razor`, its `.breadcrumb-*` CSS and its two
usages (TicketDetails, AuditLogs) are deleted.

## Testing & verification

- bUnit: Modal (closed renders nothing; title/subtitle; error with
  `role=alert`; saving disabled + text; callbacks; overlay rule; Esc closes;
  `Danger` styles save; no `svg` in header), HotkeysModal (shows rows with
  `Kbd`, close), ToastContainer (label per type, close callback, no `svg`),
  CommandPalette (renders prompt, no item icons), Breadcrumb gone.
- Source scan: no `bg-gradient`, `backdrop-blur`, `shadow-2xl` in the touched
  components.
- Build 0 warnings; `dotnet test`, `npm run test:js` pass; `wwwroot/css/app.css`
  regenerated and committed.
- Manual: both themes; admin modals (products, tech stacks, tags, SLA), audit
  inspector, settings reset, `?` help, toasts in all 4 positions, Ctrl+K.
