---
title: Solution Specification — Index & Protocol
doc_id: SPEC-INDEX
status: living
last-reviewed: 2026-08-28
owner: DancingShiva
---

# Solution Specification

This directory is the **normative specification** for AIFileProcessor. It defines what the system
must do — testable, numbered requirements — while `docs/` is the human-facing onboarding narrative.
When `spec/` and `docs/` disagree, **`spec/` wins**.

This specification is model-agnostic: human developers and any AI coding assistant (GitHub Copilot,
Claude Code, or other tools) maintain it by the same protocol.

## Category map

| File | ID prefix | Covers |
|---|---|---|
| [00-overview-and-domain.md](00-overview-and-domain.md) | `OV` | Purpose, personas, ubiquitous language |
| [01-architecture.md](01-architecture.md) | `ARCH` | Layering, Core vs. front-ends, composition roots |
| [02-llm-providers.md](02-llm-providers.md) | `PROV` | `IAiService` contract, provider settings, catalogs |
| [03-prompt-contract.md](03-prompt-contract.md) | `PRM` | Prompt file role, response JSON schema, parse/verify |
| [04-processing-pipeline.md](04-processing-pipeline.md) | `PROC` | Orchestration, chunking, retries, merge, output assembly |
| [05-security.md](05-security.md) | `SEC` | Prompt-injection defenses, API-key handling |
| [06-settings-and-identity.md](06-settings-and-identity.md) | `SET` | `ISettingsProvider`, `IUserContext`, per-user state |
| [07-telemetry.md](07-telemetry.md) | `TEL` | Metrics, logging |
| [08-frontend-winforms.md](08-frontend-winforms.md) | `WF` | Desktop shell behavior |
| [09-frontend-web.md](09-frontend-web.md) | `WEB` | Web API surface, web-only concerns |
| [10-quality-attributes.md](10-quality-attributes.md) | `QA` | Concurrency model, limits, deployment targets |
| [11-testing-strategy.md](11-testing-strategy.md) | `TEST` | Test taxonomy, conventions, coverage requirements |
| [decisions/](decisions/) | `ADR-NNNN` | Immutable architectural decision records |

Coding standards (member ordering, headers, conventions) are model-facing and live in
[.github/copilot-instructions.md](../.github/copilot-instructions.md) and
[AGENTS.md](../AGENTS.md); they are referenced from specs, not duplicated here.

## File conventions

Every spec file and ADR carries YAML front-matter between `---` delimiters:

```yaml
---
title: <human-readable title>
doc_id: <SPEC-INDEX | 2-letter prefix | ADR-NNNN>
status: draft | living | deprecated        # specs
# status: proposed | accepted | superseded | rejected   # ADRs
last-reviewed: YYYY-MM-DD
owner: <github-handle>
superseded-by: ADR-NNNN                     # ADRs only, when superseded
---
```

- `status` for specs: `draft` (skeleton, being filled), `living` (normative, actively maintained),
  `deprecated` (no longer authoritative; kept for history).
- `last-reviewed` must be bumped whenever the file is materially reviewed or edited.
- `owner` is the accountable maintainer (a human GitHub handle); models may not set this on new files
  without user direction.

### Spec file template

Each category file uses this section layout (empty sections are allowed and expected for drafts):

1. **Purpose** — what this category governs and why it matters.
2. **Scope** — *In scope* / *Out of scope* bullet lists.
3. **Requirements** — numbered, testable statements: `<PREFIX>-NNN` followed by a rule using
   **must / must not / should / may** (RFC 2119 keywords in bold).
4. **Invariants** — properties that must hold regardless of specific requirements.
5. **Related decisions** — links to governing ADRs.
6. **Open questions** — unresolved items for future sessions.
7. **References** — source files, docs, external links.

Requirement IDs are **never reused or renumbered**. A retired requirement is marked
`~~(retired — superseded by ADR-NNNN)~~` rather than deleted, so citations in tests and commits
stay meaningful.

## Update protocol

1. **Read before edit.** Before modifying code in an area, read the corresponding spec file(s).
   Before modifying a provider, pipeline step, setting, or endpoint, the spec must be in your context.
2. **Same-commit rule.** Any commit that changes behavior already specified here must update the
   affected requirements in the same commit. A commit that satisfies the code but not the spec
   is incomplete.
3. **New behavior needs requirements.** New cross-cutting or user-visible behavior gets numbered
   requirements *before or alongside* implementation, not after the fact.
4. **Front-matter freshness.** On any material edit, update `last-reviewed`. Do not let
   `status: draft` files silently become relied-upon — promote them to `living` when accurate.
5. **Conflicts.** If code and spec diverge and the code is correct, fix the spec. If the spec is
   correct, fix the code. State which in the commit message.

## ADR protocol (`spec/decisions/`)

ADRs are **immutable, append-only** decision records in
[Nygard format](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions):
Context → Decision → Alternatives considered (with rationale for rejection) → Consequences.

- **Threshold rule:** an ADR is required only for decisions that are *expensive to reverse* or
  *cross-cut multiple categories*. Minor decisions are recorded as a one-line `**Rationale:**`
  note under the requirement they affect.
- **Supersession discipline:** when an ADR-backed decision changes, write a **new** ADR that
  supersedes the old one. Set the old ADR's front-matter to `status: superseded` and
  `superseded-by: ADR-NNNN`. Never rewrite the old ADR's content.
- **Numbering:** `ADR-NNNN`, zero-padded, never reused. The new ADR links back to what it supersedes.
- **Spec linkage:** any requirement whose behavior an ADR covers must reference it in
  *Related decisions*. A spec citing a `superseded` ADR is a freshness violation — update the link.
