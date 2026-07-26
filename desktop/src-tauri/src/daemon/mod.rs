//! Background daemon — the process-supervision loop that keeps the local Core API (and, once
//! implemented, Hermes/Ollama processes) alive independent of whether the main window is open.
//! This is the desktop-side half of the "supervised by default" principle in
//! docs/00-project-vision.md#design-principles; the AI Supervisor backend worker
//! (docs/05-ai-supervisor.md) owns detection/classification once the API is up — this loop's job
//! is narrower: make sure the API process itself exists and restart it if it doesn't.
//!
//! Currently a stub that logs its own lifecycle. Real process spawning/supervision lands in
//! Phase 2 alongside the Automation Engine and Auto-Healing integration — see docs/18-roadmap.md.

use std::time::Duration;

use tauri::AppHandle;

pub async fn run(_app_handle: AppHandle) {
    // TODO(Phase 1): replace with structured logging (tauri-plugin-log) wired to the same
    // correlation-id conventions as the backend — see docs/15-coding-standards.md#logging.
    println!("[hzt-daemon] background daemon starting");

    loop {
        // TODO(Phase 2): spawn/monitor the local Core API process, restart on unexpected exit,
        // and surface state changes to the tray icon + window via Tauri events. See
        // docs/06-hermes-integration.md and docs/09-auto-healing.md for the recovery policies
        // this loop will eventually delegate to.
        tokio::time::sleep(Duration::from_secs(30)).await;
    }
}
