---
title: Processing Pipeline
doc_id: PROC
status: living
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 04 — Processing Pipeline

## Purpose

Defines the end-to-end flow that turns (prompt + data + per-user preferences) into a
`ProcessingResult` — orchestration, chunking, concurrency, retries, parse/merge, and output
assembly.

## Scope

- **In scope:** `ProcessingOrchestrator` responsibilities, `ChineseTextProcessor` chunk/fan-out/
  merge behavior, the two retry layers, ID/length assignment, response formatting.
- **Out of scope:** provider SDK behavior ([02](02-llm-providers.md)), the response JSON schema
  ([03](03-prompt-contract.md)), per-user settings resolution internals
  ([06](06-settings-and-identity.md)).

## Requirements

### Orchestration

- **PROC-001** `ProcessingOrchestrator` **must** be the single entry point for a processing request
  and **must** remain front-end-agnostic. It resolves the three providers' `*ModelSettings` from
  `ISettingsProvider` for the current `IUserContext.UserId`, applies the selected model's reasoning
  level and clamped `MaxTokens`, bundles chunk/concurrency settings into `ChunkingOptions`, builds
  the `IDictionary<string, IAiServiceSettings>` provider-settings map, calls the text processor,
  assigns IDs and cleaned length counts, and assembles the final output.

### Chunking & concurrency

- **PROC-002** Data **must** be split into chunks **by lines, not characters**. `ChunkSize` default
  is **40 lines**. The provider's 32K character input limit does **not** bound chunk count.
- **PROC-003** Chunks **must** be processed with a bounded degree of parallelism
  (`MaxConcurrentChunks`, default 3, via semaphore) and order **must** be preserved by chunk index
  regardless of completion order.
- **PROC-004** The processor **must** fail fast: if any chunk fails after its retries are exhausted,
  the entire run **must** abort (data-loss avoidance) rather than returning a partial result.

### Two retry layers

- **PROC-005** There **must** be exactly two retry layers:
  1. **Service-level** — per HTTP call, inside `IAiService.ProcessTextAsync`: `MaxRetries = 3`
	 with exponential backoff (1s / 2s / 4s), transient detection via `IsProviderTransient`.
  2. **Chunk-level** — per chunk, in `ChineseTextProcessor`: `ChunkMaxRetries = 3` with backoff
	 of 5s / 10s / 20s.
  A chunk therefore gets up to service-level retries × chunk-level retries before failing.

### Parse, merge, assemble

- **PROC-006** Each chunk's JSON **must** be parsed into `AiApiResponse`, verified (PRM-011), sandhi
  applied (PRM-012), and merged: scratchpad / corrections / rows from all chunks combine, in index
  order.
- **PROC-007** After merge, each row **must** be assigned an ID via `InitializeId`
  (prefix + zero-padded sequence; `idPrefix`, `idStart`, `idPad`) and a cleaned length via
  `GetCleanedCharacterCount` (padding `lengthPad`).
- **PROC-008** The assembled output **must** include a tab-delimited `RawResponse`
  (Id, Chinese, Pinyin, breakdown, audio reordering, English, length) alongside the structured
  `ProcessedSentenceRow` collection, corrections, and scratchpad.
- **PROC-009** Run-level and per-chunk metrics **must** be collected during the run (see
  [07-telemetry.md](07-telemetry.md)); on the failure path the `ChunkProcessingException` **must**
  carry the metrics collected so far.

## Invariants

- A partial result is never silently returned as if complete.
- Row order in the output equals input line order.
- No chunk processing happens outside `ChineseTextProcessor`.

## Related decisions

- [ADR-0001](decisions/ADR-0001-chunking-by-lines.md)
- [ADR-0004](decisions/ADR-0004-two-retry-layers.md)

## Open questions

- Backpressure/queueing semantics for the Web target (single shared process) when many concurrent
  requests contend for chunks — see QA-003.
- Whether fail-fast should eventually support partial-result delivery behind an explicit flag.

## References

- `AIFileProcessor.Core/Processing/ProcessingOrchestrator.cs`
- `AIFileProcessor.Core/Services/ChineseTextProcessor.cs`, `TransientRetryPolicy.cs`
- `AIFileProcessor.Core/Models/ChunkingOptions.cs`, `Processing/ProcessingResult.cs`
