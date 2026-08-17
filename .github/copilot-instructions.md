# Copilot Instructions for AIFileProcessor

## Project Overview

AIFileProcessor is a .NET 10 solution that sends Chinese text to various LLM APIs (e.g., DeepSeek) for structured linguistic processing and then parses the JSON response.

## The Prompt

The file `Prompt/ChineseProcessingPrompt.txt` is **the system prompt** that this project dispatches to the LLM API. The C# code (primarily in `AIFileProcessor.Core/Services/ChineseTextProcessor.cs`) loads this prompt, sends it alongside chunked Chinese text to the API, and processes the structured JSON response that comes back.

Any changes to prompt behavior should be made in that file and coordinated with the C# code that parses the expected JSON schema.

When the user refers to "the prompt," "the system prompt," "the spec," or the "Chinese processing prompt," they are referring to **`Prompt/ChineseProcessingPrompt.txt`** unless otherwise stated.

## C# Member Ordering Standard

All C# type members (classes, records, structs) must be ordered to satisfy StyleCop rules SA1201/SA1202/SA1204. The ordering is **element-type first**, then access modifier, then static/instance:

1. **By element type (global, across all access modifiers):** constructors -> properties -> methods -> fields
2. **Within each element type, by access modifier:** `public` -> `protected` -> `internal` -> `private`
3. **Within each access group, static before instance:** static members first, then instance members
4. **Within each group, alphabetical order:** members are sorted alphabetically by name

For example, a class should be laid out as:

```
public class Example
{
    // 1. ALL constructors — public first, then protected, then internal, then private
    // 2. ALL properties — public, then protected, then internal, then private (alphabetical within each)
    // 3. ALL methods — public, then protected, then internal, then private
    //    Within each access group: static methods first (alphabetical), then instance methods (alphabetical, abstract before virtual before concrete)
    // 4. ALL fields — same sub-ordering as properties
}
```

When adding a new member, insert it at the correct position to maintain this ordering. Do not append to the end of the class by default.

## Git Workflow

- **Never commit without explicit user instruction.** Only commit when the user says "commit" or explicitly asks to commit changes.
- **Never push without explicit user instruction.** Only push when the user says "push" or explicitly asks to push to remote. Do not combine commit and push into one action — always commit first, then ask whether to push.
- The user must explicitly approve each push to ensure control over versioning. This is non-negotiable.

## Deployment Targets

AIFileProcessor supports two deployment scenarios today, with a third planned:

### 1. Desktop (single-user Windows machine)
- One process, one user. No concurrency concerns.
- `WindowsUserContext` resolves the current Windows identity as the user ID.
- Per-user settings stored at `%APPDATA%\AIFileProcessor\{userId}\settings.json`.

### 2. Terminal Server (multi-user via Windows RDP)
- Each RDP session is a separate process, but the same user may open multiple sessions.
- **Cross-process file locking is required** for `FileSettingsProvider` — two sessions for the same user share the same `%APPDATA%` settings file. A `Global\` mutex per user ID serializes read-modify-write operations.
- `ProcessingLimits` is process-wide static — safe because each session is a separate process. This will change when porting to Web (single shared process for all users).
- API keys are shared from machine-level environment variables — all users on the same Terminal Server use the same keys.
- Each user has their own non-secret preferences file (temperature, max tokens, etc.).

### 3. Web (planned, not yet production-ready)
- `AIFileProcessor.Web` is a thought experiment / prototype. The WinForms version is being fully fleshed out first, then the complete project will be ported to Web.
- Single process serves all users — `ProcessingLimits` statics become a shared-state concern.
- `HttpUserContext` resolves user from Entra ID JWT claims (`oid`/`sub`/`NameIdentifier`).
- `WebSettingsProvider` resolves per-user settings from the HTTP request, not files.
- Rate limiting, per-user quotas, and server-side API key security are web-only concerns not yet implemented.

## Key Architectural Rules

- **All durable logic lives in `AIFileProcessor.Core`** — front-ends are thin shells.
- **API keys are never stored in per-user files** — `[JsonIgnore]` on settings models, machine-config only.
- **`FileSettingsProvider` uses cross-process `Mutex` locking** — `Global\` prefix, named per sanitized user ID.
- **Chunking is by lines, not characters** — `ChunkSize` default is 40 lines. The 32K character input limit does not bound chunk count.
- **Two retry layers**: service-level (per HTTP call, 3 retries) and chunk-level (per chunk, 3 retries with 5s/10s/20s backoff).

## Conventions

- **Line endings**: Always use CRLF (`\r\n`) as line terminators in all source files. This is enforced by `.gitattributes` with `eol=crlf` for C#, JSON, Markdown, and project files. When creating or editing files, ensure CRLF line endings to avoid Git normalization warnings.
- **Namespace style**: File-scoped namespaces (`namespace X;`) in all test files.
- **Test categorization**: `[Trait("Category", "Unit"|"Integration"|"WinForms")]` on all test classes.
- **Data-driven tests**: Prefer `[Theory]` + `[MemberData]` for complex object test data; `[InlineData]` for simple constants.
- **Copyright header**: `Midnight Mission Systems` copyright on all source files.
- **Validate after edit**: After editing any structured file (JSON, XML, YAML, `.csproj`, `.editorconfig`), validate it in its native format before moving on. For JSON, run `Get-Content 'file.json' -Raw | ConvertFrom-Json | Out-Null` in PowerShell. For C#, run `dotnet build`. Never assume an edit is correct without validating it.
