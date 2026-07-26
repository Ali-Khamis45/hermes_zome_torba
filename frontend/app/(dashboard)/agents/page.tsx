"use client";

import { useState } from "react";
import { Loader2, Plus } from "lucide-react";
import { AgentStatusCard } from "@/components/dashboard/agent-status-card";
import { Button } from "@/components/ui/button";
import { useHermesStatus, useInstallHermes } from "@/lib/hooks/use-agents";

/**
 * Hermes Manager UI — see docs/06-hermes-integration.md#install-flow for the sequence this page
 * drives end to end: POST /api/v1/hermes/install, then poll (and eventually subscribe to
 * AgentMonitorHub for) GET /api/v1/hermes/{id}/status.
 */
export default function AgentsPage() {
  const [installedInstanceId, setInstalledInstanceId] = useState<string | null>(null);

  const installHermes = useInstallHermes();
  const status = useHermesStatus(installedInstanceId);

  const handleInstall = () => {
    installHermes.mutate(
      { installPath: "/var/hzt/instances/instance-1" },
      { onSuccess: (result) => setInstalledInstanceId(result.instanceId) },
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Agents</h1>
          <p className="text-sm text-muted-foreground">
            Install, configure, and supervise Hermes agent instances.
          </p>
        </div>
        <Button onClick={handleInstall} disabled={installHermes.isPending}>
          {installHermes.isPending ? (
            <Loader2 className="size-4 animate-spin" />
          ) : (
            <Plus className="size-4" />
          )}
          Install Hermes
        </Button>
      </div>

      {installHermes.isError && (
        <p className="text-sm text-destructive">
          Install failed: {installHermes.error.message}
        </p>
      )}

      {status.data ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <AgentStatusCard status={status.data} />
        </div>
      ) : (
        <div className="rounded-lg border border-dashed p-6 text-sm text-muted-foreground">
          No Hermes instances installed yet. Click &ldquo;Install Hermes&rdquo; to create one.
        </div>
      )}
    </div>
  );
}
