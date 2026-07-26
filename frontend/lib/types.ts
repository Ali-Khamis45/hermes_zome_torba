/**
 * Mirrors the DTOs exposed by the Core API — see docs/04-api-spec.md and the backend records in
 * backend/src/Core/HermesZoneTorba.Application/HermesManagement/. Kept hand-written for now; Phase 1
 * generates this file from the OpenAPI spec instead (see docs/04-api-spec.md#conventions).
 */

export type InstanceStatus =
  | "NotInstalled"
  | "Installing"
  | "Stopped"
  | "Starting"
  | "Running"
  | "Faulted"
  | "Repairing"
  | "Uninstalling";

export interface HermesStatusDto {
  instanceId: string;
  version: string | null;
  status: InstanceStatus;
  installedAt: string | null;
  lastHealthCheckAt: string | null;
  lastFaultReason: string | null;
}

export interface InstallHermesResult {
  instanceId: string;
  version: string;
  status: InstanceStatus;
}

export interface InstallHermesRequest {
  requestedVersion?: string;
  installPath: string;
}

export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail: string;
  instance: string;
  traceId?: string;
}
