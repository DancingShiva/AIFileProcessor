---
title: Two retry layers
doc_id: ADR-0004
status: accepted
last-reviewed: 2026-08-28
owner: DancingShiva
---

# ADR-0004: Two retry layers

## Context

Provider calls fail for transient reasons (rate limits, 5xx). Chunks also fail from larger-scope
issues (timeouts, partial responses). A single retry layer either retries at the wrong scope
(per-call can't absorb a chunk-level stall; per-chunk re-performs per-call backoff and wastes
budget) or conflates distinct failure modes.

## Decision

Two retry layers with different scopes and backoffs:

1. **Service-level** — per HTTP call, inside `IAiService.ProcessTextAsync`: `MaxRetries = 3`
   with 1s/2s/4s exponential backoff, transient detection via `IsProviderTransient`.
2. **Chunk-level** — per chunk, in `ChineseTextProcessor`: `ChunkMaxRetries = 3` with
   5s/10s/20s backoff.

A chunk therefore receives up to service-level × chunk-level attempts before the run fails fast.

## Alternatives considered

- **Single retry layer at the service level** — rejected: cannot distinguish a per-call transient
  blip from a chunk that needs a longer, coarser-grained wait; conflates scopes.
- **Single retry layer at the chunk level** — rejected: re-sends the whole chunk even for a cheap
  per-call transient, multiplying load against an already-stressed provider.

## Consequences

- Service-level handles cheap per-call transients quickly; chunk-level handles coarser failures
  with longer waits.
- Total worst-case latency per chunk is bounded and predictable (sum of both layers' schedules).
- `TransientRetryPolicy` is the shared static helper both layers use with their own
  `isTransient` predicate (ARCH-004).

Spec: [04-processing-pipeline.md](../04-processing-pipeline.md) PROC-005.
