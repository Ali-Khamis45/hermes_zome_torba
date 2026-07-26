# shared

The shared kernel: versioned DTOs and event contracts used across the backend, frontend, and SDKs
— the single source of truth OpenAPI/AsyncAPI generation reads from. See
[docs/02-folder-structure.md#core-agents-plugins-sdk-shared](../docs/02-folder-structure.md) and
[docs/15-coding-standards.md](../docs/15-coding-standards.md).

## `contracts/`

Schema-first contract definitions (JSON Schema / OpenAPI fragments) for the DTOs documented in
[docs/04-api-spec.md](../docs/04-api-spec.md) and the domain events cataloged in
[docs/03-domain-model.md](../docs/03-domain-model.md). Currently hand-authored alongside the
backend records in `backend/src/Core/HermesZoneTorba.Application/*/Commands|Queries`; Phase 1 adds
the codegen pipeline that keeps `frontend/lib/types.ts` and `sdk/*/protocol` in sync with this
directory automatically (tracked in [docs/18-roadmap.md](../docs/18-roadmap.md)).

## Placement rule

Something belongs in `shared/` only if it crosses a process boundary (backend ↔ frontend ↔ SDK).
Code that's merely reusable within one language/runtime belongs in that project instead — see
[docs/02-folder-structure.md#placement-rules](../docs/02-folder-structure.md#placement-rules).
