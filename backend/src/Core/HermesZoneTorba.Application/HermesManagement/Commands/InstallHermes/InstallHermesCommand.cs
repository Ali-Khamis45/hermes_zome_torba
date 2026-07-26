using MediatR;

namespace HermesZoneTorba.Application.HermesManagement.Commands.InstallHermes;

/// <summary>
/// Installs a new Hermes instance. See docs/06-hermes-integration.md#install-flow for the full sequence
/// diagram this handler implements.
/// </summary>
public sealed record InstallHermesCommand(string? RequestedVersion, string InstallPath) : IRequest<InstallHermesResult>;

public sealed record InstallHermesResult(Guid InstanceId, string Version, string Status);
