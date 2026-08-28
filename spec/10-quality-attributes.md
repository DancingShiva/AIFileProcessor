---
title: Quality Attributes
doc_id: QA
status: draft
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 10 — Quality Attributes

## Purpose

Defines the cross-cutting, non-functional qualities of the system: the concurrency model per
deployment target, shared limits/configurability, and the error taxonomy callers can rely on.

## Scope

- **In scope:** concurrency per target, `ProcessingLimits` behavior and defaults, cancellation,
  error/exception model, hot-reloadability.
- **Out of scope:** performance budgets/benchmarks (none yet defined), packaging details.

## Requirements

### Deployment targets & concurrency

- **QA-001** The solution **must** support three deployment targets with the documented concurrency
  models:
  1. **Desktop (single-user):** one process, one user — no concurrency concerns.
  2. **Terminal Server:** each RDP session is a separate process; the same user may hold multiple
	 sessions sharing one `%APPDATA%` settings file → cross-process file locking required (SET-005).
  3. **Web (planned):** a single process serves all users → any process-wide/shared static state
	 becomes a shared-state concern that must be revisited before productionization.
- **QA-002** Per-user preference isolation **must** hold on Terminal Server with no extra
  configuration beyond machine-level env keys (SET-008).
- **QA-003** For the Web target, any process-wide shared state **must** be identified and made
  concurrency-safe before serving concurrent users. The older `ProcessingLimits`-as-static
  assumption is now superseded by the DI-scoped, hot-reloadable implementation (QA-004), but any
  reintroduction of process-wide mutable state here remains a Web-port risk.

### Limits & configuration

- **QA-004** `ProcessingLimits` **must** be hot-reloadable: bound from the `Processing` section of
  appsettings.json via `IOptionsMonitor<ProcessingSettings>`, with values read on every access so
  changes take effect without restart.
- **QA-005** Processing limits **must** default when unconfigured/non-positive: `MaxInputSize` =
  32768 chars, `MaxInputLines` = 400, `MaxOutputFieldSize` = 32768 chars, `MaxJsonDepth` = 16,
  `RetryJitterSeconds` = 5.
- **QA-006** Chunking limits (`ChunkSize`, `MaxConcurrentChunks`) are enforced by the pipeline
  (PROC-002/003); the provider 32K character input limit does **not** cap chunk count.

### Cancellation & errors

- **QA-007** Cancellation **must** propagate via `CancellationToken` from the front-end through
  the orchestrator into the text processor, and client disconnect in Web **must** surface as 499.
- **QA-008** Domain failures **must** use the defined exception taxonomy so callers can react
  precisely: `ApiKeyNotConfiguredException`, `ChunkProcessingException` (metrics attached),
  `PromptInjectionDetectedException`, `InjectionAftermathException`, plus
  `InvalidOperationException` for missing provider API key (→ HTTP 400 in Web).

## Invariants

- A limit change never requires an application restart.
- No partial result is ever presented as complete under cancellation or chunk failure.

## Related decisions

- [ADR-0004](decisions/ADR-0004-two-retry-layers.md)
- [ADR-0005](decisions/ADR-0005-global-mutex-settings-locking.md)

## Open questions

- Reconcile the `ProcessingLimits` concurrency story across targets — current DI-scoped +
  hot-reloadable design already removes the static shared-state concern for Web; confirm no other
  statics remain before the port.
- Performance budgets (latency/throughput targets per target) are undefined.

## References

- `AIFileProcessor.Core/Services/ProcessingLimits.cs`, `AIFileProcessor.Core/Exceptions/*.cs`
- [docs/deployment.md](../docs/deployment.md)
- [04-processing-pipeline.md](04-processing-pipeline.md), [06-settings-and-identity.md](06-settings-and-identity.md), [09-frontend-web.md](09-frontend-web.md)
