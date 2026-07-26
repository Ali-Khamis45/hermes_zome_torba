# Infrastructure

Provisioning and deployment configuration for all three deployment shapes described in
[docs/17-deployment.md](../docs/17-deployment.md).

## `docker/`

- `backend.Dockerfile` — multi-stage ASP.NET Core build, root-context (see
  [docker-compose.yml](../docker-compose.yml) and [.github/workflows/release.yml](../.github/workflows/release.yml))
- `frontend.Dockerfile` — multi-stage Next.js standalone build
- `otel-collector-config.yaml` — local-development OpenTelemetry Collector config (console exporter;
  production points at a real backend, see [docs/17-deployment.md](../docs/17-deployment.md#observability-in-production))

## `k8s/`

Kustomize-based manifests for the enterprise deployment shape
([docs/18-roadmap.md](../docs/18-roadmap.md) Phase 4):

```
k8s/
├── base/                    # Namespace, Deployments, Services, HPA, Ingress
└── overlays/
    ├── staging/              # 1 replica each, `edge` image tag
    └── production/           # 4/3 replicas, `latest` image tag
```

Render and diff before applying:

```bash
kubectl kustomize infrastructure/k8s/overlays/production
kubectl apply -k infrastructure/k8s/overlays/production
```

The base manifests assume managed PostgreSQL/Redis/vector-store external to the cluster —
connection details are injected via the `hzt-api-secrets` Secret (create this per-environment;
it is intentionally not committed). See [docs/17-deployment.md](../docs/17-deployment.md) for the
zero-downtime migration process this deployment shape requires.
