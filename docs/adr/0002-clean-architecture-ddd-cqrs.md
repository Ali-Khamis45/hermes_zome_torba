# ADR 0002: Clean Architecture + DDD + CQRS (MediatR) for the backend

## Status
Accepted

## Context
The backend owns eight-plus bounded contexts (Hermes Management, Ollama/LLM Management, Supervision,
Automation, Security, Plugins, Memory — see [docs/03-domain-model.md](../03-domain-model.md)) that must evolve
independently, be individually testable, and potentially split into separate services later (Phase 4 fleet
management) without a full rewrite. We considered three shapes:

1. **Transaction-script / "fat controller" style** — fastest to start, but business logic tends to leak into
   controllers and gets duplicated once a second entry point (SignalR, background worker) needs the same
   logic.
2. **Layered architecture without explicit DDD** — better separation, but without aggregates enforcing
   invariants, validation logic scatters across handlers and the "what can this entity legally do" question
   has no single answer.
3. **Clean Architecture (Domain/Application/Infrastructure/Presentation) + DDD tactical patterns + CQRS via
   MediatR** — more upfront structure, but each bounded context becomes a self-contained unit with enforced
   invariants, and Application/Infrastructure separation means providers, storage, and process management can
   change without touching business logic.

## Decision
Adopt option 3, enforced by architecture tests (see [docs/15-coding-standards.md](../15-coding-standards.md#dependency-rules)),
not just documentation. Commands and queries are explicit MediatR requests with one handler each; cross-
context communication is domain events only (no direct cross-context service calls).

## Consequences
- More files per feature (command, handler, validator, DTO) than a transaction-script approach — mitigated by
  the vertical-slice folder convention in [docs/02-folder-structure.md](../02-folder-structure.md), which keeps
  a feature's files physically together despite the layer split.
- Onboarding has a steeper initial curve; offset by this ADR and the domain model doc existing before most
  code does.
- Bounded contexts are extractable into separate services later with contained blast radius, since they
  already don't call each other directly — this is what keeps the Phase 4 fleet/HA roadmap
  ([docs/18-roadmap.md](../18-roadmap.md)) plausible without a rewrite.
- Requires discipline to not "cheat" by injecting Infrastructure types into Application for convenience —
  enforced mechanically, not just by review, per [docs/15-coding-standards.md](../15-coding-standards.md).
