//! Tauri commands — the IPC surface exposed to the WebView (invoked from the frontend via
//! `@tauri-apps/api/core`'s `invoke()`). Kept thin: real logic lives in `native` and `daemon`, or
//! is delegated to the Core API over HTTP — this module is a translation layer, not a place for
//! business rules. See docs/02-folder-structure.md#desktop.

use serde::Serialize;

use crate::native;

#[tauri::command]
pub fn get_app_version(app_handle: tauri::AppHandle) -> String {
    app_handle.package_info().version.to_string()
}

#[derive(Serialize)]
pub struct HardwareProfile {
    pub cpu_count: usize,
    pub total_memory_bytes: u64,
    pub available_memory_bytes: u64,
}

/// Point-in-time hardware snapshot for the pre-flight checks described in
/// docs/07-local-llm.md#hardware-detection--benchmarking. GPU/VRAM detection is
/// platform-specific and lives in `native` per-OS modules, not sampled here yet.
#[tauri::command]
pub fn get_hardware_profile() -> HardwareProfile {
    native::sample_hardware_profile()
}
