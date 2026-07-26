"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  Activity,
  Blocks,
  Bot,
  Brain,
  Cpu,
  LayoutDashboard,
  PanelLeftClose,
  PanelLeftOpen,
  ScrollText,
  Settings,
  ShieldCheck,
  Store,
  Terminal,
  Workflow,
} from "lucide-react";
import { cn } from "@/lib/utils";
import { useDashboardStore } from "@/store/use-dashboard-store";

/**
 * Primary navigation. See docs/02-folder-structure.md#frontend for the route tree this maps to —
 * each entry corresponds to a route under app/(dashboard)/.
 */
const NAV_SECTIONS = [
  {
    title: "Platform",
    items: [
      { href: "/", label: "Overview", icon: LayoutDashboard },
      { href: "/agents", label: "Agents", icon: Bot },
      { href: "/models", label: "Models", icon: Cpu },
      { href: "/automation", label: "Automation", icon: Workflow },
      { href: "/memory", label: "Memory", icon: Brain },
    ],
  },
  {
    title: "Extend",
    items: [
      { href: "/plugins", label: "Plugins", icon: Blocks },
      { href: "/marketplace", label: "Marketplace", icon: Store },
    ],
  },
  {
    title: "Operate",
    items: [
      { href: "/logs", label: "Logs", icon: ScrollText },
      { href: "/terminal", label: "Terminal", icon: Terminal },
      { href: "/security", label: "Security", icon: ShieldCheck },
      { href: "/diagnostics", label: "Diagnostics", icon: Activity },
      { href: "/settings", label: "Settings", icon: Settings },
    ],
  },
] as const;

export function Sidebar() {
  const pathname = usePathname();
  const collapsed = useDashboardStore((state) => state.sidebarCollapsed);
  const toggleSidebar = useDashboardStore((state) => state.toggleSidebar);

  return (
    <aside
      className={cn(
        "flex h-full flex-col border-r bg-sidebar text-sidebar-foreground transition-[width] duration-200",
        collapsed ? "w-16" : "w-64",
      )}
    >
      <div className="flex h-14 items-center justify-between border-b px-4">
        {!collapsed && <span className="text-sm font-semibold tracking-tight">Hermes Zone Torba</span>}
        <button
          type="button"
          onClick={toggleSidebar}
          aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
          className="rounded-md p-1.5 text-muted-foreground hover:bg-accent hover:text-accent-foreground"
        >
          {collapsed ? <PanelLeftOpen className="size-4" /> : <PanelLeftClose className="size-4" />}
        </button>
      </div>

      <nav className="flex-1 space-y-6 overflow-y-auto px-2 py-4">
        {NAV_SECTIONS.map((section) => (
          <div key={section.title}>
            {!collapsed && (
              <p className="mb-1 px-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">
                {section.title}
              </p>
            )}
            <ul className="space-y-0.5">
              {section.items.map((item) => {
                const isActive = pathname === item.href;
                const Icon = item.icon;
                return (
                  <li key={item.href}>
                    <Link
                      href={item.href}
                      className={cn(
                        "flex items-center gap-3 rounded-md px-2.5 py-2 text-sm font-medium transition-colors",
                        isActive
                          ? "bg-sidebar-accent text-sidebar-accent-foreground"
                          : "text-sidebar-foreground/80 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                      )}
                    >
                      <Icon className="size-4 shrink-0" />
                      {!collapsed && <span>{item.label}</span>}
                    </Link>
                  </li>
                );
              })}
            </ul>
          </div>
        ))}
      </nav>
    </aside>
  );
}
