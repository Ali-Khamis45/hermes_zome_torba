# contracts

Schema-first source of truth for cross-boundary DTOs and domain events. See [shared/README.md](../README.md).

Until the Phase 1 codegen pipeline lands (see [docs/18-roadmap.md](../../docs/18-roadmap.md)), the
authoritative definitions live alongside their handlers in
`backend/src/Core/HermesZoneTorba.Application/*/Commands|Queries` and are mirrored by hand in
`frontend/lib/types.ts` — see the comment at the top of that file. This directory is where they
consolidate once generation is wired up, so frontend/backend/SDK types can never silently drift.
