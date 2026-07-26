import { Activity, Bot, Cpu, HardDrive } from "lucide-react";
import { StatTile } from "@/components/dashboard/stat-tile";

/**
 * Dashboard Overview — see docs/08-os-monitor.md for where the live CPU/RAM/GPU/Disk figures come
 * from (MetricsHub over SignalR) once Phase 1 wires this up. Static placeholders for now so the
 * layout and design system are real, even before the data pipeline exists.
 */
export default function OverviewPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Overview</h1>
        <p className="text-sm text-muted-foreground">
          Fleet status at a glance. Live metrics arrive here once the OS Monitor worker
          (docs/08-os-monitor.md) is wired up in Phase 1.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatTile label="Agents running" value="0" hint="No Hermes instances installed yet" icon={Bot} />
        <StatTile label="CPU" value="—" hint="Awaiting OS Monitor" icon={Cpu} />
        <StatTile label="Memory" value="—" hint="Awaiting OS Monitor" icon={Activity} />
        <StatTile label="Disk" value="—" hint="Awaiting OS Monitor" icon={HardDrive} />
      </div>

      <div className="rounded-lg border border-dashed p-6 text-sm text-muted-foreground">
        Get started by installing your first Hermes agent from the{" "}
        <a href="/agents" className="font-medium text-foreground underline underline-offset-4">
          Agents
        </a>{" "}
        page.
      </div>
    </div>
  );
}
