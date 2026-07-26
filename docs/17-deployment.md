# 17 · Deployment

## Purpose

How HZT actually gets onto a machine and stays running — local development, self-hosted Docker, and the
Kubernetes path for enterprise fleets — plus the release mechanics that produce each artifact.

## Local development

```bash
git clone https://github.com/Ali-Khamis45/hermes_zome_torba.git && cd hermes_zome_torba
cp .env.example .env
docker compose up -d postgres redis qdrant otel-collector

cd backend/src/Api/HermesZoneTorba.Api
dotnet ef database update
dotnet run                      # http://localhost:5080, Swagger at /swagger

cd ../../../../frontend
npm install && npm run dev      # http://localhost:3000

cd ../desktop
npm install && npm run tauri dev
```

`scripts/bootstrap.sh` wraps the above into one command for a fresh clone, including a hardware check
(warns if Docker/`.NET 10 SDK`/Node 22 aren't found) before running anything.

## Docker Compose (self-hosted)

`docker-compose.yml` at the repo root brings up the full stack: `postgres`, `redis`, `qdrant`,
`otel-collector`, `api`, `web`. Production compose usage differs from dev only in `.env` values
(`ASPNETCORE_ENVIRONMENT=Production`, real secrets, TLS termination via a reverse proxy in front of `web`/`api`
— nginx/Traefik/Caddy, left to the operator rather than bundled, since TLS termination choices vary widely by
environment).

```bash
docker compose --profile production up -d
```

Dockerfiles: [`infrastructure/docker/backend.Dockerfile`](../infrastructure/docker/backend.Dockerfile)
(multi-stage: SDK build → ASP.NET runtime image, non-root user, health check baked in) and
[`infrastructure/docker/frontend.Dockerfile`](../infrastructure/docker/frontend.Dockerfile) (multi-stage Next.js
standalone build).

## Kubernetes (enterprise, Phase 4)

`infrastructure/k8s/` holds base manifests (Kustomize overlays for dev/staging/prod): `api` as a Deployment
with an HPA keyed on CPU + a custom metric (active-supervised-process count), `web` as a Deployment behind an
Ingress, managed Postgres/Redis/vector-store as external dependencies (not run in-cluster in the reference
manifests — bring your own managed data tier in production). SignalR's Redis backplane is what makes the `api`
Deployment horizontally scalable without sticky sessions being a hard requirement.

## Desktop distribution

Tauri produces native installers per OS (`.msi`/`.exe` for Windows, `.dmg`/`.app` for macOS, `.AppImage`/`.deb`
for Linux) via [.github/workflows/release.yml](../.github/workflows/release.yml), code-signed
(`TAURI_SIGNING_PRIVATE_KEY` secret) so the built-in auto-updater can verify update authenticity before
applying. Auto-update checks a signed manifest hosted alongside GitHub Releases; users can pin to manual
updates in Settings.

## Zero-downtime migrations

For the Docker/Kubernetes paths where `api` runs as more than one replica during a rolling update:

1. Migrations are additive-first — a schema change that removes/renames a column ships as two releases: add
   new column + dual-write, then a later release removes the old column once all replicas are on the new code
   path.
2. `dotnet ef database update` runs as a pre-deploy step (Kubernetes: an init Job; Compose: an explicit
   `docker compose run --rm api dotnet ef database update` before rolling `api`), never as part of `api`
   container startup — so migration failures block the rollout instead of crash-looping live replicas.
3. Backward-incompatible migrations (documented per [docs/12-database.md](12-database.md#migrations)) require
   sign-off in the PR description before merge to `release/*`.

## Release strategy

Semantic versioning, automated by `semantic-release` reading Conventional Commits from
[CONTRIBUTING.md](../CONTRIBUTING.md#commit-messages) — `fix:` → patch, `feat:` → minor,
`BREAKING CHANGE:` footer → major. On merge to `main`
([.github/workflows/release.yml](../.github/workflows/release.yml)):

1. Compute next version from commit history since the last tag.
2. Generate/update `CHANGELOG.md`, tag the release.
3. Build and push `hzt-api` and `hzt-web` container images to GHCR, tagged with the version and `latest`.
4. Build and publish signed desktop installers for all three OSes, attached to the GitHub Release.

## Release checklist (manual verification)

Automated CI covers most of this (see [docs/16-testing.md](16-testing.md)); the following is verified by hand
once per release candidate since it's inherently cross-platform/native and hard to fully automate:

- [ ] Desktop tray icon, notifications, and background daemon behave correctly on Windows/macOS/Linux
- [ ] Auto-update flow tested from the previous released version
- [ ] Fresh install flow (no prior HZT state) completes end-to-end on a clean VM per OS
- [ ] Migration applies cleanly against a copy of a "real-ish" pre-release database
- [ ] Rollback path (previous image/version) verified against the new schema (additive-only, per above)

## Observability in production

Serilog structured logs ship to the configured sink (console for containers, picked up by the cluster's log
aggregator; optionally Seq for self-hosted single-node setups). OpenTelemetry traces/metrics export via OTLP
to the `otel-collector` sidecar (compose) or a cluster-level collector (k8s), forwarded to whatever backend the
operator configures (Grafana/Prometheus/Tempo, Datadog, etc. — HZT is backend-agnostic here by design). Health
checks (`/health/live`, `/health/ready`) back both Docker's `HEALTHCHECK` and Kubernetes probes.

## Related documents

- [docs/12-database.md](12-database.md#migrations) — migration authoring rules this deployment process assumes
- [docs/16-testing.md](16-testing.md) — CI gates a release must pass before reaching this stage
- [infrastructure/](../infrastructure/) — the manifests and Dockerfiles referenced above
