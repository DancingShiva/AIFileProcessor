---
title: Web Front-End
doc_id: WEB
status: draft
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 09 — Web Front-End

## Purpose

Defines the web-facing surface — endpoints, request/response contract, authentication, and
web-only concerns — for the ASP.NET Core host. **Status: prototype; not production-ready.**

## Scope

- **In scope:** endpoint surface and status codes, request/response models, Entra ID JWT auth,
  per-request settings overlay, web-only gaps (rate limiting, quotas).
- **Out of scope:** pipeline internals ([04](04-processing-pipeline.md)), provider contracts
  ([02](02-llm-providers.md)), deployment packaging ([10](10-quality-attributes.md)).

## Requirements

- **WEB-001** The Web API **must** require an authenticated Entra ID (Azure AD) JWT bearer token on
  **every** endpoint via an authorization fallback policy (`RequireAuthenticatedUser`). Until
  `AzureAd` is configured, every request must return HTTP 401.

### Endpoint surface

- **WEB-002** `POST /api/processing/process` **must** run Chinese text through the pipeline from a
  `ProcessRequest` body containing `modelId`, `prompt`, `data` (required) and optional `idPrefix`,
  `idStart`, `idPad`, `lengthPad`, `reasoningLevel`, and per-provider `*Preferences` objects.
- **WEB-003** `GET /api/settings` **must** return current default settings per provider with
  `ApiKey` excluded (`[JsonIgnore]` → SEC-005), as `ProviderSettingsResponse`.
- **WEB-004** `POST /api/settings` **must** be a no-op returning 200 with guidance that persistence
  is client-side (`localStorage`) and API keys remain server-side. Nothing is persisted server-side.

### Request handling

- **WEB-005** Per-provider `*Preferences` sent in the request **must** be stashed by
  `ProcessingController` into `HttpContext.Items` for `WebSettingsProvider` to overlay onto
  defaults (SET-006); `apiKey` within preferences **must** be ignored.
- **WEB-006** A non-null `MaxTokens` within the first provider preference (Gemini → OpenAI →
  DeepSeek order) **must** become the per-model `MaxTokens` for the call, then be clamped
  server-side (PROV-005). Validation errors return 400.
- **WEB-007** Error mapping **must** be: 400 missing `modelId`/`prompt`/`data`, unsupported model,
  or API key not configured; 401 no/invalid bearer token (any endpoint); 499 client-cancelled;
  500 any other unhandled failure with a generic message.

### Web-only concerns

- **WEB-008** Rate limiting, per-user quotas, and any per-user server-side persistence are
  **web-only concerns not yet implemented**. Their absence **must** be treated as a known gap
  before any public/multi-user exposure beyond prototype.

## Invariants

- No endpoint is reachable without a valid bearer token.
- No API key ever appears in a response body or is accepted from a client.
- Server remains stateless with respect to per-user preferences.

## Related decisions

- None yet (the Entra-ID-vs-other-identity decision is a candidate ADR once web is productionized).

## Open questions

- Rate-limiting/quota design (per-user, per-model, token-budget-based?) — WEB-008.
- CORS policy for the eventual browser client.
- Streaming responses for long runs vs. the current request/response model.

## References

- [docs/web-api.md](../docs/web-api.md)
- `AIFileProcessor.Web/Controllers/ProcessingController.cs`, `SettingsController.cs`
- `AIFileProcessor.Web/Models/*.cs`, `Program.cs`
- `AIFileProcessor.Web/Configuration/WebSettingsProvider.cs`, `HttpUserContext.cs`
