// Hermes Zone Torba desktop shell entry point.
//
// Responsibilities: tray icon + menu, window lifecycle, wiring the Tauri command surface exposed
// to the WebView, and starting the background daemon that supervises the local API process. See
// docs/adr/0003-tauri-over-electron.md for why this is Tauri/Rust rather than Electron/Node, and
// docs/02-folder-structure.md#desktop for how this file relates to commands/, daemon/, native/.
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod commands;
mod daemon;
mod native;

use tauri::menu::{Menu, MenuItem, PredefinedMenuItem};
use tauri::tray::TrayIconBuilder;
use tauri::{Manager, WindowEvent};

fn main() {
    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_notification::init())
        .plugin(tauri_plugin_updater::Builder::new().build())
        .plugin(tauri_plugin_process::init())
        .invoke_handler(tauri::generate_handler![
            commands::get_app_version,
            commands::get_hardware_profile,
        ])
        .setup(|app| {
            setup_tray(app)?;

            // Background daemon: supervises the local API process and native OS monitoring loop.
            // See docs/06-hermes-integration.md and docs/08-os-monitor.md for what this eventually
            // drives; today it's a stub that logs its own lifecycle.
            let handle = app.handle().clone();
            tauri::async_runtime::spawn(async move {
                daemon::run(handle).await;
            });

            Ok(())
        })
        .on_window_event(|window, event| {
            // Closing the main window hides it instead of quitting — the daemon and tray icon
            // keep running, matching "install once, always supervised" from docs/00-project-vision.md.
            if let WindowEvent::CloseRequested { api, .. } = event {
                window.hide().ok();
                api.prevent_close();
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running the Hermes Zone Torba desktop shell");
}

fn setup_tray(app: &tauri::App) -> tauri::Result<()> {
    let show_hide = MenuItem::with_id(app, "show_hide", "Show / Hide", true, None::<&str>)?;
    let separator = PredefinedMenuItem::separator(app)?;
    let quit = MenuItem::with_id(app, "quit", "Quit Hermes Zone Torba", true, None::<&str>)?;
    let menu = Menu::with_items(app, &[&show_hide, &separator, &quit])?;

    TrayIconBuilder::new()
        .menu(&menu)
        .tooltip("Hermes Zone Torba")
        .on_menu_event(|app, event| match event.id.as_ref() {
            "show_hide" => {
                if let Some(window) = app.get_webview_window("main") {
                    if window.is_visible().unwrap_or(false) {
                        window.hide().ok();
                    } else {
                        window.show().ok();
                        window.set_focus().ok();
                    }
                }
            }
            "quit" => app.exit(0),
            _ => {}
        })
        .build(app)?;

    Ok(())
}
