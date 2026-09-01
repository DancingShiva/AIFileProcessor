---
title: Settings & Identity
doc_id: SET
status: living
last-reviewed: 2026-08-29
owner: DancingShiva
---

# 06 — Settings & Identity

## Purpose

Defines how per-user, non-secret preferences are resolved per call and per user, how the current
user is identified per deployment target, and how shared per-user state is kept consistent.

## Scope

- **In scope:** `ISettingsProvider`, `IUserContext`, `FileSettingsProvider` + `WebSettingsProvider`,
  per-user file storage, cross-process locking, per-target identity resolution.
- **Out of scope:** the secrecy *model* of API keys ([05](05-security.md)), provider config section
  binding ([02](02-llm-providers.md)), web auth plumbing details ([09](09-frontend-web.md)).

## Requirements

- **SET-001** All settings **must** flow through `ISettingsProvider`, resolved **per call, per
  user** (identified by `IUserContext.UserId`). The in-DI `IOptions<T>` singleton snapshot
  **must not** be the source of truth for processing calls (see ARCH-007).
- **SET-002** `IUserContext` **must** supply the current user id, with implementations per target:
  `WindowsUserContext` (Windows identity name) for desktop/terminal, `HttpUserContext` (Entra/JWT
  claims) for web.
- **SET-003** `HttpUserContext` **must** resolve the user id from the most stable available claim,
  in order: `NameIdentifier` → `oid` → `sub`, finally falling back to the display name.
  Unauthenticated requests (no `HttpContext` or not authenticated) **must** resolve to
  `"anonymous"`.
- **SET-004** `FileSettingsProvider` (desktop/terminal) **must** merge defaults from
  `IConfiguration` with a non-secret per-user override JSON at
  `%APPDATA%\AIFileProcessor\{userId}\settings.json`.
- **SET-005** `FileSettingsProvider` **must** use **cross-process `Global\` mutex locking**, named
  per sanitized user ID, to serialize all read-modify-write operations on the shared `%APPDATA%`
  settings file — a user may hold multiple RDP sessions (separate processes) sharing one file.
- **SET-006** `WebSettingsProvider` (web) **must** take defaults from `IConfiguration` and overlay
  non-secret preferences supplied with the current HTTP request (stashed by
  `ProcessingController` into `HttpContext.Items`) via `CopyOnto`. `Save*SettingsAsync` are
  **no-ops** in web mode — persistence is the client's responsibility.
- **SET-007** Per-user perference state **must** contain only non-secret values. Secrets are defined
  and protected by [05-security.md](05-security.md) (SEC-005..007).
- **SET-008** On Terminal Server, per-user isolation of preferences **must** hold automatically
  because `WindowsUserContext` resolves the per-session Windows username into the file path; no
  additional configuration may be required.
- **SET-009** Per-user preference state **must** exclude infrastructure settings
  (`Endpoint`, `ProxyUrl`, `RequestHeaders`, `AdditionalParameters`): they carry the machine-level
  trust boundary of the API key (SEC-010, ADR-0006). `CopyOnto` never overlays them and
  `FileSettingsProvider` strips them at the save chokepoints, so a per-user file is structurally
  incapable of redirecting key-authenticated traffic.

## Invariants

- The user id is the sole key partitioning per-user state.
- A settings read-modify-write on a shared file is always mutex-serialized (`Global\`, per user).
- A missing or malformed per-user file never blocks defaults-only operation.

## Related decisions

- [ADR-0003](decisions/ADR-0003-no-api-keys-in-user-files.md)
- [ADR-0005](decisions/ADR-0005-global-mutex-settings-locking.md)
- [ADR-0006](decisions/ADR-0006-infra-settings-follow-key-trust-boundary.md)

## Open questions

- Sanitization rules for the user ID used in file paths and mutex names (the exact character set
  transformation) should be pinned down and referenced here.
- For the web port, whether any server-side per-user persistence (beyond client-side) becomes
  necessary.

## References

- [docs/SettingsProviderArchitecture.md](../docs/SettingsProviderArchitecture.md)
- [docs/settings-and-authentication.md](../docs/settings-and-authentication.md)
- `AIFileProcessor.Core/Interfaces/ISettingsProvider.cs`, `IUserContext.cs`
- `AIFileProcessor.Core/Services/FileSettingsProvider.cs`, `SettingsExtensions.cs`
- `AIFileProcessor.Web/Configuration/WebSettingsProvider.cs`, `HttpUserContext.cs`
- `AIFileProcessor.WinForms/Configuration/WindowsUserContext.cs`
