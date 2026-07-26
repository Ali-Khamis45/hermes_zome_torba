# 01 · System Architecture

## Purpose

The authoritative description of how HZT's containers, layers, and processes fit together — the document to
read before touching cross-module code. Subsystem docs (05–13) assume this as background and don't repeat it.

## Responsibilities

- Define the container topology (what deploys as what, and how they talk).
- Define the layering rules every backend module must follow.
- Define the primary data flows and event flows that cross module boundaries.
- Provide the sequence diagrams for the flows that touch the most subsystems, so nobody has to reconstruct
  them from code.

## Container topology

See [ARCHITECTURE.md](../ARCHITECTURE.md) for the C4 Context and Container diagrams. In short: **Core API**
(ASP.NET Core) is the single source of truth and the only process that talks to PostgreSQL directly. **Web**
and **Desktop** are both clients of the same API — the desktop shell adds native OS integration (tray,
notifications, background daemon) and can also embed the dashboard via a local WebView pointed at the API's
own bundled web build for fully offline operation.

```mermaid
flowchart TB
    subgraph Client Tier
        Web["Next.js Web Dashboard"]
        Desktop["Tauri Desktop Shell"]
    end

    subgraph API Tier ["Core API (ASP.NET Core, horizontally scalable)"]
        REST["REST Endpoints\n(Minimal APIs + Controllers)"]
        Hubs["SignalR Hubs\n(agent status, logs, metrics)"]
        MediatRBus["MediatR Pipeline\n(Commands / Queries / Notifications)"]
    end

    subgraph Worker Tier ["Background Workers (Hosted Services)"]
        Scheduler["Scheduler\n(cron / recurring jobs)"]
        SupervisorLoop["AI Supervisor Loop"]
        HealthLoop["OS Monitor Loop"]
        HealingLoop["Auto-Healing Loop"]
    end

    subgraph Data Tier
        PG[(PostgreSQL)]
        Redis[(Redis: cache, pub/sub, locks, backplane)]
        Vector[(Qdrant: vector memory)]
    end

    subgraph Managed Processes
        Hermes["Hermes Agent Process(es)"]
        Ollama["Ollama"]
        LMStudio["LM Studio"]
        VLLM["vLLM"]
        PluginSandbox["Plugin Sandbox Runtime"]
    end

    Web --> REST & Hubs
    Desktop --> REST & Hubs
    REST --> MediatRBus
    MediatRBus --> PG
    MediatRBus --> Redis
    MediatRBus --> Vector
    MediatRBus --> Hermes & Ollama & LMStudio & VLLM
    MediatRBus --> PluginSandbox

    Scheduler --> MediatRBus
    SupervisorLoop --> Hermes & Ollama
    SupervisorLoop --> MediatRBus
    HealthLoop --> MediatRBus
    HealingLoop --> MediatRBus

    Hubs -. fan-out via .-> Redis
```

## Layering

Every bounded context in `backend/src/Core` and `backend/src/Infrastructure` follows the same four layers.
This is enforced by NetArchTest-based architecture tests (see [docs/16-testing.md](16-testing.md#architecture-tests)),
not just convention.

```mermaid
classDiagram
    class Presentation {
        Controllers
        SignalR Hubs
        Minimal API endpoints
        Maps HTTP <-> MediatR requests
    }
    class Application {
        Commands + Handlers
        Queries + Handlers
        DTOs
        Validators (FluentValidation)
        Pipeline Behaviors
        Interfaces for Infrastructure
    }
    class Domain {
        Aggregates
        Entities
        Value Objects
        Domain Events
        Specifications
        Domain Services
        Zero external dependencies
    }
    class Infrastructure {
        EF Core DbContext + Configurations
        External process clients (Hermes, Ollama)
        File system, OS APIs
        Implements Application interfaces
    }
    Presentation --> Application
    Application --> Domain
    Infrastructure --> Domain
    Infrastructure ..|> Application : implements interfaces
```

**Rules:**

1. `Domain` references nothing outside itself (no EF Core, no ASP.NET Core, no third-party SDKs). It is pure
   C#.
2. `Application` references `Domain` only. It defines interfaces (`IHermesInstallerService`,
   `IApplicationDbContext`, …) that `Infrastructure` implements — dependency inversion, not dependency
   avoidance.
3. `Infrastructure` references `Application` (to implement its interfaces) and `Domain` (to persist/hydrate
   aggregates). It never references `Presentation`.
4. `Presentation` (the `Api` project) composes everything via DI at startup and translates HTTP/SignalR
   to/from MediatR requests. It contains no business logic.
5. Cross-module communication inside the backend happens through **MediatR notifications (domain events)**,
   never direct references between one bounded context's Application layer and another's.

## Data flow: model download

```mermaid
sequenceDiagram
    actor User
    participant Web as Web Dashboard
    participant Api as Core API
    participant OM as Ollama Manager (Application)
    participant Infra as Ollama Client (Infrastructure)
    participant Ollama as Ollama Runtime
    participant DB as PostgreSQL
    participant Hub as SignalR Hub

    User->>Web: Click "Download model: llama3.1:8b"
    Web->>Api: POST /api/models/download {name}
    Api->>OM: DownloadModelCommand
    OM->>OM: Validate against Model Registry (VRAM/disk checks)
    OM->>DB: Persist ModelDownload aggregate (status: Pending)
    OM->>Infra: IOllamaClient.PullModelAsync(name)
    Infra->>Ollama: POST /api/pull (stream)
    loop progress chunks
        Ollama-->>Infra: {status, completed, total}
        Infra-->>OM: ModelDownloadProgressChanged (domain event)
        OM->>DB: Update progress
        OM-->>Hub: Publish progress
        Hub-->>Web: SignalR: model.download.progress
    end
    Ollama-->>Infra: pull complete
    Infra-->>OM: ModelDownloadCompleted (domain event)
    OM->>DB: Update status: Completed, persist ModelRegistry entry
    OM-->>Hub: Publish model.download.completed
    Hub-->>Web: Update UI, enable model
```

## Event flow: agent crash recovery

This is the flow that most exercises the supervision/healing architecture — see
[docs/05-ai-supervisor.md](05-ai-supervisor.md) and [docs/09-auto-healing.md](09-auto-healing.md) for the
full state machines behind each step.

```mermaid
sequenceDiagram
    participant Hermes as Hermes Agent Process
    participant Sup as AI Supervisor (Worker)
    participant Bus as MediatR / Domain Events
    participant Heal as Auto-Healing Engine
    participant DB as PostgreSQL
    participant Hub as SignalR Hub
    participant Notif as Notification Agent

    Hermes--xSup: Process exits (code != 0) / heartbeat timeout
    Sup->>Sup: Classify failure (crash, hang, OOM, model error)
    Sup->>Bus: Publish AgentFaultDetected {agentId, faultType, evidence}
    Bus->>DB: Persist Incident (status: Detected)
    Bus->>Heal: AgentFaultDetected
    Heal->>Heal: Match policy for faultType (RestartPolicy, FallbackPolicy, ...)
    alt policy = auto-restart (within retry budget)
        Heal->>Hermes: Restart process (supervised)
        Heal->>DB: Update Incident (status: Recovering)
        Hermes-->>Sup: Heartbeat resumes
        Sup->>Bus: Publish AgentRecovered {agentId}
        Bus->>DB: Update Incident (status: Resolved)
    else policy = requires approval / retry budget exhausted
        Heal->>DB: Update Incident (status: AwaitingApproval)
        Heal->>Notif: Send alert (desktop + configured channels)
    end
    Bus-->>Hub: Publish incident.updated
    Hub-->>Web: Live incident timeline update
```

## Deployment views

HZT supports three deployment shapes; the container topology above is identical in all three, only the
process boundaries change. See [docs/17-deployment.md](17-deployment.md) for concrete manifests.

| Shape | Client | API | Data tier | Use case |
|---|---|---|---|---|
| **Desktop-embedded** | Tauri app | Same process tree, spawned as a local child process | SQLite-compatible mode via EF Core provider swap, or local Postgres container | Single-user local install (default) |
| **Docker Compose** | Browser → Web container | `api` container | `postgres` + `redis` + `qdrant` containers | Power users, small teams, self-hosted |
| **Kubernetes** | Browser / Desktop pointed at cluster | `api` Deployment, horizontally scaled | Managed Postgres + Redis + vector DB | Enterprise fleet, Phase 4 |

## Related documents

- [ARCHITECTURE.md](../ARCHITECTURE.md) — C4 diagrams, one-page map
- [docs/03-domain-model.md](03-domain-model.md) — bounded contexts and aggregates referenced above
- [docs/04-api-spec.md](04-api-spec.md) — concrete endpoint/event contracts for the flows above
- [docs/adr/0002-clean-architecture-ddd-cqrs.md](adr/0002-clean-architecture-ddd-cqrs.md) — why this layering was chosen
