# core

The cross-runtime agent protocol and shared runtime contracts — the schema that lets built-in
agents ([`agents/`](../agents/)) and third-party plugins ([`plugins/`](../plugins/), via
[`sdk/`](../sdk/)) be implemented in any language while the backend supervises all of them
uniformly. See [docs/03-domain-model.md](../docs/03-domain-model.md) and
[docs/05-ai-supervisor.md](../docs/05-ai-supervisor.md) for how this protocol is consumed.

## `protocol/`

JSON Schema definitions for the messages exchanged between the backend and any supervised agent
process over stdio or a local socket:

- `agent-message.schema.json` — the envelope every message uses (id, type, correlationId, payload)
- `heartbeat.schema.json` — liveness signal consumed by the [AI Supervisor](../docs/05-ai-supervisor.md)
- `task-request.schema.json` / `task-result.schema.json` — the unit of work handed to an agent and
  its response

Generated TypeScript (`sdk/typescript/src/protocol/`) and C# (`shared/contracts/`) types are
produced from these schemas — the JSON Schema files are the single source of truth, not the
generated code. Regeneration is wired up in Phase 1 (see [docs/18-roadmap.md](../docs/18-roadmap.md)).

## Why this exists

Without a versioned, language-agnostic protocol, every new agent implementation (Terminal, File,
Browser, a third-party plugin) would need bespoke integration code in the backend. Instead, the
backend's `Infrastructure/ExternalServices` layer speaks exactly one protocol regardless of what's
on the other end of the process — see [docs/01-system-architecture.md#layering](../docs/01-system-architecture.md).
