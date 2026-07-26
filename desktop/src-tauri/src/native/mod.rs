//! Per-OS native API bindings — hardware/process sampling backing the OS Monitor's native
//! collectors described in docs/08-os-monitor.md. Cross-platform baseline uses `sysinfo`; deeper
//! per-OS signals (Windows PDH/WMI/ETW, Linux /proc /sys, macOS IOKit, GPU vendor APIs) are added
//! here as dedicated submodules as Phase 1/2 implement them — see docs/18-roadmap.md.

use sysinfo::System;

use crate::commands::HardwareProfile;

pub fn sample_hardware_profile() -> HardwareProfile {
    let mut system = System::new_all();
    system.refresh_all();

    HardwareProfile {
        cpu_count: system.cpus().len(),
        total_memory_bytes: system.total_memory(),
        available_memory_bytes: system.available_memory(),
    }
}

// TODO(Phase 1): GPU/VRAM detection per docs/07-local-llm.md#hardware-detection--benchmarking —
// NVML for NVIDIA, ROCm-SMI for AMD, Metal performance shaders query for Apple Silicon.
#[cfg(target_os = "windows")]
mod windows {
    // TODO(Phase 1): PDH counters, WMI queries, ETW subscription — see docs/08-os-monitor.md.
}

#[cfg(target_os = "linux")]
mod linux {
    // TODO(Phase 1): /proc and /sys sampling — see docs/08-os-monitor.md.
}

#[cfg(target_os = "macos")]
mod macos {
    // TODO(Phase 1): IOKit / host_statistics sampling — see docs/08-os-monitor.md.
}
