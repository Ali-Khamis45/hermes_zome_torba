# Build context is the repository root — see docker-compose.yml and .github/workflows/release.yml.
# Multi-stage build: SDK image compiles + publishes, runtime image only ships the ASP.NET runtime.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY backend/HermesZoneTorba.slnx ./
COPY backend/src/Core/HermesZoneTorba.Domain/HermesZoneTorba.Domain.csproj src/Core/HermesZoneTorba.Domain/
COPY backend/src/Core/HermesZoneTorba.Application/HermesZoneTorba.Application.csproj src/Core/HermesZoneTorba.Application/
COPY backend/src/Infrastructure/HermesZoneTorba.Infrastructure/HermesZoneTorba.Infrastructure.csproj src/Infrastructure/HermesZoneTorba.Infrastructure/
COPY backend/src/Api/HermesZoneTorba.Api/HermesZoneTorba.Api.csproj src/Api/HermesZoneTorba.Api/
RUN dotnet restore src/Api/HermesZoneTorba.Api/HermesZoneTorba.Api.csproj

COPY backend/src/ src/
RUN dotnet publish src/Api/HermesZoneTorba.Api/HermesZoneTorba.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN groupadd --system hzt && useradd --system --gid hzt --home /app hzt
USER hzt

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

COPY --from=build --chown=hzt:hzt /app/publish .

EXPOSE 8080
# Liveness/readiness are checked at the orchestrator level (docker-compose healthcheck /
# Kubernetes probes hitting GET /health/live and /health/ready — see
# docs/17-deployment.md#observability-in-production) rather than a Dockerfile HEALTHCHECK, since
# the minimal runtime image doesn't ship curl/wget.

ENTRYPOINT ["dotnet", "HermesZoneTorba.Api.dll"]
