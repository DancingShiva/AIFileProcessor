---
title: API keys never stored in per-user files
doc_id: ADR-0003
status: accepted
last-reviewed: 2026-08-28
owner: DancingShiva
---

# ADR-0003: API keys never stored in per-user files

## Context

Per-user state must hold user preferences (model, reasoning level, MaxTokens, temperature) that
differ across users on the same machine. API keys are shared, sensitive, and must remain
machine-controlled. Storing them per-user both leaks them into user-readable/writable files and
forces encryption at rest.

## Decision

API keys are **type-level secrets**: `[JsonIgnore]` (never serialized) and `init`-only on every
settings type, sourced only from machine-level `IConfiguration` (environment variables; User
Secrets in desktop dev. `SettingsExtensions.CopyOnto` never copies `ApiKey`.

## Alternatives considered

- **Per-user API keys encrypted at rest (DPAPI)** — rejected: adds DPAPI/key-management code,
  per-user key UI, and web-request key handling, for a use case (per-user billing) that does not
  exist in any current target.
- **Server-held per-user keys for web** — rejected: server becomes a key store with associated
  breach surface; machine-level env keys are sufficient for all current targets.

## Consequences

- Per-user files contain only non-secret preferences — plain JSON, no encryption.
- No API-key UI in `SettingsForm`; users cannot set keys.
- Terminal Server: all users on a machine share the same env-var keys — intended.
- Web: clients never send or receive keys; server reads them from env vars.

Spec: [05-security.md](../05-security.md) SEC-005..009;
[06-settings-and-identity.md](../06-settings-and-identity.md).
