# 02 · Folder Structure

## Purpose

The annotated monorepo tree. When in doubt about where new code belongs, this document — not precedent in a
random existing file — is the source of truth. Structural changes to this tree go through an ADR
(see [docs/adr/0001-record-architecture-decisions.md](adr/0001-record-architecture-decisions.md)).

## Top level

```
hermes-zone-torba/
├── backend/            ASP.NET Core 10 solution — Clean Architecture, DDD, CQRS
├── frontend/            Next.js 16 / React 19 web dashboard
├── desktop/            Tauri desktop shell (Rust + WebView)
├── core/                Cross-runtime agent protocol & shared runtime contracts
├── agents/            Built-in agent implementations (Terminal, File, Browser, IDE, Git, Docker, Notification)
├── plugins/            Plugin manifest schema, sandbox runtime, first-party example plugins
├── sdk/                Plugin & integration SDKs (TypeScript, .NET)
├── shared/            Shared kernel: versioned DTOs / event contracts used across languages
├── infrastructure/    Docker, Kubernetes, provisioning, observability config
├── scripts/            Dev bootstrap, codegen, release automation
├── docs/                Architecture, domain, security, API, process documentation (this tree)
├── examples/            End-to-end example workflows and automations
├── tests/                Cross-cutting suites: E2E, contract, load/perf (module-local tests live next to their module)
├── .github/            CI workflows, issue/PR templates, CODEOWNERS, dependabot
├── docker-compose.yml
├── .env.example
├── ARCHITECTURE.md
├── CONTRIBUTING.md
└── README.md
```

## `backend/` — Clean Architecture layout

```
backend/
├── HermesZoneTorba.slnx
├── src/
│   ├── Core/
│   │   ├── HermesZoneTorba.Domain/               # Zero external deps. Pure business rules.
│   │   │   ├── Common/                            # Entity, AggregateRoot, ValueObject, IDomainEvent, IRepository<T>
│   │   │   ├── HermesManagement/                  # Bounded context: HermesInstance aggregate + events
│   │   │   ├── OllamaManagement/                  # Bounded context: OllamaModel / ModelRegistry aggregates
│   │   │   ├── Supervision/                       # Incident, FaultDetection aggregates
│   │   │   ├── Automation/                        # Workflow, Trigger, Condition, Action aggregates
│   │   │   ├── Security/                          # User, Role, Permission, AuditLog aggregates
│   │   │   ├── Plugins/                            # PluginInstallation, PluginManifest aggregates
│   │   │   └── Memory/                            # MemoryEntry, KnowledgeGraphNode aggregates
│   │   │
│   │   └── HermesZoneTorba.Application/          # Depends only on Domain
│   │       ├── Common/
│   │       │   ├── Interfaces/                    # IApplicationDbContext, ICurrentUser, IDateTimeProvider, ...
│   │       │   └── Behaviors/                     # ValidationBehavior, LoggingBehavior, AuthorizationBehavior
│   │       ├── HermesManagement/
│   │       │   ├── Commands/                      # InstallHermes/, UpdateHermes/, RestartHermes/, ...
│   │       │   └── Queries/                        # GetHermesStatus/, ListHermesInstances/, ...
│   │       ├── OllamaManagement/{Commands,Queries}/
│   │       ├── Supervision/{Commands,Queries}/
│   │       ├── Automation/{Commands,Queries}/
│   │       ├── Security/{Commands,Queries}/
│   │       └── Plugins/{Commands,Queries}/
│   │
│   ├── Infrastructure/
│   │   └── HermesZoneTorba.Infrastructure/        # Implements Application interfaces
│   │       ├── Persistence/
│   │       │   ├── ApplicationDbContext.cs
│   │       │   ├── Configurations/                # IEntityTypeConfiguration<T> per aggregate
│   │       │   └── Migrations/
│   │       ├── ExternalServices/
│   │       │   ├── Hermes/                        # HermesProcessClient, HermesInstaller
│   │       │   ├── Ollama/                        # OllamaHttpClient
│   │       │   ├── LmStudio/, VLlm/
│   │       │   └── CloudProviders/                # OpenAI/Anthropic/Gemini adapters (provider abstraction)
│   │       ├── Os/                                # Process, filesystem, hardware detection (per-OS impls)
│   │       ├── Security/                          # JWT issuer, encryption, sandbox executor
│   │       └── Realtime/                          # SignalR group/backplane helpers
│   │
│   └── Api/
│       └── HermesZoneTorba.Api/                    # Presentation — composition root
│           ├── Controllers/
│           ├── Hubs/                              # AgentMonitorHub, LogStreamHub, MetricsHub
│           ├── Middleware/                        # ExceptionHandling, CorrelationId, ProblemDetails mapping
│           ├── Program.cs
│           └── appsettings.json
│
└── tests/
    ├── HermesZoneTorba.Domain.UnitTests/
    ├── HermesZoneTorba.Application.UnitTests/
    ├── HermesZoneTorba.Infrastructure.IntegrationTests/
    ├── HermesZoneTorba.Api.IntegrationTests/
    └── HermesZoneTorba.Architecture.Tests/         # NetArchTest layering rules
```

Each `Application/<BoundedContext>/Commands/<UseCase>/` folder is "vertical slice" style:
`<UseCase>Command.cs`, `<UseCase>CommandHandler.cs`, `<UseCase>CommandValidator.cs` live together — see the
working example at [`backend/src/Core/HermesZoneTorba.Application/HermesManagement/Commands/InstallHermes/`](../backend/src/Core/HermesZoneTorba.Application/HermesManagement/Commands/InstallHermes/).

## `frontend/`

```
frontend/
├── app/
│   ├── (dashboard)/                # Route group — shared dashboard chrome/layout
│   │   ├── layout.tsx              # Sidebar nav, top bar, theme provider
│   │   ├── page.tsx                # Overview
│   │   ├── agents/
│   │   ├── models/
│   │   ├── automation/, memory/, plugins/, marketplace/, workflow-builder/
│   │   ├── logs/, notifications/, security/, diagnostics/, updates/, terminal/
│   │   └── settings/
│   ├── (auth)/                     # Sign-in / onboarding, no dashboard chrome
│   └── layout.tsx                  # Root layout
├── components/
│   ├── ui/                          # shadcn/ui primitives
│   └── <feature>/                   # Feature-scoped composite components
├── lib/
│   ├── api-client.ts                 # Typed fetch wrapper (generated from OpenAPI, see docs/04-api-spec.md)
│   ├── signalr-client.ts
│   └── hooks/                        # React Query hooks per domain (useAgents, useModels, ...)
├── store/                            # Zustand stores (client-only UI state)
├── public/
├── next.config.ts
├── tailwind.config.ts
└── package.json
```

## `desktop/`

```
desktop/
├── src-tauri/
│   ├── src/
│   │   ├── main.rs                   # Tray, window management, updater
│   │   ├── commands/                 # Tauri commands (IPC surface exposed to the WebView)
│   │   ├── daemon/                    # Background daemon: process supervision, native OS hooks
│   │   └── native/                    # Per-OS native API bindings (Windows/Linux/macOS)
│   ├── icons/
│   ├── Cargo.toml
│   └── tauri.conf.json
├── src/                                # Optional thin web layer if not reusing frontend/ build directly
└── package.json
```

## `core/`, `agents/`, `plugins/`, `sdk/`, `shared/`

- **`core/protocol/`** — the versioned message schema agents and the backend speak (JSON Schema + generated
  TS/C# types). This is the contract that lets `agents/*` be implemented in any language.
- **`agents/<name>-agent/`** — one directory per built-in agent (`terminal-agent`, `file-agent`,
  `browser-agent`, `git-agent`, `docker-agent`, `notification-agent`, …), each a standalone process that
  speaks the `core/protocol` contract over stdio or a local socket, invoked by the backend via
  `Infrastructure/ExternalServices`.
- **`plugins/`** — `manifest.schema.json`, the sandbox runtime, and `examples/` first-party plugins used as
  the reference implementation for third-party authors.
- **`sdk/typescript/`, `sdk/dotnet/`** — published packages third parties import to build plugins/integrations
  against `core/protocol` and the REST/SignalR API without depending on internal backend code.
- **`shared/contracts/`** — versioned DTOs and event contracts shared between backend, frontend, and SDKs, the
  single source of truth OpenAPI/AsyncAPI generation reads from.

## Placement rules

1. Business logic never lives in `Api/Controllers` or in React components — it lives in `Application` command
   handlers or in typed hooks that call the API, respectively.
2. A new bounded context gets its own folder under `Domain/`, `Application/`, and a matching `Configurations/`
   entry under `Infrastructure/Persistence/` — never bolted onto an existing aggregate for convenience.
3. Anything one platform needs and another doesn't stays out of `shared/` — `shared/` is for contracts that
   cross a process boundary, not for "code that happens to be reusable."
4. Tests live next to what they test (`backend/tests/*`, `frontend/**/__tests__`) except cross-cutting E2E and
   load suites, which live in the root `tests/` — see [docs/16-testing.md](16-testing.md).
