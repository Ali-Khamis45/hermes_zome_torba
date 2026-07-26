# examples

End-to-end example workflows and automations — runnable demonstrations of the Automation Engine
([docs/13-workflows.md](../docs/13-workflows.md)) and multi-agent collaboration
([docs/18-roadmap.md](../docs/18-roadmap.md) Phase 3), kept outside `docs/` because they're meant
to be imported and run, not just read.

Planned first examples (Phase 2+):

- `gpu-throttle-workflow.json` — the canonical "IF GPU usage > 95%, pause model, wait, resume"
  automation from [docs/13-workflows.md](../docs/13-workflows.md#model-execution)
- `battery-power-saving-workflow.json` — "IF battery < 20%, stop models, enable power saving"
- `multi-agent-code-review/` — Planner → Coder → Reviewer collaboration using `core/protocol`
  structured messages, per the multi-agent roles in [docs/18-roadmap.md](../docs/18-roadmap.md#phase-3)

Each example, once added, includes the workflow/automation definition, the plugins or agents it
depends on, and a short README explaining what it demonstrates and how to run it against a local
HZT instance.
