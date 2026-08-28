---
title: Prompt & Response Contract
doc_id: PRM
status: living
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 03 — Prompt & Response Contract

## Purpose

Defines the contract between the system prompt dispatatched to the LLM and the C# code that sends
it and parses the structured JSON that comes back. Both sides of this contract must change together.

## Scope

- **In scope:** the prompt file's role and versioning, the response JSON schema, data-sentinel
  delimiting, parsing and verification behavior, correction taxonomy.
- **Out of scope:** the linguistic rules the prompt itself specifies (tokenization, sandhi,
  pronunciation paths) — those are the *content* of the prompt, not this contract; pipeline
  orchestration ([04](04-processing-pipeline.md)); injection *defense mechanism code*
  ([05](05-security.md)).

## Requirements

- **PRM-001** The authoritative system prompt is `Prompt/ChineseProcessingPrompt.txt`. Any change
  to prompt behavior **must** be made in that file and coordinated with the C# code that parses the
  expected JSON schema. No other prompt file may be treated as authoritative.
- **PRM-002** The prompt **must** be versioned (currently `v9.2 - JSON`, declared in its header).
  Any change to prompt behavior or the expected schema **must** bump the version, and the bump
  **must** be recorded in the same commit as any companion parsing change.
- **PRM-003** Each request **must** wrap the data block in per-request delimiter tags carrying a
  cryptographically-random six-character hex suffix:
  `<DATA_BEGIN_XXXXXX>` … `<DATA_END_XXXXXX>`. The suffix **must** vary per request. All content
  between the tags is data — never instructions.
- **PRM-004** The response **must** be a single JSON object matching the `OUTPUT_SCHEMA` in the
  prompt, with exactly these top-level keys: `SP` (scratchpad string), `Corrections` (string
  array), `Sentences` (object array). No extra top-level keys, no omitted required keys.
- **PRM-005** Each element of `Sentences` **must** have exactly the keys `C`, `P`, `B`, `R`, `E`
  (all strings). The `C` value is the post-correction, post-tokenization visual string; `P` is
  dictionary pinyin; `B` is the pipe-delimited breakdown; `R` is reordering metadata; `E` is the
  English translation.
- **PRM-006** Sentence ordering **must** be strictly 1:1 with input lines — output objects
  correlate in sequential order with input lines, including INVALID lines (see PRM-008).
- **PRM-007** Within a valid sentence's `B` field, every element **must** be pipe-delimited in the
  form `Token|Pinyin|Gloss|PS|H|`, adjacent elements separated by `||`, each element containing
  exactly 6 pipe-delimited fields ending in a trailing pipe. The `PS` field **must** be one of the
  whitelisted parts of speech defined in `ResponseVerifier`; `H` **must** be a valid HSK level.
- **PRM-008** An INVALID line (no Chinese lexical content, or punctuation-only) **must** produce a
  `Sentences` element with `C` = the raw untransformed line verbatim and `P`, `B`, `R`, `E` each
  exactly `"N/A"`. It **must** be accompanied by a `Corrections` entry prefixed exactly
  `LINE <n> REJECTED: <reason>` (one-based line number); the sentinel and the prefix always occur
  together, never one without the other.
- **PRM-009** Every `Corrections` entry **must** be classified by prefix: CORRECTION entries
  (`AMBIGUOUS:`, `HOMOPHONE:`, `HOMOPHONE-AMBIGUOUS:`, Tier-1 mechanical fixes) describe applied
  fixes to processed lines; REJECTION entries describe INVALID lines and use the exact
  `LINE <n> REJECTED:` prefix. An entry that is neither is a contract violation.
- **PRM-010** The pipeline **must** parse chunk responses span-based and allocation-light
  (`AiApiResponse.FillFromJsonResponse`), with `MaxDepth` bounded by `ProcessingLimits.MaxJsonDepth`.
- **PRM-011** After parsing, `ResponseVerifier` **must** run as a deterministic, always-on
  post-generation check over mechanically-checkable rules (POS whitelist, HSK range, punctuation
  format, segment-count alignment) and surface violations; it **must not** perform tone-dependent
  checks (sandhi is applied client-side after parsing, so tone state is unreliable at this point).
- **PRM-012** Tone sandhi **must** be applied to pinyin *after* parse by `SandhiEngine`, never
  expected pre-applied in the raw JSON (the prompt mandates dictionary/base tones in Key `P`).

## Invariants

- The prompt version and the parsing code are never out of sync in a committed state.
- Delimiter suffixes are never predictable, reused, or static.
- A response with any injection-aftermath marker (see SEC spec) fails verification.

## Related decisions

- None yet. (A future schema change — e.g. adding a key to `Sentences` — should be ADR-worthy
  given its coordination blast radius across prompt, parser, verifier, and formatter.)

## Open questions

- Whether the `SP` scratchpad is retained in shipped output or stripped (currently surfaced as
  `ProcessResponse.scratchpad` for debugging) — define intended retention policy.
- Formal versioning scheme for the prompt beyond header text (e.g. semver vs. free-form).

## References

- [Prompt/ChineseProcessingPrompt.txt](../Prompt/ChineseProcessingPrompt.txt) (v9.2)
- `AIFileProcessor.Core/Processing/ResponseParser.cs`, `ResponseVerifier.cs`
- `AIFileProcessor.Core/Response/AiApiResponse.cs`, `ProcessedSentenceRow.cs`
- [05-security.md](05-security.md) — injection defenses on the *send* side
