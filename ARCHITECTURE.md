# Architecture Overview

This is the top-level architecture map for Hermes Zone Torba. It exists so a new engineer can understand the
whole system in ten minutes; each subsystem then has its own deep-dive in [`docs/`](docs/).

## C4: System Context

```mermaid
C4Context
    title HZT — System Context
    Person(user, "User", "Non-technical to power user; installs and operates local AI agents")
    Person(admin, "Platform Admin", "Manages fleets of HZT instances (enterprise)")

    System(hzt, "Hermes Zone Torba", "Installs, supervises, monitors, and extends local AI agents and LLM runtimes")

    System_Ext(hermes, "Hermes Agents", "Autonomous agent runtime HZT manages")
    System_Ext(ollama, "Ollama / LM Studio / vLLM", "Local LLM inference runtimes")
    System_Ext(cloud, "Cloud Model Providers", "OpenAI, Anthropic, Gemini — optional fallback")
    System_Ext(os, "Host Operating System", "Windows / Linux / macOS — processes, hardware, drivers")
    System_Ext(notify, "Notification Channels", "Discord, Slack, Telegram, Email, Push")

    Rel(user, hzt, "Installs, configures, monitors via Desktop or Web")
    Rel(admin, hzt, "Fleet management, policy, audit")
    Rel(hzt, hermes, "Installs / supervises / restarts")
    Rel(hzt, ollama, "Manages models, routes inference")
    Rel(hzt, cloud, "Optional inference fallback")
    Rel(hzt, os, "Monitors resources, executes sandboxed commands")
    Rel(hzt, notify, "Sends alerts and incident reports")
```

## C4: Containers

```mermaid
C4Container
    title HZT — Containers

    Person(user, "User")

    System_Boundary(hzt, "Hermes Zone Torba") {
        Container(desktop, "Desktop Shell", "Tauri (Rust + WebView)", "Tray, native OS hooks, background daemon, auto-update")
        Container(web, "Web Dashboard", "Next.js 16 / React 19", "Primary UI: monitoring, automation, marketplace, settings")
        Container(api, "Core API", "ASP.NET Core 10", "REST + SignalR; hosts all application services via CQRS")
        Container(worker, "Background Workers", "ASP.NET Core Hosted Services", "Scheduler, health checks, auto-healing loop, telemetry pump")
        ContainerDb(pg, "PostgreSQL", "Relational store", "Agents, models, workflows, audit log, users")
        ContainerDb(redis, "Redis", "Cache + pub/sub", "SignalR backplane, distributed locks, job queue, rate limiting")
        ContainerDb(vector, "Vector Store", "Qdrant", "Semantic / long-term memory, RAG")
        Container(sdk, "Plugin SDK Runtime", "Node.js sandbox / WASM", "Executes third-party plugins in isolation")
    }

    System_Ext(hermes, "Hermes Agents")
    System_Ext(llm, "Ollama / LM Studio / vLLM")
    System_Ext(os, "Host OS APIs")

    Rel(user, desktop, "Uses")
    Rel(user, web, "Uses (browser)")
    Rel(desktop, api, "Local HTTPS/WSS + native IPC")
    Rel(web, api, "HTTPS/WSS")
    Rel(api, pg, "EF Core")
    Rel(api, redis, "StackExchange.Redis")
    Rel(api, vector, "gRPC/HTTP")
    Rel(api, sdk, "Plugin invocation, sandboxed")
    Rel(worker, pg, "Reads/writes")
    Rel(worker, hermes, "Supervises")
    Rel(worker, llm, "Manages")
    Rel(desktop, os, "Native OS monitoring & process control")
    Rel(api, redis, "SignalR backplane for multi-instance fan-out")
```

## Layered view (per service)

Every backend module follows the same Clean Architecture layering — see
[docs/01-system-architecture.md](docs/01-system-architecture.md#layering) and
[docs/03-domain-model.md](docs/03-domain-model.md) for the full rationale and bounded-context breakdown.

```mermaid
flowchart TB
    Presentation["Presentation\nControllers · SignalR Hubs · Minimal APIs"]
    Application["Application\nCommands · Queries · Handlers (MediatR) · Validators · DTOs"]
    Domain["Domain\nAggregates · Entities · Value Objects · Domain Events · Specifications"]
    Infrastructure["Infrastructure\nEF Core · External Clients (Ollama/Hermes) · File System · Process Mgmt"]

    Presentation --> Application
    Application --> Domain
    Infrastructure --> Domain
    Infrastructure -.implements interfaces defined in.-> Application
```

**Dependency rule:** Domain has zero outward dependencies. Application depends only on Domain. Infrastructure
depends on Application's interfaces (never the reverse — no `Infrastructure` reference from `Application` or
`Domain`). Presentation composes everything at the edge. Enforced by architecture tests — see
[docs/16-testing.md](docs/16-testing.md#architecture-tests).

## Cross-cutting concerns

| Concern | Mechanism | Doc |
|---|---|---|
| Authn/Authz | JWT + refresh tokens, RBAC/ABAC | [docs/11-security.md](docs/11-security.md) |
| Supervision | AI Supervisor watches every managed process | [docs/05-ai-supervisor.md](docs/05-ai-supervisor.md) |
| Self-repair | Auto-Healing Engine, policy-driven | [docs/09-auto-healing.md](docs/09-auto-healing.md) |
| Extensibility | Plugin manifest + sandboxed runtime | [docs/10-plugin-system.md](docs/10-plugin-system.md) |
| Observability | OpenTelemetry + Serilog + health checks | [docs/16-testing.md](docs/16-testing.md), [docs/17-deployment.md](docs/17-deployment.md) |
| Real-time updates | SignalR hubs backed by Redis | [docs/04-api-spec.md](docs/04-api-spec.md#realtime-signalr) |

## Where to go next

- Building a new backend module → [docs/03-domain-model.md](docs/03-domain-model.md) + [docs/15-coding-standards.md](docs/15-coding-standards.md)
- Understanding how Hermes/Ollama get installed and supervised → [docs/06-hermes-integration.md](docs/06-hermes-integration.md), [docs/07-local-llm.md](docs/07-local-llm.md)
- Understanding failure recovery → [docs/05-ai-supervisor.md](docs/05-ai-supervisor.md), [docs/09-auto-healing.md](docs/09-auto-healing.md)
- Writing a plugin → [docs/10-plugin-system.md](docs/10-plugin-system.md), [sdk/](sdk/)
- Decisions and their trade-offs → [docs/adr/](docs/adr/)
