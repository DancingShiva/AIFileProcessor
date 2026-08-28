---
title: Testing Strategy
doc_id: TEST
status: draft
last-reviewed: 2026-08-28
owner: DancingShiva
---

# 11 — Testing Strategy

## Purpose

Defines the test taxonomy, naming/categorization conventions, data-driven test rules, and the
conditions under which tests must accompany spec'd behavior.

## Scope

- **In scope:** test projects, categories, `[Trait]` conventions, `[Theory]`/`[MemberData]` rules,
  requirement-to-test linkage.
- **Out of scope:** CI pipeline configuration, code-coverage thresholds (none defined yet).

## Requirements

- **TEST-001** Tests **must** live in the two test projects: `AIFileProcessor.Core.Tests` (Core
  logic) and `AIFileProcessor.WinForms.Tests` (desktop shell behavior). New test files follow the
  existing naming convention `<Subject>Tests.cs` mirroring the source folder structure.
- **TEST-002** Every test class **must** carry a category trait:
  `[Trait("Category", "Unit" | "Integration" | "WinForms")]`.
- **TEST-003** Test files **must** use file-scoped namespaces (`namespace X;`).
- **TEST-004** Data-driven tests **must** prefer `[Theory]` + `[MemberData]` for complex object
  test data and `[InlineData]` for simple constants. Non-trivial object graphs must not be flattened
  into long `[InlineData]` lists.
- **TEST-005** New cross-cutting or user-visible behavior specified in the other category files
  **must** ship with tests covering the normative path and at least one failure/edge path.
- **TEST-006** Tests that exercise a numbered requirement **should** reference its ID (e.g.
  `PROC-004`) in the test name or `[Trait]`/comment, to keep requirement-to-test traceability.
- **TEST-007** The full suite **must** pass with `0` warnings from both `dotnet build` and the IDE
  analyzer set (StyleCop, CA, xUnit rules at message severity) before a spec-affecting change is
  considered complete.

## Invariants

- Normative requirements that are mechanically checkable must have at least one test.
- A spec-cited test that is deleted without a replacement is a freshness violation.

## Related decisions

- None yet (coverage thresholds and CI gating are ADR candidates once the suite matures).

## Open questions

- Whether to adopt a coverage minimum, and at what value.
- Whether the WinForms category ever merges into broader UI automation testing.

## References

- [AGENTS.md](../AGENTS.md) and [.github/copilot-instructions.md](../.github/copilot-instructions.md)
  — conventions enforced by tooling
- `AIFileProcessor.Core.Tests/`, `AIFileProcessor.WinForms.Tests/`
- [docs/architecture.md](../docs/architecture.md)
