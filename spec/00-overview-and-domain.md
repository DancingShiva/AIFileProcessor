---
title: Overview & Domain
doc_id: OV
status: draft
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 00 — Overview & Domain

## Purpose

Establishes what AIFileProcessor is, who uses it, and the ubiquitous language used throughout
every other spec file, the codebase, prompts, and tests.

## Scope

- **In scope:** product purpose, user personas, domain vocabulary, deployment targets at a glance.
- **Out of scope:** layering rules ([01](01-architecture.md)), provider contracts
  ([02](02-llm-providers.md)), per-deployment concurrency detail ([10](10-quality-attributes.md)).

## Requirements

- **OV-001** The solution **must** process blocks of Mandarin Chinese text through an LLM provider
  and return, per sentence: pinyin (with tone sandhi applied), a word-by-word breakdown
  (gloss, part of speech, HSK level, notes), an English translation, and audio-reordering metadata.
- **OV-002** The primary output artifact **must** be Anki-ready, tab-delimited flashcard rows
  (`RawResponse`) alongside the structured `ProcessedSentenceRow` data.
- **OV-003** All domain logic **must** be shared across front-ends; a front-end-specific copy of any
  domain concept is a defect (see ARCH-001).
- **OV-004** The solution **must** remain deployable in three targets: single-user desktop,
  multi-session Terminal Server, and (planned) multi-user web. Target-specific behavior is defined
  in [10-quality-attributes.md](10-quality-attributes.md).
- **OV-005** The ubiquitous language below **must** be used consistently in code identifiers, spec
  text, prompt text, and test names. Synonyms for existing terms must not be introduced.

## Ubiquitous language

| Term | Meaning |
|---|---|
| **Chunk** | A group of consecutive input lines sent to the provider in one API call. Sized in *lines* (default 40), never characters. |
| **Provider** | An LLM backend: Gemini, OpenAI, or DeepSeek. Identified by a provider key string. |
| **Model** | A specific model on a provider (e.g. `gemini-3.5-flash`). Looked up in the provider's model catalog. |
| **Settings (defaults)** | The machine-level, DI-bound provider configuration (`IOptions<T>` snapshot). Never per-user. |
| **Preferences (per-user)** | Non-secret user choices (model, reasoning level, MaxTokens, temperature) resolved per call by `ISettingsProvider`. |
| **API key** | Provider credential. A machine-level secret; never per-user state. |
| **Visual string** | The post-correction simplified-Chinese written form of a sentence (prompt's `[VISUAL_STRING]`). |
| **Acoustic string** | The phonetic overlay used to derive pinyin, which may diverge from the visual string (prompt's `[ACOUSTIC_STRING]`). |
| **Sandhi** | Mandarin tone-sandhi rules applied to pinyin after parse; engine is `SandhiEngine`. |
| **HSK level** | Hanyu Shuiping Kaoshi difficulty band for a breakdown entry. |
| **Breakdown** | Pipe-delimited per-token analysis of a sentence (Chinese, pinyin, gloss, POS, HSK, note). |
| **Audio reordering** | Index ranges describing how visual tokens reorder in spoken/acoustic order. |
| **Scratchpad** | Provider reasoning text returned in the response for audit/debugging. |
| **Corrections** | Model-reported fixes or rejections applied to input lines (classified by prefix). |
| **Settings fingerprint** | The string key (`${ApiKey}:${Endpoint}:...`) used to cache SDK clients safely. |
| **Front-end** | A thin shell (WinForms or Web) containing no durable logic. |

## Invariants

- The domain terms above are the *only* names for these concepts anywhere in the solution.

## Related decisions

- [ADR-0002](decisions/ADR-0002-durable-logic-in-core.md)

## Open questions

- Formal definition of the `LINE <n> REJECTED:` correction contract (owner: PRM spec).
- Whether "Ollama" joins Provider as a fourth backend — artifacts exist
  (`OllamaService`, `OllamaModelSettings`) but it is not in README/provider docs.

## References

- [README.md](../README.md)
- [Prompt/ChineseProcessingPrompt.txt](../Prompt/ChineseProcessingPrompt.txt) (v9.2)
