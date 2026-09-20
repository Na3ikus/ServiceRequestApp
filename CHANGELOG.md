# Changelog

All notable changes to ServiceDeskSystem are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-09-20

First stable release. The core was rebuilt on a layered Domain/Application/Infrastructure
architecture, and the ticketing workflow was completed with SLA management, audit logging,
work logs and attachments.

### Added

- **SLA management** — `SlaPolicy` domain entity, `SlaPolicyService`, a background service
  that evaluates breaches (`SlaBackgroundService`), an admin UI for editing policies, and
  ticket triage with priority assessment and breach alerts.
- **Advanced audit logging** — per-field change diffs, severity levels, real-time streaming
  of audit events, and brute-force login detection.
- **Work logs and attachments** — time tracking per ticket via `WorkLogService`, and file
  attachments via `AttachmentService` with a dedicated repository.
- **Analytics** — `AnalyticsController` and extended analytics DTOs for ticket volume,
  resolution times and counts by type.
- **Real-time updates** — SignalR hub delivering live ticket and notification updates,
  replacing polling.
- **Ticket calendar view** with scheduling and calendar-driven notifications.
- **Rich text editing** — Quill-based editor for ticket descriptions and comments.
- **German, Spanish and French localization** alongside the existing English and Ukrainian
  packs; language files split per area (profile, settings, system, tickets).
- **Appearance and settings** — customizable Settings page, system theme detection, accent
  colors, UI density, a flat UI theme, visual effects, and a keyboard shortcuts modal.
- **Rate limiting** on the API, plus response caching for hot read paths.
- Error boundary around the Blazor component tree.
- FluentValidation for request and form validation.
- ~17 new test files covering SLA, work logs, attachments, localization, notifications,
  tags, admin and seed determinism, plus frontend component tests.

### Changed

- Migrated data access to the **repository pattern** with dependency injection throughout;
  service interfaces moved to root namespaces.
- Applied **DDD patterns** in the domain layer; ticket status, priority and type are now
  enums instead of strings.
- Centralized supported languages and the default language in `LocalizationConstants`;
  language selection buttons are generated dynamically.
- Redesigned the admin panel (new sidebar, search and sorting), the CreateTicket page
  (card-based type selection), the header dropdowns and the notification dropdown.
- Optimized the database schema and added indexes for ticket queries; added paging to
  list endpoints.
- Improved JWT and Swagger configuration.
- Demo users are seeded with stable password hashes so seeding is deterministic.

### Removed

- Legacy components and the unused `TicketConstants` class.

### Fixed

- Column sizing and assorted visual defects in ticket tables.
- Logger initialization error.
- All compiler and analyzer warnings (the solution builds clean with
  `AnalysisMode=AllEnabledByDefault`).

## [0.3.0-beta] - 2026-04-04

Notifications Foundation Update — in-app notification system for tickets and comments.

## [0.2.0-beta] - 2026-04-04

The UX & Stability Update — ticket UI surfaces and transitions.

## [0.1.0-beta] - 2026-03-21

Early Beta & MVP Release.

[1.0.0]: https://github.com/Na3ikus/ServiceRequestApp/compare/v0.3.0-beta...v1.0.0
[0.3.0-beta]: https://github.com/Na3ikus/ServiceRequestApp/compare/v0.2.0-beta...v0.3.0-beta
[0.2.0-beta]: https://github.com/Na3ikus/ServiceRequestApp/compare/v0.1.0-beta...v0.2.0-beta
[0.1.0-beta]: https://github.com/Na3ikus/ServiceRequestApp/releases/tag/v0.1.0-beta
