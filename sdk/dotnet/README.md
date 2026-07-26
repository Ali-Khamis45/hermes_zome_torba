# HermesZoneTorba.Sdk

.NET SDK for integrations and first-party agent implementations that need typed access to the Core
API and `core/protocol` message contracts without referencing internal backend assemblies
(`HermesZoneTorba.Application`, `HermesZoneTorba.Infrastructure`).

**Status:** Phase 2 implementation target — see [docs/18-roadmap.md](../../docs/18-roadmap.md).
Will package:

- Typed HTTP client generated from the OpenAPI spec (see [docs/04-api-spec.md](../../docs/04-api-spec.md))
- SignalR hub client wrappers for `AgentMonitorHub`, `ModelHub`, `MetricsHub`, `LogStreamHub`, `WorkflowHub`
- `core/protocol` message types for building agent processes in .NET
