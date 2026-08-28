---
title: Architecture
doc_id: ARCH
status: living
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 01 — Architecture

## Purpose

Defines the layering of the solution and the responsibilities of each project, so that durable
logic has exactly one home and every front-end remains a thin shell.

## Scope

- **In scope:** project responsibilities, dependency direction, DI composition, static-helper rule,
  type-safe settings boundary.
- **Out of scope:** provider SDK specifics ([02](02-llm-providers.md)), deployment packaging
  ([10](10-quality-attributes.md)), endpoint surface ([09](09-frontend-web.md)).

## Requirements

- **ARCH-001** All durable logic **must** live in `AIFileProcessor.Core`. Front-ends
  (`AIFileProcessor.WinForms`, `AIFileProcessor.Web`) **must** be thin shells limited to UI/wiring,
  composition roots (`Program.cs`), and front-end-specific `IUserContext`/`ISettingsProvider`
  implementations.
- **ARCH-002** Dependency direction **must** be one-way: front-ends reference `Core`; `Core`
  **must not** reference any front-end project.
- **ARCH-003** Each front-end **must** compose `Core` via its own composition root calling
  `ServiceCollectionExtensions.AddAiFileProcessorCore(IConfiguration)`, registering typed
  `HttpClient`s per provider and the shared pipeline services.
- **ARCH-004** Pure, stateless logic **must** be kept out of DI as `internal static` helpers
  (`AiModelCatalog`, `AiServiceGuard`, `TransientRetryPolicy`, `TokenUtils`, `ModelTokenLimits`).
  Only dependencies that genuinely require injection (`ILogger`, `HttpClient`,
  `IConfiguration`/`IOptions`, provider settings resolution) may be injected.
- **ARCH-005** Provider settings crossing the `IAiService` boundary **must** be typed via the
  `IAiServiceSettings` marker interface; `AiServiceBase<TSettings>` performs the safe cast.
  The `object` type **must not** be used for the per-call settings parameter.
- **ARCH-006** `IAiService` implementations **must** be registered so first-match-wins dispatch
  works: `IEnumerable<IAiService>` resolved by `SupportsModel(modelId)`, the first supporting
  provider handles the call.
- **ARCH-007** The settings snapshot registered in DI (`IOptions<TSettings>`) **must not** be the
  source of truth for processing calls; per-call resolution goes through `ISettingsProvider`
  (see SET-002).
- **ARCH-008** Chunk/concurrency values **must** travel as a bundled `ChunkingOptions`
  (`ChunkSize` + `MaxConcurrentChunks`), never as loose `int` parameters.

## Invariants

- No duplicated pipeline logic exists outside `Core`.
- `Core` compiles and functions with no front-end reference present.
- The DI container holds nothing that is pure/staticable logic.

## Related decisions

- [ADR-0001](decisions/ADR-0001-chunking-by-lines.md)
- [ADR-0002](decisions/ADR-0002-durable-logic-in-core.md)

## Open questions

- Formalizing the seam contract between `ProcessingOrchestrator` and `ChineseTextProcessor`
  for the future Web port (see QA-003).

## References

- [docs/architecture.md](../docs/architecture.md)
- `AIFileProcessor.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- `AIFileProcessor.WinForms/Program.cs`, `AIFileProcessor.Web/Program.cs`
