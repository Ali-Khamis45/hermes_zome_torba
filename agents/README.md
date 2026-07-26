# agents

Built-in agent implementations — each a standalone process speaking the [`core/protocol`](../core/protocol/)
contract over stdio, invoked by the backend's `Infrastructure/ExternalServices` layer and
supervised like any other managed process (see [docs/05-ai-supervisor.md](../docs/05-ai-supervisor.md)).

| Agent | Responsibility | Key doc |
|---|---|---|
| [`terminal-agent/`](terminal-agent/) | Shell command execution — whitelist/blacklist, sandbox, streaming output, cancellation | [docs/11-security.md](../docs/11-security.md#sandboxing) |
| [`file-agent/`](file-agent/) | Read/write/delete/move/rename/copy/watch/compress/extract/search, scoped to the instance's working directory | [docs/11-security.md](../docs/11-security.md#sandboxing) |
| [`browser-agent/`](browser-agent/) | Playwright-driven browser automation — tabs, forms, downloads, scraping, isolated profile per task | [docs/03-domain-model.md](../docs/03-domain-model.md) |
| [`git-agent/`](git-agent/) | Clone/commit/push/pull/branch/merge/PR/conflict resolution | — |
| [`docker-agent/`](docker-agent/) | Containers/images/volumes/networks/compose/logs/exec/build | — |
| [`notification-agent/`](notification-agent/) | Fan-out to Desktop/Discord/Slack/Telegram/Email/SMS/Push, per [docs/13-workflows.md](../docs/13-workflows.md) `SendNotificationAction` | — |

## Why agents are separate processes, not backend code

Isolating each agent as its own process — rather than a library the API links against — keeps a
crashing or hung agent from taking the API down with it (the AI Supervisor detects and recovers it
independently, see [docs/05-ai-supervisor.md](../docs/05-ai-supervisor.md)), and keeps the
sandboxing boundary (docs/11-security.md) enforced by the OS process boundary itself, not just
application-level checks.

## Adding a new built-in agent

1. Create `agents/<name>-agent/` implementing the `core/protocol` message contract.
2. Add the corresponding `Infrastructure/ExternalServices/<Name>/` client in the backend.
3. Document its task-type vocabulary (the `taskType` values it accepts) in this table and in
   [docs/13-workflows.md](../docs/13-workflows.md) if it's usable from workflow actions.
4. It goes through the same security review as the Terminal/File agents — new capability surface
   is not "just a helper script."
