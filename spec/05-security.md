---
title: Security
doc_id: SEC
status: living
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 05 — Security

## Purpose

Defines the two security concerns this solution actively defends: prompt-injection (input and
output), and the handling of API keys as secrets across all deployment targets.

## Scope

- **In scope:** prompt-injection guardrails (send-side and verify-side), API-key sourcing/secrecy,
  per-target secret model.
- **Out of scope:** web authentication/authorization mechanics ([09](09-frontend-web.md)),
  per-user settings mechanics ([06](06-settings-and-identity.md)).

## Requirements

### Prompt-injection defense

- **SEC-001** Untrusted Chinese data **must** be wrapped in per-request delimiter tags with a
  cryptographically-random suffix before being sent to the provider (PRM-003). The suffix is
  generated per request and never reused.
- **SEC-002** Outbound requests **must** pass input validation via `AiServiceGuard`: non-null model
  ID, prompt, and data, plus a prompt-injection scan of the data (`PromptInjectionGuard`). A scan
  hit **must** surface as `PromptInjectionDetectedException` before any API call.
- **SEC-003** The response side **must** be defended by `ResponseVerifier` detecting *injection
  aftermath* — delimiter tags or injection markers that leaked into the model's output
  fields (`Chinese`, `English`, etc.) — and `SanitizeResponse` **must** strip data-delimiter tags
  from output fields rather than forwarding them.
- **SEC-004** Injection detection **must** use distinct exceptions (`PromptInjectionDetectedException`
  on input, `InjectionAftermathException` on output) so callers can distinguish pre-call rejection
  from post-call contamination.

### API-key secrecy

- **SEC-005** `ApiKey` **must** be a type-level secret on every provider settings type:
  annotated `[JsonIgnore]` (never serialized to JSON or web responses) and `init`-only (settable
  only during object initialization by `IConfiguration` binding).
- **SEC-006** API keys **must** be sourced **only** from machine-level `IConfiguration`
  (environment variables, or User Secrets in desktop development). They **must never** be stored
  in per-user files, never sent by web clients, and never encrypted at rest by this solution.
- **SEC-007** The per-user overlay (`SettingsExtensions.CopyOnto`) **must** never copy `ApiKey`
  when merging per-user preferences onto defaults. Saving per-user settings via `SettingsForm`
  **must** strip the API key automatically.
- **SEC-008** When an API key is missing, the corresponding `IAiService` **must** throw
  `InvalidOperationException` ("`<Provider>` API key is not configured.") which the Web layer
  maps to HTTP 400 — never a partial or unauthenticated attempt against the provider.
- **SEC-009** Client/SDK caches keyed by settings fingerprint **must** include the API key in the
  fingerprint (PROV-007), so no cached client can be reused across credentials.

## Invariants

- No secret ever crosses the machine-level boundary into per-user state, a web request, or a
  serialized response.
- Injection markers never flow from input data or model output into downstream callers unhandled.

## Related decisions

- [ADR-0003](decisions/ADR-0003-no-api-keys-in-user-files.md)
- [ADR-0005](decisions/ADR-0005-global-mutex-settings-locking.md)

## Open questions

- Terminal Server currently shares machine-level API keys across users (intended). Whether any
  deployment ever requires per-user keys is undecided (would change SEC-006..007).
- Rate limiting / per-user quotas for the public Web target are not yet designed (see WEB spec).

## References

- `AIFileProcessor.Core/Services/PromptInjectionGuard.cs`, `AiServiceGuard.cs`, `AiServiceBase.cs`
- `AIFileProcessor.Core/Processing/ResponseVerifier.cs`
- `AIFileProcessor.Core/Exceptions/PromptInjectionDetectedException.cs`, `InjectionAftermathException.cs`
- `AIFileProcessor.Core/Services/SettingsExtensions.cs`
