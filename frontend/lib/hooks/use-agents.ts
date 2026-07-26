"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/api-client";
import type { HermesStatusDto, InstallHermesRequest, InstallHermesResult } from "@/lib/types";

/**
 * React Query hooks over the Hermes Manager endpoints (docs/04-api-spec.md). One hook per use case,
 * mirroring the backend's command/query split — components never call apiClient directly.
 */

const hermesKeys = {
  status: (instanceId: string) => ["hermes", "status", instanceId] as const,
};

export function useHermesStatus(instanceId: string | null) {
  return useQuery({
    queryKey: hermesKeys.status(instanceId ?? "unknown"),
    queryFn: ({ signal }) => apiClient.get<HermesStatusDto>(`/api/v1/hermes/${instanceId}/status`, signal),
    enabled: instanceId !== null,
    // Live updates arrive over AgentMonitorHub; this poll is a fallback for tabs without a socket yet.
    refetchInterval: 15_000,
  });
}

export function useInstallHermes() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: InstallHermesRequest) =>
      apiClient.post<InstallHermesResult>("/api/v1/hermes/install", request),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: hermesKeys.status(result.instanceId) });
    },
  });
}
