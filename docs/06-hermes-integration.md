# 06 · Hermes Integration

## Purpose

How HZT installs, configures, runs, and manages the lifecycle of Hermes AI Agent processes. This is the
Infrastructure-layer implementation backing the `HermesManagement` bounded context defined in
[docs/03-domain-model.md](03-domain-model.md#hermes-management).

## Responsibilities (Hermes Manager)

Install · Update · Configure · Repair · Restart · Stop · Health Checks · Diagnostics · Version Management ·
Configuration Validation — each maps to a command in
`backend/src/Core/HermesZoneTorba.Application/HermesManagement/Commands/`.

## Install flow

```mermaid
sequenceDiagram
    actor User
    participant Web
    participant Api
    participant App as InstallHermesCommandHandler
    participant Installer as HermesInstaller (Infrastructure)
    participant OS as Host OS
    participant DB as PostgreSQL

    User->>Web: Click "Install Hermes"
    Web->>Api: POST /api/v1/hermes/install {version?, installPath?}
    Api->>App: InstallHermesCommand
    App->>DB: Create HermesInstance (Status: Installing)
    App->>Installer: ResolveVersion() -> download URL + checksum
    Installer->>OS: Download release artifact
    Installer->>Installer: Verify checksum + signature
    Installer->>OS: Extract to installPath, set permissions
    Installer->>OS: Register as supervised process (not yet started)
    App->>DB: Update HermesInstance (Status: Stopped)
    App-->>Api: HermesInstallCompleted
    Api-->>Web: 201 Created + instance state
    Note over App,DB: Domain event HermesInstallCompleted published;<br/>AI Supervisor begins heartbeat monitoring once started.
```

Failure at any step transitions the instance to `Installing → Faulted` with the failure recorded, and the
partial install is rolled back (temp directory cleanup) rather than left half-extracted — install is
transactional from the user's point of view even though it spans several non-transactional OS operations.

## Configuration model

`HermesConfiguration` (value object, immutable — changes create a new configuration and are versioned):

| Field | Description |
|---|---|
| `provider` | Which LLM provider this instance targets (Ollama/LM Studio/vLLM/cloud), via [Provider Abstraction](07-local-llm.md#provider-abstraction) |
| `defaultModel` | Model identifier used when a task doesn't specify one |
| `maxConcurrentTasks` | Concurrency cap enforced by Hermes itself, mirrored here for supervision/resource planning |
| `resourceLimits` | RAM/VRAM ceiling HZT enforces before allowing `Start()` |
| `toolsEnabled` | Which built-in agents (Terminal, File, Browser, Git, Docker, …) this instance may invoke |
| `workingDirectory` | Sandboxed root the File/Terminal agents are scoped to for this instance |

Configuration changes go through `ConfigureHermesCommand`, are validated against currently-installed providers
and available resources before being accepted, and produce `HermesConfigurationChanged` — consumed by the
Automation Engine so workflows can react to config drift.

## Update flow

Updates follow the same download → verify → swap pattern as install, but preserve configuration and stop the
running instance first if `Status == Running` (graceful stop with a configurable drain timeout before force
stop). A failed update leaves the previous version intact (blue/green install directories, atomic symlink
swap on Linux/macOS, junction swap on Windows) — HZT never leaves an instance without a working binary.

## Repair flow

`RepairHermesCommand` is the "it's misbehaving but not obviously crashed" escape hatch, triggered manually or
suggested by the Auto-Healing Engine ([docs/09-auto-healing.md](09-auto-healing.md)):

1. Validate installation integrity (checksum the installed binaries against the manifest).
2. Validate configuration against current provider/model availability.
3. Clear transient state (stale locks, orphaned temp files).
4. Restart under supervision.
5. Record a `HermesRepaired` event with a diff of what was fixed, surfaced in the Diagnostics page.

## Health checks & diagnostics

Exposed via `GET /api/v1/hermes/{id}/status` (point-in-time) and streamed via `AgentMonitorHub`
(continuous). A health check aggregates: process liveness (from the [AI Supervisor](05-ai-supervisor.md)),
last successful task completion timestamp, configured-vs-actual provider connectivity, and resource headroom.
The Diagnostics dashboard page renders this plus the last N `Incident`s scoped to the instance.

## Version management

HZT tracks installed version, latest known-compatible version (from a version manifest fetched with the same
integrity checks as the binary itself), and whether an update is a patch/minor/major bump — major bumps
require explicit user confirmation even when auto-update is enabled, since they may carry Hermes-side breaking
config changes.

## Related documents

- [docs/09-auto-healing.md](09-auto-healing.md) — automatic recovery policies that call into repair/restart
- [docs/07-local-llm.md](07-local-llm.md) — the provider abstraction Hermes instances target
- [docs/11-security.md](11-security.md) — installer signature verification, sandboxed working directories
