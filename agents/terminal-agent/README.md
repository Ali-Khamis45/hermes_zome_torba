# terminal-agent

Executes shell commands on behalf of Hermes instances and workflows, under the constraints defined
in [docs/11-security.md#sandboxing](../../docs/11-security.md#sandboxing):

- Command whitelist/blacklist enforcement (policy-driven, not hardcoded)
- Restricted working directory (scoped to the owning instance)
- Resource-limited subprocess execution
- Streamed stdout/stderr back to the caller
- Cancellation support mid-execution
- Every invocation requiring approval is gated *before* dispatch by the backend, per
  [docs/11-security.md#approval-workflow](../../docs/11-security.md#approval-workflow) — this
  agent trusts that gate and does not re-implement authorization itself.

**Status:** Phase 2 implementation target — see [docs/18-roadmap.md](../../docs/18-roadmap.md).
Speaks the [`core/protocol`](../../core/protocol/) `task-request`/`task-result` messages with
`taskType` values `terminal.execute` and `terminal.cancel`.
