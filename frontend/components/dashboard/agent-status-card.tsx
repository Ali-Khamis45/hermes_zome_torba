import { Bot } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { cn } from "@/lib/utils";
import type { HermesStatusDto } from "@/lib/types";

/**
 * Renders a single HermesInstance's status — the UI counterpart to the aggregate documented in
 * docs/03-domain-model.md#hermes-management. Status color mapping matches the state machine there.
 */
const STATUS_STYLES: Record<HermesStatusDto["status"], string> = {
  NotInstalled: "bg-muted text-muted-foreground",
  Installing: "bg-blue-500/15 text-blue-600 dark:text-blue-400",
  Stopped: "bg-muted text-muted-foreground",
  Starting: "bg-blue-500/15 text-blue-600 dark:text-blue-400",
  Running: "bg-emerald-500/15 text-emerald-600 dark:text-emerald-400",
  Faulted: "bg-destructive/15 text-destructive",
  Repairing: "bg-amber-500/15 text-amber-600 dark:text-amber-400",
  Uninstalling: "bg-muted text-muted-foreground",
};

export function AgentStatusCard({ status }: { status: HermesStatusDto }) {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="flex items-center gap-2 text-sm font-medium">
          <Bot className="size-4 text-muted-foreground" />
          Hermes instance
        </CardTitle>
        <Badge variant="outline" className={cn("border-0", STATUS_STYLES[status.status])}>
          {status.status}
        </Badge>
      </CardHeader>
      <CardContent className="space-y-1 text-sm text-muted-foreground">
        <p>Version: {status.version ?? "—"}</p>
        <p>Last health check: {status.lastHealthCheckAt ?? "never"}</p>
        {status.lastFaultReason && (
          <p className="text-destructive">Last fault: {status.lastFaultReason}</p>
        )}
      </CardContent>
    </Card>
  );
}
