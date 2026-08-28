---
title: LLM Providers
doc_id: PROV
status: living
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 02 — LLM Providers

## Purpose

Defines the contract every LLM provider backend must satisfy, how provider settings are typed
and resolved, and how clients are cached safely across users.

## Scope

- **In scope:** `IAiService` contract, `AiServiceBase<TSettings>` shape, provider settings types,
  model catalogs, token clamping, client caching.
- **Out of scope:** the retry *policy layers* ([04](04-processing-pipeline.md)), API-key sourcing
  ([05](05-security.md)), per-user preference merging ([06](06-settings-and-identity.md)).

## Requirements

- **PROV-001** The supported providers **must** be Gemini, OpenAI, and DeepSeek, each with its own
  `IAiService` implementation (`GeminiService`, `OpenAiService`, `DeepSeekService`).
  (Ollama artifacts exist; see Open questions.)
- **PROV-002** Every provider service **must** inherit `AiServiceBase<TSettings>`, which supplies
  the shared shell: constructor pattern, model-catalog lookups, the two `ProcessTextAsync`
  overloads, transient-exception detection, and request validation. A derived class implements
  **only** `ProcessTextCoreAsync` and `IsProviderTransient`.
- **PROV-003** `IAiService.ProcessTextAsync` **must** be declared with an `IAiServiceSettings`
  per-call settings parameter (not `object`); `AiServiceBase<TSettings>` casts to the concrete
  `TSettings`. Callers **must not** downcast outside the base class.
- **PROV-004** Each provider's model set **must** be exposed via `SupportedModels` and queried
  through `AiModelCatalog` (`SupportsModel`, `GetReasoningLevels`, `GetProviderKey`). Model IDs
  are provider-owned strings (e.g. `gemini-3.5-flash`).
- **PROV-005** A user-supplied `MaxTokens` **must** be clamped to the provider/model hard maximum
  (`TokenUtils.ClampRequestedMaxTokens` → `ModelTokenLimits.GetHardMax`; currently 262144 for all
  providers) before any API call is made. The API must never receive an out-of-range value.
- **PROV-006** Each provider **must** declare a settings type in `AIFileProcessor.Core/Configuration`
  bound from the matching `IConfiguration` section (`GeminiSettings`, `OpenAiSettings`,
  `DeepSeekSettings`), exposing at minimum `Endpoint`, `ModelName`, `TimeoutSeconds`, `MaxTokens?`,
  `Temperature?`, `ReasoningLevel?`, and retry knobs `MaxRetries` / `BaseDelay` / `MaxDelay`.
  Gemini may carry additional streaming/project/location/managed-identity fields.
- **PROV-007** SDK clients **must** be cached in a `ConcurrentDictionary` keyed by the settings
  fingerprint (`${ApiKey}:${Endpoint}:...`) so that two users with different credentials never
  share a cached client (no cross-user credential bleed) while repeated calls reuse clients.

## Invariants

- A provider that does not support a requested model must never receive the call.
- A request never reaches a provider with an unvalidated model ID, prompt, or data.
- Client cache keys uniquely identify (credentials + endpoint + model-relevant settings).

## Related decisions

- None yet — provider-set membership is a candidate for an ADR if a fourth provider is added.

## Open questions

- Is `OllamaService`/`OllamaSettings` intended to become a fully-supported provider (would require
  updates to PROV-001, README, OV glossary)? Currently registered but undocumented.
- Hard cap of 262144 tokens is uniform today; whether it should move per-model is undecided.

## References

- `AIFileProcessor.Core/Interfaces/IAiService.cs`, `IAiServiceSettings.cs`
- `AIFileProcessor.Core/Services/AiServiceBase.cs`, `AiModelCatalog.cs`, `AiServiceGuard.cs`,
  `TokenUtils.cs`, `ModelTokenLimits.cs`
- `AIFileProcessor.Core/Configuration/*Settings.cs`
