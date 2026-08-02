# AIFileProcessor

A .NET 10 application for processing Chinese (Mandarin) text through large-language-model
pipelines. It sends blocks of Chinese text to an AI provider (Google Gemini, OpenAI, or
DeepSeek), which returns each sentence with pinyin (with tone sandhi applied), a
word-by-word breakdown (gloss, part of speech, HSK level, notes), an English translation,
and audio-reordering metadata — aimed at producing Anki-ready, tab-delimited flashcard rows.

The same core engine runs behind two front-ends:

- **AIFileProcessor.WinForms** — a Windows Forms desktop app (single-user or Terminal Server).
- **AIFileProcessor.Web** — an ASP.NET Core Web API (multi-user, Entra-ID-secured).

## Solution layout

| Project | Purpose |
|---|---|
| `AIFileProcessor.Core` | All shared logic: AI service clients, text/chunk processing, parsing/formatting, settings, DI registration. Front-end-agnostic. |
| `AIFileProcessor.Core.Tests` | xUnit test suite for the Core (715+ tests). |
| `AIFileProcessor.WinForms` | Desktop UI — `MainForm` pipeline + `SettingsForm`; hosted via generic-host DI. |
| `AIFileProcessor.Web` | Web API controllers (`/api/processing`, `/api/settings`), JWT (Entra ID) auth. |

See **[docs/architecture.md](docs/architecture.md)** for the component and data-flow view.

## The processing pipeline

1. **Input** — load a *prompt* file (system instruction/rules) and a *data* file (Chinese sentences,
   one per line).
2. **Per-user settings** — `ProcessingOrchestrator` resolves settings for all three providers from
   `ISettingsProvider`, applying the selected model's reasoning level and a clamped `MaxTokens`
   (clamped to the provider hard limit via `TokenUtils`/`ModelTokenLimits`).
3. **Chunked fan-out** — `ChineseTextProcessor` splits the data into 40-line chunks, processes up to
   3 concurrently (semaphore), and fails fast on any chunk error.
4. **Provider calls** — each `IAiService` (Gemini/OpenAI/DeepSeek) calls its API with
   transient-failure retry (exponential backoff via `TransientRetryPolicy`), using cached clients
   keyed by a settings fingerprint.
5. **Parse + sandhi** — the JSON response is deserialized (span-based, allocation-light in
   `AiApiResponse.FillFromJsonResponse`), and Mandarin tone sandhi is applied to pinyin.
6. **Assemble** — rows get generated IDs (prefix + padded sequence) and cleaned character counts,
   then are written as tab-delimited output for the UI or API caller.

## Technologies

- **.NET 10** (C#), Windows Forms, ASP.NET Core minimal hosting.
- **AI SDKs:** `Google.GenAI`, `OpenAI` (also used for the OpenAI-compatible DeepSeek endpoint).
- **Microsoft.Extensions.*** (Hosting, Http, Options, Logging.Abstractions) for DI/config.
- **Microsoft.Identity.Web** (JWT bearer / Entra ID) for the Web API.
- **xUnit v3 + Moq** for tests; **StyleCop.Analyzers** for style.

## Quick start

### Prerequisites
- .NET 10 SDK.
- API keys for the provider(s) you intend to use, from their consoles.

### Configure API keys
Keys are **never** stored in per-user files or sent to web clients. Provide them via
**environment variables** (recommended) or, for desktop dev, **User Secrets**. See
[docs/settings-and-authentication.md](docs/settings-and-authentication.md).

```powershell
$env:GeminiSettings__ApiKey   = "your-gemini-key"
$env:OpenAiSettings__ApiKey   = "your-openai-key"
$env:DeepSeekSettings__ApiKey = "your-deepseek-key"
```

### Run the desktop app
```powershell
dotnet run --project AIFileProcessor.WinForms
```
Load a prompt file and a data file, pick a model and reasoning level, then click **Process**.

### Run the Web API
```powershell
dotnet run --project AIFileProcessor.Web
```
The API requires an Entra ID bearer token and an `AzureAd` config section — see
[docs/deployment.md](docs/deployment.md#web-api). Endpoint reference:
[docs/web-api.md](docs/web-api.md).

### Run tests
```powershell
dotnet test AIFileProcessor.Core.Tests
```

## Documentation

- **README.md** (this file) — overview + quick start.
- **[docs/architecture.md](docs/architecture.md)** — component diagram, DI composition, request flow.
- **[docs/settings-and-authentication.md](docs/settings-and-authentication.md)** — configuration
  hierarchy, API-key handling, per-user isolation, Entra ID auth.
- **[docs/web-api.md](docs/web-api.md)** — HTTP endpoint reference with example requests/responses.
- **[docs/deployment.md](docs/deployment.md)** — desktop, Terminal Server, and Web API deployment.
- **[docs/SettingsProviderArchitecture.md](docs/SettingsProviderArchitecture.md)** — design rationale
  for the per-user `ISettingsProvider` pattern.

## Conventions

- StyleCop-enforced C# style; XML doc comments on public members.
- Static, stateless helpers for shared logic (`AiModelCatalog`, `TransientRetryPolicy`,
  `TokenUtils`, `ModelTokenLimits`, `AiServiceGuard`) — no extra DI for pure logic.
- **Security invariant:** `ApiKey` is `[JsonIgnore]` and `init`-only on every settings type;
  it is sourced only from machine-level configuration and is never persisted per-user or
  serialized to a client.

## License

Copyright (c) Midnight Mission Systems. All rights reserved.
