---
title: All durable logic lives in AIFileProcessor.Core
doc_id: ADR-0002
status: accepted
last-reviewed: 2026-08-28
owner: DancingShiva
---

# ADR-0002: All durable logic lives in AIFileProcessor.Core

## Context

The solution has three front-end consumers of the same engine (WinForms desktop, Terminal Server
RDP sessions, planned Web). Duplicating pipeline logic across them guarantees divergence and
untestable behavior in at least one front-end.

## Decision

All durable logic lives in `AIFileProcessor.Core`. Front-ends are thin shells containing only
UI/wiring, their composition root, and front-end-specific `IUserContext`/`ISettingsProvider`
implementations.

## Alternatives considered

- **Duplicate a subset of logic per front-end** — rejected: each copy drifts, and the WinForms
  pipeline lessons wouldn't carry to Web (the stated plan is to port the fleshed-out WinForms
  behavior to Web).
- **Shared logic via a second "shared" assembly per front-end pair** — rejected: reintroduces
  layering ambiguity; one Core seam is cleaner than N pairwise ones.

## Consequences

- Each front-end is independently skinny and replaceable; the Web port is a composition exercise
  rather than a rewrite.
- Front-ends cannot add pipeline behavior without a Core change, enforcing the single home for
  domain decisions.

Spec: [01-architecture.md](../01-architecture.md) ARCH-001..003;
[08-frontend-winforms.md](../08-frontend-winforms.md) WF-001;
[09-frontend-web.md](../09-frontend-web.md).
