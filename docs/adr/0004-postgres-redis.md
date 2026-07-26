# ADR 0004: PostgreSQL as system of record, Redis for cache/pub-sub/coordination

## Status
Accepted

## Context
The system needs a durable relational store for aggregates with real invariants and relationships
(users/roles, incidents, workflows, audit log — see [docs/03-domain-model.md](../03-domain-model.md)), plus
fast ephemeral state for SignalR fan-out across horizontally scaled API instances, distributed locks for the
Scheduler ([docs/13-workflows.md](../13-workflows.md#scheduler)), and short-TTL caches (provider health,
permission sets).

Alternatives considered: MongoDB (weaker fit for the relational/RBAC parts of the model, JSONB in Postgres
covers the genuinely schemaless parts — see [docs/12-database.md](../12-database.md#conventions) — without
giving up relational integrity elsewhere); SQLite-only (fine for the desktop-embedded single-user deployment
shape, insufficient for Docker/Kubernetes multi-instance deployments); an in-memory-only cache with no shared
backplane (breaks SignalR fan-out and distributed locking the moment `api` scales beyond one replica).

## Decision
PostgreSQL is the single system of record for all durable domain data, accessed exclusively through EF Core
from the Infrastructure layer. Redis backs the SignalR backplane, the Scheduler's distributed locks, and
short-TTL derived caches — never the sole copy of anything that matters (see
[docs/12-database.md](../12-database.md#caching-strategy)). Qdrant is used for vector similarity search,
joined back to PostgreSQL `MemoryEntry` rows by reference id rather than being treated as a second system of
record for the same data.

For the desktop-embedded single-user deployment shape, the same EF Core model runs against a local
lightweight PostgreSQL container bundled with the installer rather than swapping to SQLite — keeping one
database engine across all deployment shapes was judged more valuable than shaving the desktop installer's
footprint further, given migrations, JSONB usage, and partitioning ([docs/12-database.md](../12-database.md#partitioning))
all assume Postgres semantics.

## Consequences
- One relational engine to operate, migrate, and reason about across every deployment shape — simpler
  operational story, at the cost of a heavier desktop installer than a SQLite-based alternative would produce.
- Redis becomes a hard runtime dependency, not an optional performance optimization — acceptable since it's
  already required for SignalR to scale horizontally at all.
- Any future move to a different primary datastore is a significant migration, not a config change — this ADR
  exists so that trade-off is explicit rather than accidental.
