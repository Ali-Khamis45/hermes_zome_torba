# browser-agent

Playwright-driven browser automation: tabs, form fill, downloads/uploads, scraping, and
authenticated flows. Each task runs in an isolated browser profile/context — no access to the
user's real browser profile or cookies unless explicitly authorized, per
[docs/11-security.md#sandboxing](../../docs/11-security.md#sandboxing).

**Status:** Phase 3 implementation target (multi-agent collaboration roles depend on this for
research/data-gathering tasks) — see [docs/18-roadmap.md](../../docs/18-roadmap.md). `taskType`
values: `browser.navigate`, `browser.fillForm`, `browser.extract`, `browser.download`.
