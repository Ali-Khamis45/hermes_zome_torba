import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Minimal-footprint Docker/Kubernetes runtime image — see
  // infrastructure/docker/frontend.Dockerfile and docs/17-deployment.md. The desktop shell
  // (desktop/) instead needs a static export for Tauri's frontendDist; reconciling the two build
  // outputs is tracked for Phase 1 — see desktop/README.md#building-an-installer.
  output: "standalone",
};

export default nextConfig;
