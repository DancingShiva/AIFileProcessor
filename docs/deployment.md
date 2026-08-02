# Deployment

AIFileProcessor supports three deployment modes. All modes source API keys from server/machine-level
configuration — never from clients or per-user files.

## Prerequisites (all modes)

- **.NET 10** runtime (desktop apps need the *Windows Desktop* runtime for WinForms).
- API keys for the provider(s) in use, supplied via environment variables:

  ```powershell
  $env:GeminiSettings__ApiKey   = "..."
  $env:OpenAiSettings__ApiKey   = "..."
  $env:DeepSeekSettings__ApiKey = "..."
  ```

## 1. Desktop (single user) — WinForms

Build/publish the WinForms front end:

```powershell
dotnet publish AIFileProcessor.WinForms -c Release -r win-x64 --self-contained
# or framework-dependent (smaller; requires the runtime installed):
dotnet publish AIFileProcessor.WinForms -c Release
```

Run `AIFileProcessor.exe`. On first use open **Settings** to choose models/reasoning/MaxTokens;
these are saved to `%APPDATA%\AIFileProcessor\{user}\settings.json`. API keys come from env vars
(or User Secrets during development).

## 2. Terminal Server / multi-session — WinForms

The same WinForms app runs per RDP/session. Because `WindowsUserContext` resolves the Windows
username and settings load from `%APPDATA%\AIFileProcessor\{userId}\settings.json`, **each session
gets isolated settings automatically**. No extra configuration is required — install per machine,
set the machine-level environment variables for API keys, and each user's preferences stay separate.

## 3. Web API — ASP.NET Core

### Configure Entra ID (required)

1. Register an application in Microsoft Entra ID; note the **Tenant ID** and **Client ID**, and create
   a scope/app role for the API audience.
2. Fill in `AzureAd` in `appsettings.json` (or supply via env vars `AzureAd__TenantId` etc.):

   ```json
   "AzureAd": {
	 "Instance": "https://login.microsoftonline.com/",
	 "TenantId": "<tenant-id>",
	 "ClientId": "<client-id>",
	 "Audience": "<api://client-id or client-id>"
   }
   ```

> **Until configured, every request returns HTTP 401.** Auth is enforced by a fallback authorization
> policy (`RequireAuthenticatedUser`) so no endpoint is accidentally public.

### Provide API keys

Set the three `*Settings__ApiKey` environment variables in the hosting environment (App Service
configuration, container env, IIS environment block, etc.). Do not place real keys in the deployed
`appsettings.json`.

### Publish & host

```powershell
dotnet publish AIFileProcessor.Web -c Release -o publish
```

Host the `publish` output on any ASP.NET Core host: **Azure App Service**, **IIS** (ASP.NET Core
Module), **Docker**, or **Kestrel`/`dotnet` directly. HTTPS is required in practice
(`app.UseHttpsRedirection()`). Then point clients at the endpoints in
[docs/web-api.md](web-api.md).

### Example (IIS / self-host quick check)

```powershell
cd publish
$env:ASPNETCORE_URLS = "https://localhost:5001"
dotnet AIFileProcessor.Web.dll
# then: GET https://localhost:5001/api/settings with a valid Bearer token
```

## Mode comparison

| Aspect | Desktop | Terminal Server | Web API |
|---|---|---|---|
| Project | WinForms | WinForms | AIFileProcessor.Web |
| Settings store | `FileSettingsProvider` | `FileSettingsProvider` (per-user file) | `WebSettingsProvider` (per-request) |
| User identity | `WindowsUserContext` | `WindowsUserContext` (per session) | `HttpUserContext` (Entra claims) |
| Auth | none | none | Entra ID JWT (required) |
| API keys | env / user-secrets | env (machine) | env (host) |

## Configuration checklist (per mode)

- [ ] API keys set via environment variables.
- [ ] `appsettings.json` reviewed (non-secret defaults only).
- [ ] (Web) `AzureAd` TenantId/ClientId/Audience configured and a test token acquired.
- [ ] (Web) HTTPS endpoint reachable; `/api/settings` returns `401` without a token and `200` with one.
- [ ] (Desktop/TS) `%APPDATA%\AIFileProcessor` writable per user.
