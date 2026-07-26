import { beforeEach, describe, expect, it } from "vitest";
import { useDashboardStore } from "@/store/use-dashboard-store";

describe("useDashboardStore", () => {
  beforeEach(() => {
    useDashboardStore.setState({ sidebarCollapsed: false, theme: "system" });
  });

  it("toggles sidebar collapse state", () => {
    useDashboardStore.getState().toggleSidebar();
    expect(useDashboardStore.getState().sidebarCollapsed).toBe(true);

    useDashboardStore.getState().toggleSidebar();
    expect(useDashboardStore.getState().sidebarCollapsed).toBe(false);
  });

  it("sets the theme preference", () => {
    useDashboardStore.getState().setTheme("dark");
    expect(useDashboardStore.getState().theme).toBe("dark");
  });
});
