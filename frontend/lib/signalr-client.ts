import * as signalR from "@microsoft/signalr";

/**
 * Factory for the SignalR hub connections described in docs/04-api-spec.md#realtime-signalr.
 * Auto-reconnects with backoff; callers subscribe to specific resources (e.g. an agent id) via the
 * hub's own methods rather than this module broadcasting everything.
 */
export function createHubConnection(hubPath: string): signalR.HubConnection {
  const baseUrl = process.env.NEXT_PUBLIC_SIGNALR_URL ?? "http://localhost:5080/hubs";

  return new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}${hubPath}`, {
      // TODO(Phase 1): supply accessTokenFactory once the frontend auth store exists —
      // see docs/11-security.md#authn--authz.
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

export const agentMonitorHub = () => createHubConnection("/agents");
