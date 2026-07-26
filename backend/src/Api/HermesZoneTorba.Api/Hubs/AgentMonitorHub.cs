using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HermesZoneTorba.Api.Hubs;

/// <summary>
/// Pushes agent status/incident updates to subscribed clients. Backed by the Redis backplane so this
/// scales across horizontally deployed API instances — see docs/04-api-spec.md#realtime-signalr and
/// docs/01-system-architecture.md.
/// </summary>
[Authorize]
public sealed class AgentMonitorHub : Hub
{
    public Task SubscribeToAgent(Guid agentId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(agentId));

    public Task UnsubscribeFromAgent(Guid agentId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(agentId));

    private static string GroupName(Guid agentId) => $"agent:{agentId}";
}
