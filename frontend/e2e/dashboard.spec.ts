import { expect, test } from "@playwright/test";

// Smoke-level E2E coverage for the critical navigation path — see docs/16-testing.md#frontend.
// Deeper flows (install, model download, workflow creation) land alongside their backend
// endpoints in Phase 1, per docs/18-roadmap.md.

test("dashboard shell renders with sidebar navigation", async ({ page }) => {
  await page.goto("/");

  await expect(page.getByRole("heading", { name: "Overview" })).toBeVisible();
  await expect(page.getByRole("navigation").getByRole("link", { name: "Agents" })).toBeVisible();
});

test("navigating to Agents shows the install call to action", async ({ page }) => {
  await page.goto("/");
  // Scoped to the sidebar <nav> — the Overview page also links to /agents from its body
  // copy, so an unscoped role locator matches both and trips Playwright's strict mode.
  await page.getByRole("navigation").getByRole("link", { name: "Agents" }).click();

  await expect(page.getByRole("heading", { name: "Agents" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Install Hermes" })).toBeVisible();
});
