# Design system — Light theme accent colour

**Date:** 2026-10-11
**Status:** Approved in chat (HTML mockup `audit-light.html`, "colour" variant)
**Scope:** App-wide tokens and a few shared components. Do together with
Stage 3e-2 (audit) or right after it, before Stage 3g.

## Problem

In the light theme `--accent-ink` is pure black, so every accent spot
(logo cursor `_`, active nav `›`, `● live`, `ok:` lines, `+` diff lines,
accent tags) turns black and the light theme looks colourless next to the
dark one.

## Decision

1. **Coloured accent ink in light mode.** `--accent-ink` becomes a dark
   shade of the current accent (≥ 4.5:1 on white), set per accent in
   `Styles/input.css`:
   - lime (default): `77 124 15`
   - amber: `180 83 9`
   - phosphor: `4 120 87`
   - ice: `14 116 144`

   Dark mode keeps `--accent-ink: var(--accent)`. `--on-accent` (text on
   an accent fill) stays black.
2. **Selected segments are filled with the accent** in light mode
   (`SegmentedControl` checked option: `bg-accent text-on-accent`) instead
   of the neutral fill. Dark mode unchanged unless it looks wrong there.
3. **Active sidebar item** gets a soft accent background in light mode
   (accent at ~22 % opacity) next to the existing `›` marker.
4. **Open / editing rows** (admin `EditRow`, the open audit event, other
   "selected row" surfaces that use `bg-surface` for that purpose) use a
   light accent tint (accent at ~10 %, hover ~7 %) in light mode instead
   of `bg-surface`. The 2 px accent bar on the left stays.

Introduce the tints as tokens or Tailwind classes (e.g. `bg-accent/10`)
rather than one-off colours; check every accent choice (lime, amber,
phosphor, ice) in light and dark.

## Checks

- Contrast: accent-ink text on white and on the 10 % tint ≥ 4.5:1.
- Places that relied on black accent ink in light mode (the
  `html:not(.dark)` accent tag rule with an accent fill and black border,
  `.mark`, legacy `!important` accent overrides in `input.css`) still read
  well.
- Existing bUnit tests that assert classes on `SegmentedControl` / nav /
  `EditRow` are updated, plus `prefs` JS tests if accent handling changes.
- Browser check: home, tickets, ticket details, admin, audit, settings in
  light mode with all four accents.
