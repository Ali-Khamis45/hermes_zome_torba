# ADR 0001: Record architecture decisions as ADRs

## Status
Accepted

## Context
HZT's architecture spans a dozen interacting subsystems (see [docs/01-system-architecture.md](../01-system-architecture.md)).
Without a record of *why* a decision was made, contributors re-litigate settled trade-offs, and the reasoning
behind constraints (e.g., why plugins can't have raw filesystem access) gets lost the moment the person who
made the call moves on.

## Decision
We record significant architecture decisions as ADRs in `docs/adr/`, numbered sequentially, following the
lightweight Michael Nygard format (Status / Context / Decision / Consequences). "Significant" means: it
constrains future work, it was non-obvious or contested, or reversing it later would be expensive.

An ADR is proposed in the same PR as the change it justifies (or before, for decisions that precede any code).
Reviewers evaluate the ADR alongside the implementation — approving the PR approves the decision.

## Consequences
- Every non-trivial architectural choice has a discoverable, dated rationale.
- ADRs are immutable once accepted; a changed decision gets a *new* ADR that supersedes the old one (marked
  `Superseded by ADR-00XX`), preserving history instead of rewriting it.
- Adds a small amount of process overhead to genuinely architectural PRs — acceptable given the alternative is
  undocumented tribal knowledge.
