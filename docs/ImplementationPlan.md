# Implementation Plan: Per-User Settings Provider Architecture

## Goal

Replace the static `IOptions<T>` singleton settings model with a per-call, per-user `ISettingsProvider` abstraction that supports three deployment modes: dedicated WinForms, Windows Terminal Server, and web application.

---

## Step 1: Define Core Abstractions (`AIFileProcessor.Core`)

Create two new interfaces in `AIFileProcessor.Core/Interfaces/`:

### `ISettingsProvider.cs`
Resolves per-user settings on demand. Each AI service calls this per-request instead of capturing `IOptions<T>` at construction.

```
namespace AIFileProcessor.Interfaces

public interface ISettingsProvider
	Task<GeminiSettings> GetGeminiSettingsAsync(string userId, CancellationToken ct)
	Task<OpenAiSettings> GetOpenAiSettingsAsync(string userId, CancellationToken ct)
	Task<DeepSeekSettings> GetDeepSeekSettingsAsync(string userId, CancellationToken ct)
```

### `IUserContext.cs`
Abstracts how the current user ID is obtained, so the core doesn't depend on Windows identity or HTTP context.

```
namespace AIFileProcessor.Interfaces

public interface IUserContext
	string UserId { get }
```

**Files to create:**
- `AIFileProcessor.Core/Interfaces/ISettingsProvider.cs`
- `AIFileProcessor.Core/Interfaces/IUserContext.cs`

---

## Step 2: Implement `FileSettingsProvider` (`AIFileProcessor.Core`)

Create a file-based provider that reads defaults from `IConfiguration` (appsettings.json) and merges per-user overrides from `%APPDATA%\AIFileProcessor\{userId}\settings.json`.

### `FileSettingsProvider.cs`
Located in `AIFileProcessor.Core/Services/`.

**Behavior:**
- Constructor takes `IConfiguration` (for defaults) and computes the per-user directory: `Path.Combine(Environment.GetFolderPath(SpecialFolder.ApplicationData), "AIFileProcessor", userId, "settings.json")`
- `GetGeminiSettingsAsync`:
  1. Bind defaults from `IConfiguration.GetSection("GeminiSettings")` into a `GeminiSettings` instance
  2. If per-user file exists, deserialize it and overlay non-null/non-default values on top of defaults
  3. If per-user file has an `ApiKey` field (DPAPI-encrypted in a future step), decrypt and set it
  4. Return merged settings
- Same pattern for `GetOpenAiSettingsAsync` and `GetDeepSeekSettingsAsync`
- `SaveSettingsAsync(userId, settings)` — writes the per-user file, stripping `ApiKey` (as the existing SettingsManagers already do)

**Per-user file format** (`%APPDATA%\AIFileProcessor\{userId}\settings.json`):
```json
{
  "GeminiSettings": { ... non-secret overrides only ... },
  "OpenAiSettings": { ... },
  "DeepSeekSettings": { ... }
}
```

**Files to create:**
- `AIFileProcessor.Core/Services/FileSettingsProvider.cs`

---

## Step 3: Implement `WindowsUserContext` (`AIFileProcessor`)

Create a WinForms-specific `IUserContext` that resolves the current Windows user identity.

### `WindowsUserContext.cs`
Located in `AIFileProcessor/Configuration/`.

**Behavior:**
- Returns `Environment.UserName` (or `System.Security.Principal.WindowsIdentity.GetCurrent().Name` for a fully qualified domain\user)
- On a Terminal Server, each session gets a different value — natural per-user isolation
- For single-user desktop, returns the one user's name — works identically

**Files to create:**
- `AIFileProcessor/Configuration/WindowsUserContext.cs`

---

## Step 4: Refactor `IAiService` and AI Service Implementations

### `IAiService.cs` — Add per-call settings overload

Add a new overload to `ProcessTextAsync` that accepts settings object:

```
Task<string> ProcessTextAsync(
	string modelId,
	string prompt,
	string data,
	CancellationToken ct = default);

// NEW: per-call settings overload
Task<string> ProcessTextAsync(
	string modelId,
	string prompt,
	string data,
	object settings,
	CancellationToken ct = default);
```

The existing overload (without `settings`) remains for backward compatibility — it uses whatever settings were constructor-injected (useful for tests, simple scenarios).

### `GeminiService.cs` — Refactor to use per-call settings

**Changes:**
- Constructor: keep `IOptions<GeminiSettings>` for the default/legacy overload, but also inject `ISettingsProvider` and `IUserContext`
- New `ProcessTextAsync(..., object settings, ...)` overload:
  1. Cast `settings` to `GeminiSettings` (throw `ArgumentException` if wrong type)
  2. Use the per-call settings to configure `GenerateContentConfig` (temperature, topP, maxTokens, etc.)
  3. Create or reuse the `Google.GenAI.Client` keyed by a settings fingerprint (API key + endpoint + projectId + location) so that different users don't share a cached client with the wrong credentials
  4. Execute the same retry/streaming logic as today, but using per-call settings
- Extract the shared execution logic (retry loop, streaming, schema building) into a private method that both overloads call, parameterized by a `GeminiSettings` instance

**Client caching strategy:**
- Replace the single `_client` field with a `ConcurrentDictionary<string, Google.GenAI.Client>` keyed by a fingerprint string: `{apiKey}:{endpoint}:{projectId}:{location}`
- This allows multiple Terminal Server users with different keys to each get their own cached client
- The dictionary is bounded in practice (number of concurrent users × number of providers)

### `OpenAiService.cs` — Same refactoring pattern

**Changes:**
- Same constructor additions (`ISettingsProvider`, `IUserContext`)
- New overload accepting `object settings` → cast to `OpenAiSettings`
- Replace single `_client` / `_cachedModelId` with `ConcurrentDictionary<string, ChatClient>` keyed by `{apiKey}:{endpoint}:{modelId}`
- Extract shared execution logic into private method

### `DeepSeekService.cs` — Same refactoring pattern

**Changes:**
- Same constructor additions
- New overload accepting `object settings` → cast to `DeepSeekSettings`
- Replace single `_client` / `_cachedModelId` with `ConcurrentDictionary<string, ChatClient>` keyed by `{apiKey}:{endpoint}:{modelId}`
- Extract shared execution logic into private method

**Files to modify:**
- `AIFileProcessor.Core/Interfaces/IAiService.cs`
- `AIFileProcessor.Core/Services/GeminiService.cs`
- `AIFileProcessor.Core/Services/OpenAiService.cs`
- `AIFileProcessor.Core/Services/DeepSeekService.cs`

---

## Step 5: Refactor `ChineseTextProcessor` and `ProcessingOrchestrator`

### `IChineseTextProcessor.cs` — Add settings-aware overload

Add an overload that accepts a settings dictionary so the orchestrator can pass per-user settings down:

```
Task<AiApiResponse> ProcessChineseTextAsync(
	string modelId, string prompt, string data,
	CancellationToken ct = default);

// NEW: per-user settings overload
Task<AiApiResponse> ProcessChineseTextAsync(
	string modelId, string prompt, string data,
	IDictionary<string, object> providerSettings,
	CancellationToken ct = default);
```

The `providerSettings` dictionary maps provider names ("Gemini", "OpenAI", "DeepSeek") to their resolved settings objects. The processor selects the matching `IAiService` for the model, then calls the per-call settings overload.

### `ChineseTextProcessor.cs`

- New overload: when `providerSettings` is provided, calls `aiService.ProcessTextAsync(modelId, prompt, chunkText, settingsForProvider, ct)` instead of the no-settings overload
- The existing overload continues to use constructor-injected settings (backward compat)
- Selects the correct settings from the dictionary by checking which `IAiService` supports the model

### `ProcessingOrchestrator.cs`

- Constructor: add `ISettingsProvider` and `IUserContext` parameters
- `ProcessAsync(ProcessingOptions, CancellationToken)` — existing overload, now resolves settings internally:
  1. Call `ISettingsProvider` to get all three settings objects for the current `IUserContext.UserId`
  2. Build `providerSettings` dictionary
  3. Call `ChineseTextProcessor.ProcessChineseTextAsync(..., providerSettings, ct)`
- This means **MainForm and other callers don't need to change** — the orchestrator handles settings resolution transparently

**Files to modify:**
- `AIFileProcessor.Core/Interfaces/IChineseTextProcessor.cs`
- `AIFileProcessor.Core/Services/ChineseTextProcessor.cs`
- `AIFileProcessor.Core/Processing/ProcessingOrchestrator.cs`

---

## Step 6: Refactor Settings Managers to Use Per-User Files

### `GeminiSettingsManager.cs`, `OpenAiSettingsManager.cs`, `DeepSeekSettingsManager.cs`

**Current behavior:** Read from `IConfiguration`, write to shared `appsettings.json`.

**New behavior:** Delegate to `ISettingsProvider` for reads, write to per-user file.

**Changes per manager:**
- Constructor: inject `ISettingsProvider` and `IUserContext` (remove direct `IConfiguration` and `_configFilePath`)
- `ReadSettings()` → calls `ISettingsProvider.GetXxxSettingsAsync(_userContext.UserId, CancellationToken.None).GetAwaiter().GetResult()` (or make async)
- `SaveSettings(settings)` → calls `FileSettingsProvider.SaveSettingsAsync(_userContext.UserId, settings)` which writes to `%APPDATA%\AIFileProcessor\{userId}\settings.json`
- The managers become thin wrappers around the provider — they exist for backward compat with `SettingsForm`

**Alternative:** Consider having `SettingsForm` call `ISettingsProvider` directly and removing the three managers entirely. This is cleaner but requires updating `SettingsForm` more significantly. Recommend keeping managers for now to minimize form changes.

**Files to modify:**
- `AIFileProcessor.Core/Services/GeminiSettingsManager.cs`
- `AIFileProcessor.Core/Services/OpenAiSettingsManager.cs`
- `AIFileProcessor.Core/Services/DeepSeekSettingsManager.cs`

---

## Step 7: Update DI Registration (`ServiceCollectionExtensions.cs`)

### `AddAiFileProcessorCore` changes:

**Register new abstractions:**
```
// Register ISettingsProvider (file-based for desktop, swapped for web later)
services.AddSingleton<ISettingsProvider, FileSettingsProvider>();

// IUserContext is frontend-specific — register in each frontend's Program.cs
// (WinForms registers WindowsUserContext, Web registers HttpUserContext, etc.)
```

**Keep existing registrations:**
- `services.Configure<GeminiSettings>(...)` — stays, provides defaults for `FileSettingsProvider`
- `services.AddHttpClient<GeminiService>(...)` — stays, provides base HttpClient configuration
- The `IOptions<T>` binding continues to serve as the defaults source

**Update AI service registrations:**
- `GeminiService`, `OpenAiService`, `DeepSeekService` — now also need `ISettingsProvider` and `IUserContext` in their constructors, which DI resolves automatically

**Update settings manager registrations:**
- Now depend on `ISettingsProvider` and `IUserContext` instead of `IConfiguration` directly

**Files to modify:**
- `AIFileProcessor.Core/DependencyInjection/ServiceCollectionExtensions.cs`

---

## Step 8: Update WinForms Frontend (`Program.cs`)

### `Program.cs` changes:

**Register `IUserContext`:**
```
services.AddSingleton<IUserContext, WindowsUserContext>();
```

No other changes needed — the rest of the DI chain resolves automatically.

**Files to modify:**
- `AIFileProcessor/Program.cs`

---

## Step 9: Verify `MainForm.cs` Requires No Changes

`MainForm` calls `_processingOrchestrator.ProcessAsync(_options, ct)`. Since the orchestrator now resolves settings internally via `ISettingsProvider`, **MainForm requires no changes** for the processing pipeline.

The only MainForm interaction with settings is opening the `SettingsForm` via `_settingsFormFactory()`, which is updated in Step 6.

**Files to modify:**
- None (verify only)

---

## Step 10: Build and Test

1. Build the solution — verify no compilation errors
2. Run the WinForms app — verify:
   - Settings load from per-user file (or defaults if no file exists yet)
   - Settings save to `%APPDATA%\AIFileProcessor\{username}\settings.json`
   - `appsettings.json` is no longer modified by `SaveSettings`
   - API key is entered in SettingsForm and used in-memory for the session
   - Processing works with per-user settings
3. Simulate a second user (different Windows account or override `IUserContext`) — verify independent settings

---

## Dependency Order

```
Step 1 (interfaces) 
  → Step 2 (FileSettingsProvider) 
  → Step 3 (WindowsUserContext) 
  → Step 4 (AI services) 
  → Step 5 (processor/orchestrator) 
  → Step 6 (settings managers) 
  → Step 7 (DI registration) 
  → Step 8 (Program.cs) 
  → Step 9 (verify MainForm) 
  → Step 10 (build & test)
```

Each step builds on the previous. Steps 4 and 5 are the most involved — they touch the core processing pipeline. Steps 6–8 are wiring changes.

---

## Future Extensions (Out of Scope for This Plan)

- **Web frontend:** Implement `HttpUserContext` (resolves user from JWT/claims) and `WebSettingsProvider` (reconstructs settings from request). Register them in the web project's `Program.cs` instead of the file-based versions. No core changes needed.
- **DPAPI encryption for API keys:** Add `ProtectedData.Protect`/`Unprotect` in `FileSettingsProvider` for the `ApiKey` field in per-user files.
- **Client-side preferences (web):** SPA stores non-secret settings in `localStorage`, sends them with each request. `WebSettingsProvider` reads from request body/headers.
- **Settings validation:** Add `IValidateOptions<T>` for per-user settings to catch invalid combinations early.