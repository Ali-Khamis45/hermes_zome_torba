# sdk

Published packages third parties use to build plugins and integrations against
[`core/protocol`](../core/protocol/) and the REST/SignalR API — without depending on internal
backend code. See [docs/10-plugin-system.md](../docs/10-plugin-system.md).

| Package | Path | Consumers |
|---|---|---|
| `@hzt/plugin-sdk` | [`typescript/`](typescript/) | Plugin authors (docs/10-plugin-system.md) |
| `HermesZoneTorba.Sdk` | [`dotnet/`](dotnet/) | .NET integrations, first-party agent implementations |

## Versioning

Both SDKs are versioned against `hztApiVersion` (see the manifest schema in
[`plugins/manifest.schema.json`](../plugins/manifest.schema.json)) and support the last two major
backend releases, so plugin authors aren't forced into lockstep upgrades with every core release
— see [docs/10-plugin-system.md#versioning--dependencies](../docs/10-plugin-system.md#versioning--dependencies).

## What belongs here vs. `shared/`

`sdk/` is for packages an external developer installs. `shared/contracts/` is the internal
source-of-truth for DTOs/events that the SDKs, frontend, and backend all generate from — see
[shared/README.md](../shared/README.md).
