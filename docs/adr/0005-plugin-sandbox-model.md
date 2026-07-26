# ADR 0005: Capability-mediated sandbox for plugins, no raw OS access

## Status
Accepted

## Context
The plugin ecosystem ([docs/10-plugin-system.md](../10-plugin-system.md)) is explicitly meant to grow beyond
first-party code — third-party plugins run alongside a system that already has elevated capability (it
installs software, executes terminal commands, reads the filesystem). The threat model
([docs/11-security.md](../11-security.md#threat-model-summary)) treats a malicious or compromised plugin as a
first-class actor, not an edge case.

Options considered: (a) run plugins as regular Node.js child processes with normal OS permissions, relying on
manifest review alone; (b) full OS-level sandboxing per plugin (containers/VMs per plugin instance); (c) an
in-process isolated VM (or WASM) with a mediated capability bridge exposing only explicitly granted host
functions.

(a) fails the moment review misses something or a plugin is compromised post-review (supply-chain attack on a
plugin's own dependency, for instance) — review is a speed bump, not a boundary. (b) gives the strongest
isolation but the resource and complexity cost (a container or VM per installed plugin, on what may be a
consumer laptop already running local LLM inference) is disproportionate to the actual capability most
plugins need (send a notification, read specific memory entries).

## Decision
Plugins run in an isolated VM (Node.js isolate) or WASM runtime with **no direct access** to `fs`, `net`,
`child_process`, or process handles. All host interaction goes through a Capability Bridge that checks the
plugin's granted permission set (from its manifest, approved at install time) on every call — see
[docs/10-plugin-system.md](../10-plugin-system.md#sandbox-runtime). There is no "trusted plugin" escape hatch
that bypasses the bridge; first-party plugins go through the identical enforcement path as third-party ones,
just with a faster review/signing process.

## Consequences
- The plugin SDK (`sdk/typescript/`) can only offer capabilities the bridge implements — a plugin author
  cannot "just `require('fs')`" their way around a missing capability; new capabilities are added deliberately
  to the bridge, keeping the attack surface enumerable.
- Some plugin ideas (deep IDE integration needing broad filesystem access, for example) require correspondingly
  broad, explicitly-granted permissions rather than being blocked outright — the model scales capability with
  explicit trust, it doesn't cap what's possible.
- Sandbox escape becomes the single highest-value security target for the whole system; the bridge
  implementation gets disproportionate security review attention relative to its code size, and is covered by
  the CODEOWNERS security-team gate (see [.github/CODEOWNERS](../../.github/CODEOWNERS)).
- Slight per-call overhead versus native access — acceptable for the plugin use cases in scope (notifications,
  integrations, workflow actions); not the right model for performance-critical core subsystems, which
  correctly remain in-process C#/Rust rather than plugins.
