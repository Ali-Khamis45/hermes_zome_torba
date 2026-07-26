# file-agent

Read/write/delete/move/rename/copy/watch/compress/extract/search — filesystem operations scoped to
the owning Hermes instance's configured `workingDirectory`
([docs/06-hermes-integration.md#configuration-model](../../docs/06-hermes-integration.md#configuration-model)).
Path traversal outside that scope is rejected at the Application layer before this agent is ever
invoked, per [docs/11-security.md#sandboxing](../../docs/11-security.md#sandboxing) — this agent
enforces the same boundary again defensively, not as the only line of defense.

**Status:** Phase 2 implementation target — see [docs/18-roadmap.md](../../docs/18-roadmap.md).
`taskType` values: `file.read`, `file.write`, `file.delete`, `file.move`, `file.copy`,
`file.watch`, `file.compress`, `file.extract`, `file.search`.
