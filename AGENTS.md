# AGENTS.md

Instructions for any AI coding agent (GitHub Copilot, Claude Code, or other) working in this
repo. `.github/copilot-instructions.md` contains the same conventions in Copilot-specific form;
this file is the model-agnostic canonical pointer. **Keep both in sync.**

## Specification (read this first)

This repo has a strict, normative, multi-file specification under **`spec/`**:

| Path | Role |
|---|---|
| `spec/README.md` | Index, category map, front-matter schema, update protocol, ADR protocol — **start here** |
| `spec/00..11-*.md` | Category specifications (numbered requirements per facet) |
| `spec/decisions/` | Immutable ADRs (`ADR-NNNN`, append-only) |
| `.github/copilot-instructions.md` | Coding standards + git workflow (Copilot-facing mirror) |

**`spec/` is authoritative for *what the system must do*.** `docs/` is the human-facing onboarding
narrative. When they disagree, `spec/` wins.

### Protocol you must follow

1. **Read before edit.** Before modifying code in an area, read the corresponding `spec/` file(s)
   from the category map in `spec/README.md`.
2. **Same-commit rule.** If a commit changes behavior already specified in `spec/`, update the
   affected numbered requirements in the same commit.
3. **Requirement IDs are stable.** Never renumber or reuse them; mark retired ones as superseded.
4. **ADRs are append-only.** Changing an ADR-backed behavior requires a *new* ADR that supersedes
   the old one (`status: superseded` + `superseded-by:` on the old file). Never edit ADR content.
5. **Front-matter freshness.** Bump `last-reviewed` on any material edit; promote `draft` ->
   `living` when accurate.
6. **Sync both instruction files.** Any change to conventions, the spec protocol, or coding
   standards **must** be reflected in *both* `.github/copilot-instructions.md` and this
   `AGENTS.md` in the same commit.

## Project overview

AIFileProcessor is a .NET 10 solution that sends Chinese text to LLM APIs (Gemini, OpenAI,
DeepSeek) for structured linguistic processing and parses the JSON response.

- **All durable logic lives in `AIFileProcessor.Core`**; `WinForms` and `Web` are thin shells
  (ARCH-001).
- The system prompt is `Prompt/ChineseProcessingPrompt.txt`; its behavior changes are coordinated
  with the parser per `spec/03-prompt-contract.md`.
- API keys are machine-level only, never per-user (SEC-005..009).
- Three deployment targets: desktop, Terminal Server, and web (planned) — see
  `spec/10-quality-attributes.md`.

## Key conventions (summary)

- Line endings **CRLF** for all C#, JSON, Markdown, and project files (`.gitattributes eol=crlf`).
- StyleCop member ordering per `.github/copilot-instructions.md` (SA1201/SA1202/SA1204).
- Tests: xUnit v3, `[Trait("Category", ...)]` on every class, file-scoped namespaces, data-driven
  via `[Theory]`+`[MemberData]`.
- Git: never commit or push without explicit user instruction; one action at a time.

Detailed conventions, the C# member-ordering standard, and the git-workflow rules are in
[`.github/copilot-instructions.md`](.github/copilot-instructions.md).
