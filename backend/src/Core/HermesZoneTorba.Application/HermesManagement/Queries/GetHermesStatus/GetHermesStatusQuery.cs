using MediatR;

namespace HermesZoneTorba.Application.HermesManagement.Queries.GetHermesStatus;

public sealed record GetHermesStatusQuery(Guid InstanceId) : IRequest<HermesStatusDto?>;

public sealed record HermesStatusDto(
    Guid InstanceId,
    string? Version,
    string Status,
    DateTimeOffset? InstalledAt,
    DateTimeOffset? LastHealthCheckAt,
    string? LastFaultReason);
