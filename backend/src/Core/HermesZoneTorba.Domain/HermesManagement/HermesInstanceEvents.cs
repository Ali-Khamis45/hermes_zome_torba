using HermesZoneTorba.Domain.Common;

namespace HermesZoneTorba.Domain.HermesManagement;

// Domain events raised by HermesInstance. Consumed across bounded contexts via MediatR notifications —
// see docs/03-domain-model.md#hermes-management and docs/01-system-architecture.md.

public sealed record HermesInstallStarted(Guid InstanceId, SemVer? RequestedVersion, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesInstallCompleted(Guid InstanceId, SemVer Version, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesInstallFailed(Guid InstanceId, string Reason, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesConfigurationChanged(Guid InstanceId, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesStarted(Guid InstanceId, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesStopped(Guid InstanceId, string Reason, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesFaulted(Guid InstanceId, string Reason, DateTimeOffset OccurredOn) : IDomainEvent;

public sealed record HermesRepaired(Guid InstanceId, string Summary, DateTimeOffset OccurredOn) : IDomainEvent;
