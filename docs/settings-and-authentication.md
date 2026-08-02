# Settings & Authentication

How configuration, per-user preferences, API keys, and (for the web app) caller authentication work.

## Configuration model

Settings are provided per provider via three settings types in `AIFileProcessor.Core/Configuration`:
`GeminiSettings`, `OpenAiSettings`, `DeepSeekSettings`. Each binds from a matching section of
`IConfiguration` (`GeminiSettings`, `OpenAiSettings`, `DeepSeekSettings`). Common fields include
`Endpoint`, `ModelName`, `TimeoutSeconds`, `MaxTokens?`, `Temperature?`, `ReasoningLevel?`, and retry
knobs `MaxRetries` / `BaseDelay` / `MaxDelay` (Gemini additionally supports streaming, top-p, project/
location, managed identity, etc.). Defaults live in each front-end's `appsettings.json`.

### Provider `MaxTokens` clamping

A user-supplied `MaxTokens` is clamped to a hard maximum before reaching the API:
`ProcessingOrchestrator` calls `TokenUtils.ClampRequestedMaxTokens(value, provider, modelId)`, which
uses `ModelTokenLimits.GetHardMax` (currently 262144 for all providers). This turns "you asked for too
many tokens" into a clean clamp instead of an API error.

## API-key security (invariant)

`ApiKey` is a **type-level secret** on every settings type:

- `[JsonIgnore]` — it is never serialized to JSON (per-user files) or to web responses.
- `init`-only setter — settable only during object initialization (by `IConfiguration` binding).
- It is sourced **only** from machine-level `IConfiguration` — never from per-user files or from a
  web request. `SettingsExtensions.CopyOnto` (used to overlay per-user preferences onto defaults)
  deliberately never copies `ApiKey`.

### Supplying API keys

Use one of these (all are read by `IConfiguration`):

1. **Environment variables** (recommended for all modes):

   ```powershell
   $env:GeminiSettings__ApiKey   = "..."
   $env:OpenAiSettings__ApiKey   = "..."
   $env:DeepSeekSettings__ApiKey = "..."
   ```

2. **User Secrets** (desktop development only — added in `WinForms/Program.cs` when
   `Environment.IsDevelopment()`):

   ```powershell
   cd AIFileProcessor.WinForms
   dotnet user-secrets set "GeminiSettings:ApiKey" "..."
   ```

3. `appsettings.json` — only for non-secret defaults; **do not commit real keys here** (the shipped
   files have empty `ApiKey` values).

> If a key is missing, the corresponding `IAiService` throws `InvalidOperationException
> ("<Provider> API key is not configured.")`, which the Web `ProcessingController` maps to HTTP 400.

## Per-user settings resolution

All settings flow through `ISettingsProvider`, resolved **per call, per user** (identified by
`IUserContext.UserId`), so the in-DI `IOptions<T>` singleton snapshot is never the source of truth.

### Desktop / Terminal Server — `FileSettingsProvider`

- Defaults from `IConfiguration` (`appsettings.json` + env vars).
- Non-secret overrides merged from `%APPDATA%\AIFileProcessor\{userId}\settings.json`.
- `WindowsUserContext` supplies the Windows username, so each Terminal Server session gets its own
  isolated override file.
- Saved via `SettingsForm` → `Save*SettingsAsync` (ApiKey stripped automatically).

### Web — `WebSettingsProvider`

- Defaults from `IConfiguration`.
- Non-secret preferences come from the **client** with each processing request: the
  `ProcessingController` stashes `GeminiPreferences`/`OpenAiPreferences`/`DeepSeekPreferences` from the
  request body into `HttpContext.Items`, and `WebSettingsProvider` overlays them onto the defaults via
  `CopyOnto` (which never copies the API key).
- `Save*SettingsAsync` are **no-ops** in web mode — persistence is the client's job
  (e.g., browser localStorage; see `SettingsController.SaveSettings`).

## Web authentication & user identity

The Web API is secured with **Entra ID (Azure AD) JWT bearer** tokens via `Microsoft.Identity.Web`:

- `Program.cs` registers `AddMicrosoftIdentityWebApi(AzureAd)` and an **authorization fallback
  policy** that requires an authenticated caller on **every** endpoint.
- `UseAuthentication()` runs **before** `UseAuthorization()`.

Configure the tenant/app in `appsettings.json` (or env vars):

```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "TenantId": "<your-tenant-id>",
  "ClientId": "<your-app-client-id>",
  "Audience": "<api://your-client-id or client-id>"
}
```

> **Until `AzureAd` is configured, all API requests return HTTP 401.** Fill in real Entra
> app-registration values at deployment time — API keys for AI providers remain server-side regardless.

### UserId resolution (`HttpUserContext`)

When the caller is authenticated, the user id is taken from the most stable available claim, in order:
`NameIdentifier` → `oid` → `sub`, finally falling back to the display name; unauthenticated requests
(`HttpContext` absent or not authenticated) resolve to `"anonymous"`. This id is used for any
per-user settings resolution in web mode.
