# 11 · Security

## Purpose

The production-grade security model covering authentication, authorization, sandboxing, secrets, and audit —
treated as domain concerns from day one (see [docs/00-project-vision.md](00-project-vision.md#design-principles),
principle 5), not a layer added after the fact.

## Threat model summary

| Actor | Motivation | Primary mitigations |
|---|---|---|
| Malicious/compromised plugin | Exfiltrate data, escalate to host access | Sandbox runtime, capability-scoped permissions, signature verification ([docs/10-plugin-system.md](10-plugin-system.md)) |
| Malicious workflow/automation | Chain built-in actions into unintended destructive behavior | Approval workflow ceiling on destructive actions ([docs/09-auto-healing.md](09-auto-healing.md#guardrails)), workflow permission scoping |
| Compromised network position | Intercept API traffic, credential theft | TLS everywhere, short-lived JWTs, refresh token rotation |
| Local attacker with disk access | Read stored credentials/model configs | Encrypted secrets at rest, OS keychain integration on desktop |
| Malicious/crafted terminal command | Arbitrary code execution via Terminal Agent | Command whitelist/blacklist, sandboxed execution, approval workflow |
| Insider (over-privileged user) | Access data/actions beyond their role | RBAC/ABAC, least-privilege defaults, full audit trail |

Follows OWASP ASVS practices for the API surface and OWASP Top 10 for web (frontend). Threat model is revisited
every major release and whenever a new agent type or plugin capability is added.

## AuthN / AuthZ

```mermaid
sequenceDiagram
    actor User
    participant Web
    participant Api
    participant Auth as Auth Service
    participant DB

    User->>Web: Login (credentials / OS-native auth on desktop)
    Web->>Api: POST /auth/login
    Api->>Auth: Validate credentials
    Auth->>DB: Load User + Roles + Permissions
    Auth-->>Api: Access token (15m) + Refresh token (rotating, 30d)
    Api-->>Web: Access token (body) + Refresh token (HttpOnly cookie / OS keychain)
    Web->>Api: Subsequent requests: Authorization: Bearer <access token>
    Api->>Api: Validate JWT signature + claims + AuthorizationBehavior (MediatR pipeline)
    Note over Api: On expiry, Web calls /auth/refresh with rotating token;<br/>reuse of a revoked refresh token invalidates the whole token family.
```

- **JWT** signed with an asymmetric key (RS256) in production; access tokens carry `sub`, `roles`,
  `permissions` (flattened, cached), `iat`/`exp`. Refresh tokens are opaque, stored hashed, rotated on every
  use (refresh token reuse detection invalidates the entire family — a stolen-then-reused token locks the
  session out and flags the incident).
- **RBAC**: built-in roles (`viewer`, `operator`, `admin`) map to permission sets on the standard resources
  (`hermes.*`, `models.*`, `workflows.*`, `security.*`, …).
- **ABAC layer** on top for resource-scoped rules the role model can't express alone — e.g., "operator may
  restart Hermes instances they created" — implemented as the `AuthorizationBehavior` MediatR pipeline step
  evaluating policies against the request + current user + target resource.
- Enforced once, centrally, in the MediatR pipeline (`Application/Common/Behaviors/AuthorizationBehavior.cs`)
  — individual command handlers do not re-implement authorization checks.

## Secrets & credential management

- Provider API keys (OpenAI/Anthropic/Gemini), plugin config secrets, and the JWT signing key are encrypted
  at rest (AES-256-GCM) with a key-encryption-key held in the OS keychain (desktop) or a configured secrets
  backend (server: environment-injected, with first-class support for mounting from HashiCorp Vault / cloud
  KMS in enterprise deployments — see [docs/17-deployment.md](17-deployment.md)).
- Secrets are never logged; the logging pipeline (Serilog) has a destructuring policy that redacts known
  secret-shaped fields (`*ApiKey`, `*Token`, `*Password`, `*Secret`) even if a developer forgets to mark a
  field explicitly.
- Plugin-declared config secrets are stored per-plugin, scoped so one plugin cannot read another's config
  through the Capability Bridge.

## Sandboxing

| Surface | Mechanism |
|---|---|
| Plugins | Isolated VM/WASM runtime, capability-mediated host access — [docs/10-plugin-system.md](10-plugin-system.md#sandbox-runtime) |
| Terminal Agent | Command whitelist/blacklist, restricted working directory, resource-limited subprocess, streamed output with cancellation |
| File Agent | Scoped to the instance's configured working directory; path traversal outside scope rejected at the Application layer, not just UI-hidden |
| Browser Agent | Runs in an isolated browser profile/context per task, no access to the user's real browser profile/cookies unless explicitly authorized |

## Approval workflow

Any action classified as destructive or irreversible (see [docs/09-auto-healing.md](09-auto-healing.md#guardrails)
for the healing-specific ceiling) requires explicit approval regardless of the requesting user's role, unless
the specific action + resource combination has a standing, audited pre-approval (e.g., "always auto-restart
this specific instance" — itself an auditable grant, revocable at any time).

## Audit logging

Append-only `AuditLog` (see [docs/12-database.md](12-database.md#audit-log)) records: actor (user id or
`system`/policy id), action, target resource, timestamp, correlation/trace id, and result. Audit entries are
never deleted, only retained per a configurable policy and exported for compliance in enterprise deployments;
the audit log itself is a first-class read model exposed at `GET /api/v1/security/audit-log` with the same
pagination/filtering conventions as [docs/04-api-spec.md](04-api-spec.md).

## Rate limiting

ASP.NET Core's built-in rate limiter, configured per-endpoint-group via the Options pattern: stricter limits
on `/auth/*` (brute-force protection, sliding window + lockout after repeated failures) and on
Terminal/Browser agent execution endpoints; generous limits on read-only dashboard polling endpoints, with
SignalR preferred over polling wherever the dashboard needs live data specifically to keep REST rate limits
meaningful.

## Plugin isolation recap

Covered in depth in [docs/10-plugin-system.md](10-plugin-system.md); the security-relevant guarantee restated
here: a plugin's declared permission set is the *ceiling* of what it can do, enforced by the Capability Bridge
independent of plugin code correctness — a buggy or malicious plugin cannot exceed its grant, full stop.

## Related documents

- [docs/09-auto-healing.md](09-auto-healing.md#guardrails) — approval workflow for automated actions
- [docs/10-plugin-system.md](10-plugin-system.md) — plugin trust and sandbox model
- [docs/12-database.md](12-database.md) — `Users`, `Roles`, `Permissions`, `AuditLog` schema
