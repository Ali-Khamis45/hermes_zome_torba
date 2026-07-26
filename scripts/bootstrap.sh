#!/usr/bin/env bash
# One-command dev environment bootstrap — see README.md#quick-start-development.
# Checks prerequisites, brings up the data tier, and prints next steps. Does not start the
# backend/frontend dev servers itself (left to the developer, since they usually want them in
# separate terminals with visible logs).
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

echo "==> Checking prerequisites"

check() {
  local name="$1" cmd="$2"
  if ! command -v "$cmd" >/dev/null 2>&1; then
    echo "  [MISSING] $name — install it before continuing"
    return 1
  fi
  echo "  [OK] $name"
}

missing=0
check "Docker"        docker    || missing=1
check ".NET SDK"      dotnet    || missing=1
check "Node.js"       node      || missing=1
check "npm"           npm       || missing=1

if [ "$missing" -ne 0 ]; then
  echo ""
  echo "One or more prerequisites are missing. See README.md#quick-start-development."
  exit 1
fi

if [ ! -f .env ]; then
  echo "==> Creating .env from .env.example"
  cp .env.example .env
else
  echo "==> .env already exists, leaving it as-is"
fi

echo "==> Starting data tier (postgres, redis, qdrant, otel-collector)"
docker compose up -d postgres redis qdrant otel-collector

echo "==> Restoring backend dependencies"
dotnet restore backend/HermesZoneTorba.slnx

echo "==> Installing frontend dependencies"
npm install --prefix frontend --legacy-peer-deps

echo ""
echo "Bootstrap complete. Next steps:"
echo "  1. Apply database migrations:"
echo "       cd backend/src/Api/HermesZoneTorba.Api && dotnet ef database update"
echo "  2. Run the API:"
echo "       cd backend/src/Api/HermesZoneTorba.Api && dotnet run"
echo "  3. Run the dashboard:"
echo "       cd frontend && npm run dev"
echo ""
echo "See README.md and docs/17-deployment.md for the full guide."
