# @hzt/plugin-sdk

TypeScript SDK for building HZT plugins — the only surface a plugin's sandboxed code can call
through the Capability Bridge (see [docs/10-plugin-system.md#sandbox-runtime](../../docs/10-plugin-system.md#sandbox-runtime)
and [docs/adr/0005-plugin-sandbox-model.md](../../docs/adr/0005-plugin-sandbox-model.md)).

**Status:** Phase 2 implementation target — see [docs/18-roadmap.md](../../docs/18-roadmap.md).
Planned shape:

```ts
import { definePlugin } from "@hzt/plugin-sdk";

export default definePlugin({
  async onNotify(event) {
    // capability-scoped calls only — no raw fs/net/child_process access
    await hzt.notifications.send({ title: event.title, body: event.body });
  },
});
```

Generated protocol types (from [`core/protocol/`](../../core/protocol/)) will live under
`src/protocol/` once the codegen pipeline lands.
