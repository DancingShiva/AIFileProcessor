---
title: Chunking by lines, not characters
doc_id: ADR-0001
status: accepted
last-reviewed: 2026-08-28
owner: DancingShiva
---

# ADR-0001: Chunking by lines, not characters

## Context

Input data is blocks of Chinese text, one sentence per line. The providers impose a ~32K character
input limit. A chunking strategy must (a) respect input limits, (b) never split a sentence, and
(c) keep output rows aligned 1:1 with input lines.

## Decision

Chunk **by lines**, never by characters. `ChunkSize` counts lines (default 40). The provider
character limit does not bound chunk count.

## Alternatives considered

- **Chunk by characters** — rejected: can split a sentence across chunk boundaries, breaking the
  `Sentences` array's strict 1:1 correlation with input lines (PRM-006) and making merge
  order-dependent.
- **Chunk by estimated tokens** — rejected: token counts are provider-dependent and unavailable
  pre-call; adds nondeterminism to an otherwise deterministic split.

## Consequences

- A chunk can exceed 32K characters if a single region of the input is dense; the provider call is
  responsible for surfacing that as a failure (fail-fast, PROC-004) rather than silent truncation.
- Chunk count scales with line count, not input size, keeping ordering deterministic and testable.

Spec: [04-processing-pipeline.md](../04-processing-pipeline.md) PROC-002, PROC-006.
