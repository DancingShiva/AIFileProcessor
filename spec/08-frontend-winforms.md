---
title: WinForms Front-End
doc_id: WF
status: draft
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 08 — WinForms Front-End

## Purpose

Defines the desktop (single-user and Terminal Server) shell behavior — forms, threading,
persistence entry points, and composition — so the desktop app remains a thin shell over Core.

## Scope

- **In scope:** `MainForm`/`SettingsForm` responsibilities, DI host setup, `WindowsUserContext`,
  `IDialogService`/logging, per-user settings save flow.
- **Out of scope:** pipeline logic ([04](04-processing-pipeline.md)), provider contracts
  ([02](02-llm-providers.md)), web surface ([09](09-frontend-web.md)).

## Requirements

- **WF-001** The WinForms app **must** contain no durable logic; it is a thin shell per ARCH-001.
  Long-running processing and all decisions about settings, chunking, parsing, and formatting
  occur in `AIFileProcessor.Core`.
- **WF-002** `Program.cs` **must** build a generic-host composition root: `appsettings.json`
  (+ User Secrets in Development, + environment variables), `AddAiFileProcessorCore`, plus
  `WindowsUserContext`, the forms, and `StartupAppContext` registered.
- **WF-003** `SettingsForm` **must** save per-user settings via `ISettingsProvider.Save*SettingsAsync`,
  which writes only non-secret preferences; the API key **must** be stripped automatically
  (SEC-007) and never shown/copyable in the UI.
- **WF-004** UI interactions **must** not block the WinForms message pump; processing calls are
  awaited asynchronously with cancellation support wired to the UI.
- **WF-005** Dialogs **must** go through `IDialogService` (implementation
  `MessageBoxDialogService`) so tests and future shells can substitute it.
- **WF-006** Logging **must** be routed through an `ILogger` provider (`FileLoggerProvider`)
  usable in dev diagnostics; per-run telemetry surfaced per [07-telemetry.md](07-telemetry.md).
- **WF-007** `WindowsUserContext` **must** resolve the Windows identity name as the user ID, giving
  automatic per-session settings isolation on Terminal Server (SET-008).

## Invariants

- Saving settings never writes an API key to a per-user file.
- A processing run can always be cancelled from the UI.

## Related decisions

- [ADR-0002](decisions/ADR-0002-durable-logic-in-core.md)
- [ADR-0005](decisions/ADR-0005-global-mutex-settings-locking.md)

## Open questions

- Exact async/UI-threading patterns (e.g. `async void` handlers, `SynchronizationContext`
  marshaling) are not yet specified — to be tightened when the WinForms UX is fully fleshed out.
- Progress/cancellation UX for long multi-chunk runs during the desktop polish phase.

## References

- `AIFileProcessor.WinForms/Program.cs`, `MainForm.cs`, `SettingsForm.cs`
- `AIFileProcessor.WinForms/Configuration/WindowsUserContext.cs`, `StartupAppContext.cs`
- `AIFileProcessor.WinForms/Interfaces/IDialogService.cs`,
  `Logging/FileLoggerProvider.cs`
