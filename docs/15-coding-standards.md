# 15 · Coding Standards

## Purpose

The rules that keep a multi-year, multi-contributor Clean Architecture / DDD / CQRS codebase from decaying
into the "big ball of mud" that pattern is meant to prevent. Enforced partly by tooling (`.editorconfig`,
analyzers, architecture tests) and partly by review discipline.

## Dependency rules (enforced by architecture tests)

See [ARCHITECTURE.md](../ARCHITECTURE.md#layering) for the diagram. Concretely, in
`backend/tests/HermesZoneTorba.Architecture.Tests`:

```csharp
[Fact]
public void Domain_Should_Not_DependOn_OtherLayers()
{
    var result = Types.InAssembly(DomainAssembly)
        .ShouldNot()
        .HaveDependencyOnAny("HermesZoneTorba.Application", "HermesZoneTorba.Infrastructure", "HermesZoneTorba.Api")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}

[Fact]
public void Handlers_Should_Have_NameEndingWith_Handler()
{
    var result = Types.InAssembly(ApplicationAssembly)
        .That().ImplementInterface(typeof(IRequestHandler<,>))
        .Should().HaveNameEndingWith("Handler")
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

These tests run in CI (`backend-ci.yml`, `Category=Architecture`) and fail the build on violation — layering
is not a code-review-only convention.

## Naming

| Element | Convention | Example |
|---|---|---|
| C# namespaces/folders | `HermesZoneTorba.<Layer>.<BoundedContext>[.<SubArea>]` | `HermesZoneTorba.Application.HermesManagement.Commands.InstallHermes` |
| Commands | `<Verb><Noun>Command` | `InstallHermesCommand`, `RestartHermesCommand` |
| Queries | `<Verb><Noun>Query` (verb is usually `Get`/`List`) | `GetHermesStatusQuery`, `ListModelsQuery` |
| Handlers | `<RequestName>Handler` | `InstallHermesCommandHandler` |
| Domain events | `<Noun><PastTenseVerb>` | `HermesInstallCompleted`, `ModelDownloadFailed` |
| Validators | `<RequestName>Validator` | `InstallHermesCommandValidator` |
| React components | `PascalCase`, one per file matching the filename | `AgentStatusCard.tsx` → `AgentStatusCard` |
| React hooks | `use<Noun>` | `useAgents`, `useModelDownloadProgress` |
| Zustand stores | `use<Domain>Store` | `useDashboardStore` |
| TS/Rust files | `kebab-case` | `agent-monitor-hub.ts`, `process-supervisor.rs` |

## SOLID & Clean Code in practice here

- **Single Responsibility**: a command handler orchestrates one use case. If a handler needs to coordinate
  multiple aggregates, it does so through their public methods/repositories, not by reaching into another
  bounded context's internals — cross-context coordination happens via domain events, not shared handlers.
- **Open/Closed**: new providers, plugins, and healing actions are added by implementing an existing interface
  (`ILlmProvider`, `HealingAction`), never by adding a branch to a switch statement in a shared handler — see
  [docs/07-local-llm.md](07-local-llm.md#provider-abstraction) and [docs/09-auto-healing.md](09-auto-healing.md#policy-model).
- **Dependency Inversion**: Application defines interfaces, Infrastructure implements them — this is the whole
  point of the layering, not a suggestion (see [ARCHITECTURE.md](../ARCHITECTURE.md#layering)).
- **DDD conventions**: aggregates expose behavior methods (`instance.Restart(reason)`), never public setters
  that let calling code put the aggregate in an invalid state. All domain events are raised inside the
  aggregate method that causes them, added to `Entity._domainEvents`, and dispatched after `SaveChangesAsync`
  succeeds (transactional-outbox-adjacent — see [docs/12-database.md](12-database.md) for the persistence
  side).

## Error handling

- Domain layer throws domain-specific exceptions (`DomainException` subtypes) for invariant violations — never
  generic `Exception` or framework exceptions.
- Application layer: validation failures surface as `ValidationException` from the `ValidationBehavior`
  pipeline step (FluentValidation), not scattered `if` checks in handlers.
- Presentation layer: a single `ExceptionHandlingMiddleware` maps domain/validation/not-found exceptions to
  RFC 9457 Problem Details responses (see [docs/04-api-spec.md](04-api-spec.md#conventions)) — controllers
  never contain try/catch for this.
- Frontend: React Query's error boundaries + a shared `ApiError` type parsed from Problem Details responses;
  no swallowed promise rejections.

## Logging

Structured logging via Serilog, message templates (not string interpolation) so fields stay queryable:

```csharp
_logger.LogInformation("Hermes instance {InstanceId} transitioned to {Status}", instance.Id, instance.Status);
```

Every request/background job carries a correlation id propagated into every log line and into OpenTelemetry
spans — see [docs/16-testing.md](16-testing.md) and [docs/17-deployment.md](17-deployment.md) for the
observability pipeline this feeds.

## Configuration

Strongly-typed via the Options pattern (`IOptions<T>`/`IOptionsMonitor<T>`), bound from `appsettings.json` +
environment variables + (in production) a secrets backend — never `IConfiguration["Some:Key"]` string lookups
scattered through business code. Each bounded context that needs configuration defines its own options class
(`SupervisorOptions`, `AutoHealingOptions`) validated at startup via `ValidateOnStart()` so a bad config fails
fast at boot, not on first use at 2am.

## Documentation

- Public API surfaces (Application interfaces, SDK packages) carry XML doc comments / TSDoc — these feed
  generated reference docs.
- A comment explains *why*, never *what* — code should be legible enough that "what" is redundant; reserve
  comments for non-obvious constraints (e.g., "ordering here matters because X").
- New bounded contexts or subsystems get a corresponding `docs/*.md` entry before merge, not "later."

## Pull requests & review

Covered in [CONTRIBUTING.md](../CONTRIBUTING.md#pull-requests) and [CONTRIBUTING.md](../CONTRIBUTING.md#code-review-guidelines) —
this document defines the bar reviewers check against; that one defines the process.

## Related documents

- [ARCHITECTURE.md](../ARCHITECTURE.md) — the layering these standards enforce
- [docs/16-testing.md](16-testing.md) — how each of these rules gets verified in CI
- [docs/03-domain-model.md](03-domain-model.md) — the DDD vocabulary these naming rules apply to
