# notification-agent

Fans a single notification out to whichever channels are configured: Desktop, Discord, Slack,
Telegram, Email, SMS, Push. Invoked directly by the Auto-Healing Engine
([docs/09-auto-healing.md](../../docs/09-auto-healing.md)) and as a `SendNotificationAction` from
user-authored workflows ([docs/13-workflows.md](../../docs/13-workflows.md)).

Channel-specific delivery (Discord webhook, Slack app, etc.) is implemented as first-party plugins
under [`plugins/examples/`](../../plugins/) rather than hardcoded here, so a channel can be added
or swapped without changing this agent — see [docs/10-plugin-system.md](../../docs/10-plugin-system.md).

**Status:** Phase 1 implementation target (Desktop notifications ship first; other channels are
Phase 2/3 plugin work).
