---
title: Infrastructure settings follow the API key's trust boundary
doc_id: ADR-0006
status: accepted
last-reviewed: 2026-08-28
owner: DancingShiva
---

# ADR-0006: Infrastructure settings follow the API key's trust boundary

## Context

API keys are machine-level shared secrets (ADR-0003): on Terminal Server every user of a machine
shares the same key, and on Web the server holds the key. Before this decision, per-user settings
could override `Endpoint`, `ProxyUrl`, `RequestHeaders`, and `AdditionalParameters` — meaning any
user (Terminal Server) or any authenticated web client (Web) could redirect machine-key-authenticated
traffic to an arbitrary destination and receive the key as a `Bearer` credential, or route it through
a proxy they control. A security review flagged this as the solution's most severe finding, and the
owner confirmed the vulnerability: whoever does not own the key must not choose where the key travels.

## Decision

Infrastructure settings — `Endpoint`, `ProxyUrl`, `RequestHeaders`, `AdditionalParameters` — are
**machine-level only**, exactly like `ApiKey`. They are sourced solely from machine-level
`IConfiguration` (environment variables, appsettings.json, User Secrets in dev) and:

- `SettingsExtensions.CopyOnto` (all four provider overloads) **never copies them** from any
  per-user source (settings files, web request bodies).
- `FileSettingsProvider` **strips them before persisting** any per-user settings file
  (`StripInfraProperties` at the save chokepoints), and legacy-migration seeding does not
  propagate them.
- The Web API accepts **narrow preference DTOs only** (`GeminiPreferencesDto`, `OpenAiPreferencesDto`,
  `DeepSeekPreferencesDto`) that structurally exclude infrastructure fields, mapped server-side onto
  Core settings objects before use.
- `GET /api/settings` returns those same narrow DTOs — machine infrastructure (which may embed
  gateway credentials in `RequestHeaders`) is never echoed to clients.
- The WinForms `SettingsForm` no longer exposes endpoint/proxy fields.

## Alternatives considered

- **Per-user endpoints with per-user keys** — rejected: requires a per-user key model that ADR-0003
  explicitly ruled out (no billing or key-management use case exists in any current target).
- **Endpoint allowlist validation** — rejected for now: keeps the parsing/validation surface large
  and still concedes that users may influence where machine credentials are sent; a allowlist remains
  a future option if a legitimate multi-endpoint need appears.
- **Keep per-user overrides, document the risk** — rejected: on Terminal Server any local user is
  potentially adversarial to other users of the same machine; documentation is not a control.

## Consequences

- Users (desktop/terminal) no longer choose provider endpoints, proxies, or custom headers; those
  are admin-controlled via machine configuration.
- Custom gateways/proxies remain fully supported — configured by the machine administrator, per
  model or per provider, via appsettings.json or environment variables.
- Per-user files, web request bodies, and web responses are all structurally incapable of carrying
  infrastructure values; defense is layered (DTO narrowing, CopyOnto exclusion, persistence strip).
- Existing user settings files that contain stale `Endpoint` values become inert: the read path
  ignores them and the next save removes them.

Spec: [05-security.md](../05-security.md) SEC-010, SEC-011;
[06-settings-and-identity.md](../06-settings-and-identity.md) SET-009;
[09-frontend-web.md](../09-frontend-web.md) WEB-005.
Supersedes: nothing (ADR-0003 stands; this extends its boundary principle to infrastructure settings).