# Settings Provider Architecture for Multi-Mode Deployment

## The Core Problem

All three AI services capture `IOptions<TSettings>` in their constructors as a **read-only snapshot** at DI resolution time:

```csharp
// GeminiService — delegates to AiServiceBase<TSettings>
public GeminiService(ILogger<GeminiService> logger, IOptions<GeminiSettings> settings, HttpClient httpClient)
	: base(logger, settings?.Value ?? throw ..., httpClient)
{
}
```

The `AiServiceBase<TSettings>` base class stores the snapshot in `DefaultSettings`.

This means:

- The AI client (`Google.GenAI.Client`, `ChatClient`) is lazily created once and cached with the original API key
- Changing settings via `SettingsForm` → `SaveSettings()` writes to `appsettings.json`, but the in-memory `_settings` and `_client` are **never updated**
- On a Terminal Server, all users share the same process, same `IConfiguration`, same `IOptions<T>` singleton — there's no per-user isolation at all

## The Solution: Settings Provider Abstraction

The key insight is that **settings should not be a static DI singleton** — they should be **resolved per-call, per-user**. Here's how to support all three deployment modes with one architecture:

### Architecture Overview

```
					ISettingsProvider (new interface)
				   ┌─────────────┴──────────────┐
				   │                            │
		 GetSettingsAsync(userId)          GetSettingsAsync(userId)
				   │                            │
		┌──────────┴──────────┐    ┌───────────┴───────────┐
		│  FileSettingsProvider │    │ WebSettingsProvider   │
		│  (WinForms/Terminal)  │    │ (Web/SPA)             │
		└──────────┬──────────┘    └───────────┴───────────┘
				   │                            │
	┌──────────────┴───────────┐    ┌────────────┴────────────┐
	│ appsettings.json          │    │ Per-request: API key     │
	│ + per-user override files  │    │ + client-side preferences│
	│ in %APPDATA%/AIFileProcessor │   │ (localStorage → request)  │
	└──────────────────────────┘    └─────────────────────────┘
```

### API Key Model

API keys are **always** sourced from machine-level `IConfiguration` (environment variables or `appsettings.json`). They are never stored in per-user files, never sent by web clients, and never encrypted. This simplifies the architecture:

- **No DPAPI encryption code** — per-user files contain only non-secret preferences
- **No API key UI in SettingsForm** — users cannot set keys; they are machine-configured
- **No API key headers in web requests** — clients never handle keys; the server reads them from env vars
- **Terminal Server** — all users on a machine share the same API keys via environment variables; each user has their own preferences file

### How It Works for Each Deployment Mode

**1. Dedicated WinForms (single user)**

- `userId` defaults to a fixed string (e.g., `"default"` or the Windows identity name)
- `FileSettingsProvider` reads from `appsettings.json` as it does today, but also checks for a per-user override file in `%APPDATA%\AIFileProcessor\{userId}\settings.json`
- The `SettingsForm` saves non-secret preferences to the per-user file; API keys come from environment variables
- Per-user files are plain JSON — no encryption, no secrets

**2. Windows Terminal Server (multiple users, shared machine)**

- `userId` = the Windows identity (`Environment.UserName` or `Thread.CurrentPrincipal.Identity.Name`)
- Each user gets their own `%APPDATA%\AIFileProcessor\{userId}\settings.json` (preferences only)
- **Cross-process file locking** — the same user may open multiple RDP sessions; `FileSettingsProvider`
  uses a `Global\` mutex (named per sanitized user ID) to serialize all read-modify-write operations,
  preventing concurrent writes from corrupting the settings file or silently losing changes
- API keys are shared from machine-level environment variables — all users use the same keys

**3. Web application (many users)**

- `userId` = authenticated user ID (from JWT, Entra ID, etc.)
- `WebSettingsProvider` resolves non-secret preferences from the HTTP request body; API keys come from server-side environment variables
- Server is stateless — no per-user files on the server; preferences are client-side in `localStorage`
- Clients never send API keys to the server

### What Needs to Change in the Core

The critical change is that the AI services must stop capturing settings at construction time and instead **receive settings per call**. Here's the pattern:

```csharp
// NEW interface — resolves settings per-call, per-user
public interface ISettingsProvider
{
	Task<GeminiSettings> GetGeminiSettingsAsync(string userId, CancellationToken ct);
	Task<OpenAiSettings> GetOpenAiSettingsAsync(string userId, CancellationToken ct);
	Task<DeepSeekSettings> GetDeepSeekSettingsAsync(string userId, CancellationToken ct);
}

// NEW: ProcessingOrchestrator passes settings through to the AI service per-call
public interface IAiService
{
	// Existing members unchanged...
	Task<string> ProcessTextAsync(
		string modelId, string prompt, string data,
		CancellationToken ct = default);

	// NEW: overload that accepts per-call settings
	Task<string> ProcessTextAsync(
		string modelId, string prompt, string data,
		object settings,            // provider-specific (GeminiSettings, OpenAiSettings, DeepSeekSettings)
		CancellationToken ct = default);
}
```

Each AI service gets a per-call `settings` object instead of relying on the constructor-injected snapshot. The internal client (`Google.GenAI.Client`, `ChatClient`) is created fresh per call or cached keyed by a settings hash so that changing the API key or model doesn't reuse a stale client.

**Why this works:**

- WinForms: `FileSettingsProvider` resolves the current user's settings from their file on disk
- Terminal Server: same provider, different `userId` per Windows session
- Web: `WebSettingsProvider` resolves from the request context
- The AI services, `ProcessingOrchestrator`, and the rest of `AIFileProcessor.Core` don't know or care which provider is active — they just call `ISettingsProvider` and get settings

### What Stays Unchanged

| Component | Change? |
|---|---|
| `GeminiSettings`, `OpenAiSettings`, `DeepSeekSettings` | **No change** — same POCOs |
| `ProcessingOrchestrator` | **Minimal** — gets settings from provider, passes to AI service |
| `IAiService` implementations | **Internal change** — accept per-call settings, but public `ProcessTextAsync` signature can stay for backward compat |
| `SettingsForm` (WinForms) | **Updated** — saves to per-user file via provider instead of `appsettings.json` |
| `ServiceCollectionExtensions` | **Updated** — registers `ISettingsProvider` (different impl per frontend), keeps `IOptions<T>` for defaults |
| `appsettings.json` | **Stays** — becomes the default/fallback, overridden by per-user settings |

### Recommended Settings Storage Per Mode

| Setting | WinForms (single) | Terminal Server | Web |
|---|---|---|---|
| **API keys** | Machine-level environment variables | Machine-level environment variables | Server-side environment variables |
| **Non-secret prefs** | Per-user JSON file in `%APPDATA%` (plain, no encryption) | Same | Client-side `localStorage` |
| **Defaults** | `appsettings.json` | `appsettings.json` | `appsettings.json` (server-side defaults) |

### Why Not Just Use `IOptionsMonitor<T>`?

`IOptionsMonitor<T>` supports `reloadOnChange: true` (which you already have), but it only gives you **one global current value** — there's no per-user dimension. It would solve the "stale settings after save" bug on a single-user machine, but it can't help on Terminal Server or web where multiple users need different settings simultaneously in the same process. You need the `ISettingsProvider` abstraction.

### Summary

The single change that unlocks all three deployment modes is **replacing the static `IOptions<T>` singleton with a per-call, per-user `ISettingsProvider`**. Everything else flows from that:

- `FileSettingsProvider` covers WinForms + Terminal Server (per-user preference files in `%APPDATA%`, API keys from env vars, cross-process `Mutex` locking for concurrent session safety)
- `WebSettingsProvider` covers web (per-request preferences from client, API keys from server env vars)
- API keys are always machine-level — never in per-user files, never from web clients
- Non-secret preferences are per-user files (desktop) or `localStorage` (web), stored as plain JSON without encryption

### Cross-Process File Locking

`FileSettingsProvider` serializes all file read-modify-write operations using a named `Mutex` with
the `Global\` prefix (e.g., `Global\AIFileProcessor_{sanitizedUserId}_Settings`). This is required
because on Terminal Server, the same user can open multiple RDP sessions — two processes writing
the same `%APPDATA%` settings file concurrently could corrupt the file or silently lose changes.

The implementation uses two methods:
- `ReadUserOverrides` — acquires the mutex, reads the file, releases the mutex. Used by all
  getter methods (e.g., `GetGeminiSettingsAsync`) that are not already inside a locked context.
- `ReadUserOverridesInternal` — reads without acquiring the mutex. Used by `SaveSection`,
  `SaveModelSection`, `MigrateLegacyProviderToModelSettings`, and migration `seed` callbacks
  that are already inside a `WithUserFileLock` scope (avoids re-entrant deadlock).

`WithUserFileLock` wraps an `Action` or `Func<T>` with `Mutex.WaitOne()` / `Mutex.ReleaseMutex()`,
using a `using` block to ensure the mutex handle is released even on exceptions.
- `appsettings.json` remains as the shared defaults baseline