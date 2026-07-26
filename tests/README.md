# tests

Cross-cutting test suites that don't belong to a single project: end-to-end, contract, and
load/performance tests. Module-local tests (backend unit/integration/architecture tests, frontend
component tests) live next to their module instead — see
[docs/16-testing.md](../docs/16-testing.md) for the full test pyramid and where each tier lives.

| Path | Covers | Status |
|---|---|---|
| `contract/` | OpenAPI/AsyncAPI backward compatibility, plugin manifest schema conformance | Phase 1 |
| `load/` | API throughput under concurrent agent supervision, SignalR fan-out at scale (k6/NBomber) | Phase 2 |

Frontend E2E tests live in [`frontend/e2e/`](../frontend/e2e/) (Playwright, run against the
built app — see [docs/16-testing.md#frontend](../docs/16-testing.md#frontend)) rather than here,
since they're tightly coupled to the frontend's own build/dev tooling.
