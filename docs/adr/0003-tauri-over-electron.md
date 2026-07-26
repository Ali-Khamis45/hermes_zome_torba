# ADR 0003: Tauri as the primary desktop shell, Electron as fallback

## Status
Accepted

## Context
The Desktop app needs: a system tray, native notifications, a background daemon that can run before/without
the UI open, low idle resource usage (it's meant to sit alongside resource-hungry local LLM inference, not
compete with it for RAM), and cross-platform installers with a trustworthy auto-updater.

**Electron**: mature ecosystem, most prior art for exactly this kind of app (VS Code, Slack, Discord), but
ships a full Chromium + Node runtime per app (~120-200MB baseline, 100MB+ idle RAM) — a real cost when the
whole point of the product is leaving headroom for local model inference.

**Tauri**: uses the OS's native WebView (WebView2 on Windows, WKWebView on macOS, WebKitGTK on Linux) instead
of bundling Chromium, backend logic in Rust instead of Node — smaller installers (~10-20MB), lower idle
memory, and a Rust background daemon is a better fit for tight OS-level process supervision than a Node
daemon would be.

## Decision
Tauri is the primary desktop shell. Electron remains a documented fallback path (not implemented by default)
for contributors who need a capability Tauri's WebView-per-OS model can't yet provide, or for platforms where
the target WebView version is unacceptably old.

## Consequences
- Desktop-specific native code (`desktop/src-tauri/src/native/`) is written in Rust — raises the bar for
  desktop contributors slightly higher than "any JS developer," but matches the systems-programming portfolio
  goal of the project and gives real OS API access for [docs/08-os-monitor.md](../08-os-monitor.md)'s native
  collectors.
- WebView inconsistency across OS versions (especially older WebView2 installs on Windows) is a real risk;
  mitigated by bundling a WebView2 fallback installer bootstrap on first run rather than assuming presence.
- The web dashboard (`frontend/`) and desktop shell share the same React/Next.js UI code — Tauri wraps it, it
  doesn't fork it — so UI investment isn't duplicated between Web and Desktop.
