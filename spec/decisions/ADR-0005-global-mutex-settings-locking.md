---
title: Global\ mutex for FileSettingsProvider locking
doc_id: ADR-0005
status: accepted
last-reviewed: 2026-08-28
owner: DancingShiva
---

# ADR-0005: Global\ mutex for FileSettingsProvider locking

## Context

On a Terminal Server, the same user may hold multiple RDP sessions (separate processes) that share
one `%APPDATA%\AIFileProcessor\{userId}\settings.json`. Concurrent read-modify-write operations
across processes can corrupt the file or silently lose a change.

## Decision

`FileSettingsProvider` uses a **cross-process `Mutex` with the `Global\` prefix**, named per
sanitized user ID, to serialize all read-modify-write operations on the shared settings file.

## Alternatives considered

- **In-process lock (`lock`/`SemaphoreSlim`)** — rejected: does nothing across the multiple
  processes involved in Terminal Server sessions.
- **Windows file locking on the settings file** — rejected: a crash can leave an orphaned lock
  handle; named mutexes are the conventional cross-process primitive and survive process exit.
- **Per-session file paths** — rejected: breaks per-user preference continuity across that user's
  sessions; the shared file is the intended key partition.

## Consequences

- A settings write in one session is visible and safe from concurrent writes in another session
  for the same user.
- Mutex name is derived per sanitized user ID; sanitization rules must be pinned (see SET-open
  question).
- Desktop single-user is unaffected (one process, one session).

Spec: [06-settings-and-identity.md](../06-settings-and-identity.md) SET-005;
[10-quality-attributes.md](../10-quality-attributes.md) QA-001 (Terminal Server).
