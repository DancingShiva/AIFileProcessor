# Architecture

AIFileProcessor is organized so that **all** durable logic lives in `AIFileProcessor.Core`, and each
front-end (WinForms, Web) is a thin shell that wires it up with its own composition root.

## Deployment Scenarios

AIFileProcessor supports two deployment scenarios today, with a third planned:

### Desktop (single-user Windows machine)
- One process, one user. No concurrency concerns.
- `WindowsUserContext` resolves the current Windows identity as the user ID.
- Per-user settings stored at `%APPDATA%\AIFileProcessor\{userId}\settings.json`.

### Terminal Server (multi-user via Windows RDP)
- Each RDP session is a separate process, but the same user may open multiple sessions.
- **Cross-process file locking is required** — `FileSettingsProvider` uses a `Global\` mutex
  per user ID to serialize read-modify-write operations on the shared `%APPDATA%` settings file.
- `ProcessingLimits` is process-wide static — safe because each session is a separate process.
  This will change when porting to Web (single shared process for all users).
- API keys are shared from machine-level environment variables.
- Each user has their own non-secret preferences file.

### Web (planned, not yet production-ready)
- `AIFileProcessor.Web` is a prototype. The WinForms version is being fully fleshed out first,
  then the complete project will be ported to Web.
- Single process serves all users — `ProcessingLimits` statics become a shared-state concern.
- `HttpUserContext` resolves user from Entra ID JWT claims.
- `WebSettingsProvider` resolves per-user settings from the HTTP request, not files.
- Rate limiting, per-user quotas, and server-side API key security are web-only concerns
  not yet implemented.

## Projects and responsibilities

```
┌─────────────────────────┐   ┌──────────────────────────────┐
│ AIFileProcessor.WinForms │   │ AIFileProcessor.Web          │
│  MainForm / SettingsForm │   │  ProcessingController        │
│  Program.cs (host/DI)    │   │  SettingsController          │
│  WindowsUserContext      │   │  Program.cs (host/DI + JWT)  │
└───────────┬─────────────┘   │  HttpUserContext             │
			│                 │  WebSettingsProvider         │
			│                 └───────────────┬──────────────┘
			│ uses                            │ uses
			▼                                 ▼
		┌───────────────────────────────────────────────┐
		│            AIFileProcessor.Core                │
		│                                                │
		│  ProcessingOrchestrator ── resolves per-user   │
		│      settings, reasoning level, MaxTokens      │
		│  ChineseTextProcessor ── chunk/fan-out/merge   │
		│      ┌───────────┬─────────────┬────────────┐  │
		│   GeminiService OpenAiService DeepSeekService│  │
		│      └───────────┴─────────────┴────────────┘  │
		│  IAiService / ISettingsProvider / IUserContext │
		│  Parsing & formatting (ResponseParser,          │
		│   ResponseFormatter, AiApiResponse, Sandhi*)   │
		│  Config/settings models + ServiceCollectionExt │
		└───────────────────────────────────────────────┘
```

### Roles

- **`ProcessingOrchestrator`** — entry point for a processing request. Resolves the three providers'
  `*ModelSettings` from `ISettingsProvider` for the current `IUserContext.UserId`, applies the selected
  model's reasoning level and a clamped `MaxTokens`, bundles chunk/concurrency settings into a `ChunkingOptions`,
  builds the `IDictionary<string, IAiServiceSettings>` provider-settings map, calls the text processor, assigns
  IDs and cleaned length counts, and assembles the output. Front-end-agnostic.
- **`ChineseTextProcessor` (`IChineseTextProcessor`)** — splits the data into chunks (size from `ChunkingOptions`,
  default 40 lines), runs up to `MaxConcurrentChunks` concurrently (default 3, semaphore), preserves ordering
  by index, **fails fast** if any chunk fails (data-loss avoidance), and merges scratchpad/corrections/rows.
- **`IAiService` implementations** (`GeminiService`, `OpenAiService`, `DeepSeekService`) — one per provider.
  All inherit from `AiServiceBase<TSettings>`, which provides the shared shell: constructor pattern, model
  catalog lookups, the two `ProcessTextAsync` overloads, transient-exception detection, request validation,
  and telemetry helpers. Each derived class implements only the SDK-specific `ProcessTextCoreAsync` and
  `IsProviderTransient`. Registered as `IEnumerable<IAiService>` and the first one `SupportsModel(modelId)` wins.
- **`ISettingsProvider`** — resolves per-user settings. Two implementations:
  - `FileSettingsProvider` (desktop/terminal): defaults from `IConfiguration` + per-user override
	JSON at `%APPDATA%\AIFileProcessor\{userId}\settings.json`.
  - `WebSettingsProvider` (web): defaults from `IConfiguration` + non-secret preferences taken from
	the current HTTP request (`HttpContext.Items`).
- **`IUserContext`** — supplies the current user id. `WindowsUserContext` (Windows identity),
  `HttpUserContext` (Entra/JWT claims → `oid`/`sub`/`NameIdentifier`).

## Shared static helpers

Pure, stateless logic is kept out of DI as `internal static` helpers:

| Helper | Responsibility |
|---|---|
| `AiModelCatalog` | Model lookup — `SupportsModel` / `GetReasoningLevels` over a provider's `SupportedModels`, plus `GetProviderKey` to map an `IAiService` instance to its provider key string. |
| `AiServiceGuard` | Shared request-argument validation (non-null model ID, prompt, data + prompt-injection scan). |
| `TransientRetryPolicy` | `ExecuteAsync<T>` — transient retry with exponential backoff; caller supplies its `isTransient` predicate. |
| `TokenUtils` / `ModelTokenLimits` | Clamp requested `MaxTokens` to a provider/model hard max. |

## Composable client caching

Each `IAiService` caches its SDK client in a `ConcurrentDictionary` keyed by a **settings fingerprint**
(`${ApiKey}:${Endpoint}:...`). This guarantees that two users with different keys/endpoints never share
a cached client, avoiding cross-user credential bleed while still reusing clients for repeated calls.

## Dependency-injection composition

`ServiceCollectionExtensions.AddAiFileProcessorCore(IConfiguration)` registers everything the Core needs
(settings binding, typed `HttpClient`s per service, the three `IAiService`s, `FileSettingsProvider`,
`ProcessingOrchestrator`, factories). Each front-end then:

- **WinForms (`Program.cs`)** — builds a generic host, adds `appsettings.json` (+ User Secrets in dev,
  + environment variables), registers `WindowsUserContext`, forms, and the `StartupAppContext`.
- **Web (`Program.cs`)** — `AddHttpContextAccessor`, calls `AddAiFileProcessorCore`, **replaces**
  `ISettingsProvider` with `WebSettingsProvider` (singleton), registers `HttpUserContext`, and adds JWT
  authentication + an authorization fallback policy.

## Request-processing flow

```
UI/API request
   └─> ProcessingOrchestrator.ProcessAsync(options, ct)
		├─ ISettingsProvider.Get{Gemini|OpenAi|DeepSeek}ModelSettingsAsync(modelId, userId)
		├─ apply reasoning level + clamp MaxTokens (TokenUtils/ModelTokenLimits)
		├─ bundle chunk/concurrency settings into ChunkingOptions
		├─ ChineseTextProcessor.ProcessChineseTextAsync(modelId, prompt, data, providerSettings, chunking, ct)
		│     ├─ split data into chunks (ChunkingOptions.ChunkSize, default 40)
		│     ├─ pick IAiService where SupportsModel(modelId)
		│     ├─ per chunk: chunk-level TransientRetryPolicy retry (ChunkMaxRetries=3, 5s/10s/20s backoff)
		│     │     └─ IAiService.ProcessTextAsync(...) → service-level TransientRetryPolicy retry (MaxRetries=3, 1s/2s/4s backoff)
		│     ├─ parse each chunk's JSON → AiApiResponse (span-based)
		│     ├─ fail fast on first chunk error (after all retries exhausted)
		│     └─ merge → AiApiResponse
		├─ assign IDs (InitializeId) + cleaned length (GetCleanedCharacterCount)
		└─ assemble tab-delimited RawResponse → ProcessingResult
```

## Design invariants

- **Front-ends share everything** — no duplicated pipeline logic outside `Core`.
- **API keys are type-level secrets** — `[JsonIgnore]`, `init`-only; machine-config only.
- **Pure logic is not in DI** — static helpers; services hold only what must be injected
  (`ILogger`, `IConfiguration`/`IOptions`, `HttpClient`, provider settings provider).
- **Type-safe settings boundary** — `IAiServiceSettings` marker interface replaces `object` on the
  `IAiService.ProcessTextAsync` overload; `AiServiceBase<TSettings>` performs the type-safe cast.
- **Chunking bundled** — `ChunkingOptions` carries `ChunkSize` + `MaxConcurrentChunks` through the
  pipeline instead of two loose `int` parameters.
