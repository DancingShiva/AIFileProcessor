# Architecture

AIFileProcessor is organized so that **all** durable logic lives in `AIFileProcessor.Core`, and each
front-end (WinForms, Web) is a thin shell that wires it up with its own composition root.

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
  `*Settings` from `ISettingsProvider` for the current `IUserContext.UserId`, applies the selected
  model's reasoning level and a clamped `MaxTokens`, builds the `providerSettings` dictionary, calls
  the text processor, assigns IDs and cleaned length counts, and assembles the output. Front-end-agnostic.
- **`ChineseTextProcessor` (`IChineseTextProcessor`)** — splits the data into 40-line chunks, runs up to
  `MaxConcurrentChunks = 3` concurrently (semaphore), preserves ordering by index, **fails fast** if any
  chunk fails (data-loss avoidance), and merges scratchpad/corrections/rows.
- **`IAiService` implementations** (`GeminiService`, `OpenAiService`, `DeepSeekService`) — one per
  provider. Own the model catalog, client caching, retry, and the provider-specific request/response
  mapping. Registered as `IEnumerable<IAiService>` and the first one `SupportsModel(modelId)` wins.
- **`ISettingsProvider`** — resolves per-user settings. Two implementations:
  - `FileSettingsProvider` (desktop/terminal): defaults from `IConfiguration` + per-user override
	JSON at `%APPDATA%\AIFileProcessor\{userId}\settings.json`.
  - `WebSettingsProvider` (web): defaults from `IConfiguration` + non-secret preferences taken from
	the current HTTP request (`HttpContext.Items`).
- **`IUserContext`** — supplies the current user id. `WindowsUserContext` (Windows identity),
  `HttpUserContext` (Entra/JWT claims → `oid`/`sub`/`NameIdentifier`).

## Shared static helpers

Pure, stateless logic is kept out of DI as `internal static` helpers (no base classes):

| Helper | Responsibility |
|---|---|
| `AiModelCatalog` | Model lookup — `SupportsModel` / `GetReasoningLevels` over a provider's `SupportedModels`. |
| `AiServiceGuard` | Shared request-argument validation + `object settings` → typed-settings cast guard. |
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
		├─ ISettingsProvider.Get{Gemini|OpenAi|DeepSeek}SettingsAsync(userId)
		├─ apply reasoning level + clamp MaxTokens (TokenUtils/ModelTokenLimits)
		├─ ChineseTextProcessor.ProcessChineseTextAsync(modelId, prompt, data, providerSettings, ct)
		│     ├─ split data into chunks (40 lines)
		│     ├─ pick IAiService where SupportsModel(modelId)
		│     ├─ per chunk: IAiService.ProcessTextAsync(...)  [TransientRetryPolicy retry]
		│     ├─ parse each chunk's JSON → AiApiResponse (span-based)
		│     ├─ fail fast on first chunk error
		│     └─ merge → AiApiResponse
		├─ assign IDs (InitializeId) + cleaned length (GetCleanedCharacterCount)
		└─ assemble tab-delimited RawResponse → ProcessingResult
```

## Design invariants

- **Front-ends share everything** — no duplicated pipeline logic outside `Core`.
- **API keys are type-level secrets** — `[JsonIgnore]`, `init`-only; machine-config only.
- **Pure logic is not in DI** — static helpers; services hold only what must be injected
  (`ILogger`, `IConfiguration`/`IOptions`, `HttpClient`, provider settings provider).
