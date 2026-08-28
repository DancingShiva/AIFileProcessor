---
title: Telemetry
doc_id: TEL
status: draft
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 07 — Telemetry

## Purpose

Defines what operational data the pipeline collects about provider calls and chunk processing,
how it flows out on success and failure paths, and how it is kept consistent with provider-side
gaps in usage metadata.

## Scope

- **In scope:** `CallTelemetry`, `ChunkMetrics`, `ProcessingMetrics`, `TelemetryUtils`, logging.
- **Out of scope:** web request logging/middleware, deployment observability (monitoring stacks).

## Requirements

- **TEL-001** Each provider API call **must** be instrumented with a `CallTelemetry` record carrying
  `StartUtc`, `EndUtc`, `ElapsedMs`, `Attempts` (1 + retries), and `Status` ("Success"/"Failed").
- **TEL-002** `CallTelemetry` fields that a provider does not surface **must** be nullable and set
  to `null` when absent: `TimeToFirstByteMs` (non-streaming), `PromptTokens`, `CompletionTokens`.
  Missing metadata must not be fabricated.
- **TEL-003** Each chunk **must** be instrumented with a `ChunkMetrics` record carrying
  `ChunkIndex`, `LineCount`, `ElapsedMs`, `Attempts` (chunk + service retries), and `Status`.
- **TEL-004** Run-level metrics **must** be aggregated into `ProcessingMetrics` — total chunks,
  succeeded/failed counts, `ElapsedMs` (monotonic wall clock), and a `RunStartUtc` timestamp —
  and attached to the merged `AiApiResponse` on success.
- **TEL-005** On the failure path (PROC-004/PROC-009), the thrown `ChunkProcessingException`
  **must** carry the `ProcessingMetrics` collected for completed chunks so a failed run is still
  observable.
- **TEL-006** Telemetry values derived from wall clock **must** use UTC timestamps and a monotonic
  elapsed measurement (`Stopwatch`-based), not `DateTime` subtraction.

## Invariants

- Telemetry is additive-only — it never alters processing behavior.
- A run always yields some telemetry, success or failure.

## Related decisions

- None yet.

## Open questions

- Aggregation/reporting destination (UI surface, log sink, or metrics backend) is undefined.
- Whether per-token usage should drive rate/quota accounting for the Web target (ties to QA/WEB).

## References

- `AIFileProcessor.Core/Telemetry/CallTelemetry.cs`, `ChunkMetrics.cs`, `ProcessingMetrics.cs`,
  `TelemetryUtils.cs`
- `AIFileProcessor.Core/Services/AiServiceBase.cs` (telemetry helper methods)
- [04-processing-pipeline.md](04-processing-pipeline.md) (PROC-009)
