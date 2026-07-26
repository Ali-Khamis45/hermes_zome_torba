# plugins

The plugin manifest schema, sandbox runtime, and first-party reference plugins — see
[docs/10-plugin-system.md](../docs/10-plugin-system.md) for the full lifecycle, permission model,
and trust model this directory implements.

## Contents

- [`manifest.schema.json`](manifest.schema.json) — JSON Schema every plugin's `manifest.json` is
  validated against before install.
- [`examples/discord-plugin/`](examples/discord-plugin/) — reference implementation new plugin
  authors should start from; goes through the same signing/review process as any first-party
  plugin (see [docs/10-plugin-system.md#trust-model](../docs/10-plugin-system.md#trust-model)).

## Writing a plugin

1. Scaffold from `examples/discord-plugin/` or `npx @hzt/create-plugin` (Phase 3, see
   [`sdk/typescript/`](../sdk/typescript/)).
2. Declare the minimum `permissions.required` your plugin actually needs — the install flow shows
   users exactly what's requested, and over-asking is a fast way to fail marketplace review.
3. Implement against `@hzt/plugin-sdk` — never reach for Node's raw `fs`/`net`/`child_process`;
   the sandbox blocks it regardless (see [docs/adr/0005-plugin-sandbox-model.md](../docs/adr/0005-plugin-sandbox-model.md)).
4. Validate your manifest: `npx ajv validate -s plugins/manifest.schema.json -d your-plugin/manifest.json`.

## Where plugins run

Every plugin — first-party or third-party — executes in the isolated sandbox runtime described in
[docs/10-plugin-system.md#sandbox-runtime](../docs/10-plugin-system.md#sandbox-runtime), mediated
by the Capability Bridge. There is no unsandboxed execution path.
