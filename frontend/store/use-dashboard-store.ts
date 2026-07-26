"use client";

import { create } from "zustand";
import { persist } from "zustand/middleware";

/**
 * Client-only UI state that doesn't belong in React Query (which owns server state — see
 * lib/hooks/*). Sidebar collapse and theme preference persist across sessions; nothing here is
 * ever the source of truth for domain data.
 */
interface DashboardState {
  sidebarCollapsed: boolean;
  toggleSidebar: () => void;

  theme: "light" | "dark" | "system";
  setTheme: (theme: DashboardState["theme"]) => void;
}

export const useDashboardStore = create<DashboardState>()(
  persist(
    (set) => ({
      sidebarCollapsed: false,
      toggleSidebar: () => set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),

      theme: "system",
      setTheme: (theme) => set({ theme }),
    }),
    { name: "hzt-dashboard-preferences" },
  ),
);
