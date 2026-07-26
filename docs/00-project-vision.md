# 00 · Project Vision

## Purpose

Define why Hermes Zone Torba (HZT) exists, who it serves, and the principles every architectural decision in
this repository is judged against. Every other document in `docs/` implements some part of this vision — when
a design choice is ambiguous, resolve it against the principles below, then record the resolution as an ADR
in [`docs/adr/`](adr/).

## The problem

Running a local AI agent well today requires stitching together tools that were never designed to work
together:

1. **A runtime** (Ollama, LM Studio, vLLM) — installed via a separate installer, updated separately, with its
   own model format and its own quirks per OS.
2. **Models** — multi-gigabyte downloads with no visibility into whether your hardware can actually run them
   well until you try and it's too slow, or it OOMs.
3. **An agent framework** (Hermes) — a process that needs to be started, kept alive, restarted on crash, and
   configured against the runtime above.
4. **Monitoring** — `top`, `nvidia-smi`, Task Manager, log files scattered across three tools.
5. **Recovery** — when something breaks (driver crash, OOM, model corruption, an agent stuck in a loop), the
   user is on their own with a terminal and a search engine.

None of this is technically hard in isolation. It is hard as an *integrated, reliable, observable system* —
which is exactly the gap general-purpose runtimes and CLI tools intentionally leave unfilled, because it's not
their job. It's HZT's job.

## What HZT is

A **local-first control plane** for AI agents and LLM runtimes: one supervised system that installs, updates,
configures, monitors, heals, and extends Hermes agents and local LLM providers, exposed through a Desktop app,
a Web dashboard, and a first-class REST/SignalR API — with a plugin architecture so the ecosystem isn't capped
by what the core team ships.

## What HZT is not

- **Not a new agent framework.** Hermes (and other agent runtimes reachable through the provider abstraction)
  do the reasoning and tool-use. HZT supervises, connects, and operates them — it does not re-implement agent
  cognition.
- **Not a hosted SaaS by default.** The default deployment is local-first: your models, your data, your
  machine. Cloud providers (OpenAI/Anthropic/Gemini) and enterprise fleet management are opt-in extensions of
  the same architecture, not the default path.
- **Not a walled garden.** Every capability exposed in the dashboard is backed by a documented API and, where
  applicable, a plugin extension point. If the UI can do it, a script or a plugin can do it too.

## Design principles

| # | Principle | What it rules out |
|---|---|---|
| 1 | **Local-first, cloud-optional.** Core workflows (install, run, monitor, automate) work fully offline. | Features that hard-require a cloud account or phone-home telemetry by default. |
| 2 | **One click for the common path, full control underneath.** A non-technical user installs Hermes and a model in two clicks; an advanced user can override every config value the UI sets. | Dumbing down the API to match the simplest UI flow. |
| 3 | **Supervised by default.** Every managed process (Hermes, Ollama, plugins) runs under supervision — crashes are detected and reported, not silently swallowed. | "Fire and forget" process management. |
| 4 | **Recoverable, not just observable.** Monitoring without an auto-healing policy is a dashboard nobody acts on at 2am. Every failure class the OS Monitor detects has a corresponding healing policy, even if the default policy is "notify only." | Read-only monitoring as an end state. |
| 5 | **Security is not a layer bolted on later.** Sandboxing, permissions, and audit logging are part of the domain model from the first command executed, not retrofitted once there's a plugin ecosystem to protect. | Plugins or agents with unmediated OS access. |
| 6 | **Extensible by contract, not by fork.** Every subsystem (providers, plugins, notification channels) is defined by an interface a third party can implement without touching core code. | Provider- or vendor-specific logic leaking into the domain layer. |
| 7 | **Explainable automation.** Every automated action (healing, workflow execution) is logged with the trigger, the decision, and an undo path where technically possible. | Silent autonomous actions a user can't trace or reverse. |

## Primary personas

- **The Builder** — a developer running local models for coding/agent workflows who wants infra to just work
  so they can focus on the agent logic, with an API/SDK when they need to go beyond the UI.
- **The Operator** — a non-technical or semi-technical user who wants "install an AI assistant" to be as easy
  as installing any desktop app, with sane defaults and clear recovery when something goes wrong.
- **The Platform Admin** (enterprise, later phases) — manages a fleet of HZT instances, needs policy,
  audit, and centralized visibility without giving up the local-first execution model.
- **The Extension Author** — builds plugins, prompt packs, or workflow templates for the marketplace; needs a
  stable SDK, a sandbox they can trust, and a review process they can predict.

## Success criteria

HZT is successful when:

1. A new user goes from "nothing installed" to "a Hermes agent running against a local model" in under five
   minutes, without opening a terminal.
2. A crashed or stuck agent is detected and either recovered or clearly reported — silent failure is
   architecturally impossible, not just "usually caught."
3. A third-party developer can ship a plugin using only the published SDK and docs, with no access to
   unpublished internals.
4. The project is legible enough that a new contributor can find the bounded context responsible for any given
   behavior within minutes, using [docs/03-domain-model.md](03-domain-model.md) and [docs/02-folder-structure.md](02-folder-structure.md).

## Non-goals (for now)

Explicitly out of scope until a later roadmap phase reopens them (see [docs/18-roadmap.md](18-roadmap.md)):
multi-tenant SaaS hosting, mobile clients, training/fine-tuning infrastructure, distributed multi-node
inference clustering. These are plausible futures, not commitments — don't let them creep into Phase 0–2
scope.
