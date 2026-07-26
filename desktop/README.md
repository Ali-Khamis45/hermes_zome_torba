# Hermes Zone Torba — Desktop Shell

Tauri (Rust + native WebView) desktop application: tray icon, native OS integration, background
daemon, and auto-update, wrapping the same dashboard UI as [`frontend/`](../frontend/). See
[docs/adr/0003-tauri-over-electron.md](../docs/adr/0003-tauri-over-electron.md) for why Tauri was
chosen, and [docs/02-folder-structure.md#desktop](../docs/02-folder-structure.md) for the layout.

## Status

This is the Phase 0 skeleton: a real `Cargo.toml`/`tauri.conf.json`/capabilities configuration and
the tray + window + daemon wiring in `src-tauri/src/`, written against the stable Tauri v2 API.
**It has not been compiled in this environment** (no Rust toolchain available during scaffolding)
— run `cargo check` from `src-tauri/` as your first step before building on it further.

## Structure

```
desktop/
├── src-tauri/
│   ├── src/
│   │   ├── main.rs        # Tray, window lifecycle, command registration, daemon spawn
│   │   ├── commands/      # #[tauri::command] surface exposed to the WebView
│   │   ├── daemon/        # Background supervision loop (stub — see docs/18-roadmap.md Phase 2)
│   │   └── native/        # Per-OS hardware/process sampling (docs/08-os-monitor.md)
│   ├── capabilities/      # Tauri v2 permission grants — default-deny, explicit allow-list
│   ├── icons/             # Generate via `npm run tauri icon` — see icons/README.md
│   ├── Cargo.toml
│   ├── build.rs
│   └── tauri.conf.json
└── package.json           # @tauri-apps/cli + plugin JS bindings
```

## Prerequisites

- [Rust toolchain](https://www.rust-lang.org/tools/install) (stable, 1.77+)
- Platform build dependencies per the [Tauri prerequisites guide](https://v2.tauri.app/start/prerequisites/)
  (WebView2 on Windows — usually preinstalled; `libwebkit2gtk` + friends on Linux; Xcode Command
  Line Tools on macOS)
- Node.js 22+ (for the wrapped `frontend/` build)

## Development

```bash
npm install
npm run dev     # tauri dev — launches the frontend dev server + a native window pointed at it
```

## Building an installer

```bash
npm run build   # tauri build — requires frontend/ to produce a static export at frontend/out
                 # (see next.config.ts; the desktop target and the Docker/Kubernetes web target
                 # currently build the frontend two different ways — reconciling that is tracked
                 # for Phase 1, see docs/18-roadmap.md)
```

CI builds and publishes signed installers for Windows/macOS/Linux on release —
see [.github/workflows/release.yml](../.github/workflows/release.yml) and
[docs/17-deployment.md#desktop-distribution](../docs/17-deployment.md#desktop-distribution).

## Permissions

Tauri v2 is default-deny: every command or plugin surface the WebView can call must be listed in
[`src-tauri/capabilities/default.json`](src-tauri/capabilities/default.json). Treat additions here
with the same scrutiny as a plugin permission grant in
[docs/10-plugin-system.md](../docs/10-plugin-system.md#permission-model) — it's the same
principle, applied to the desktop shell's own WebView instead of a third-party plugin sandbox.
