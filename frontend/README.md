# Hermes Zone Torba — Web Dashboard

Next.js 16 / React 19 dashboard — the primary UI for monitoring, automating, and extending HZT.
See [docs/02-folder-structure.md#frontend](../docs/02-folder-structure.md) for the route/component
layout and [ARCHITECTURE.md](../ARCHITECTURE.md) for how this fits the rest of the system.

## Stack

TypeScript, Tailwind CSS v4, shadcn/ui (on [base-ui](https://base-ui.com)), React Query for server
state, Zustand for client UI state, React Hook Form + Zod for forms, Framer Motion, Recharts.

## Development

```bash
cp .env.example .env.local   # points NEXT_PUBLIC_API_URL at the local backend
npm install
npm run dev                  # http://localhost:3000
```

## Scripts

| Command | Purpose |
|---|---|
| `npm run dev` / `build` / `start` | Standard Next.js dev/build/start |
| `npm run typecheck` | `tsc --noEmit` |
| `npm run lint` | ESLint (flat config) |
| `npm run test` / `test:watch` | Vitest unit/component tests |
| `npm run test:e2e` | Playwright end-to-end tests (`e2e/`) |

## Structure

```
app/
├── (dashboard)/    # Shared chrome (Sidebar/Topbar) + Overview, Agents, Models, ...
└── (auth)/         # Sign-in — no dashboard chrome
components/
├── ui/             # shadcn/ui primitives
└── dashboard/      # Feature-scoped composite components
lib/
├── api-client.ts   # Typed fetch wrapper, Problem Details error handling — docs/04-api-spec.md
├── signalr-client.ts
├── types.ts        # Hand-mirrored backend DTOs (codegen lands in Phase 1)
└── hooks/          # React Query hooks per domain
store/              # Zustand — client-only UI state, never server state
```

## Notes for contributors

This project targets **Next.js 16**, which has several breaking changes from earlier versions
(Turbopack by default, fully-async `params`/`searchParams`, `middleware` renamed to `proxy`, etc.)
— see `node_modules/next/dist/docs/01-app/02-guides/upgrading/version-16.md` (or the public
[upgrade guide](https://nextjs.org/docs/app/guides/upgrading/version-16)) before assuming prior
App Router knowledge still applies verbatim.

Full architecture and process docs live in [`../docs/`](../docs/).
