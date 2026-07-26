using HermesZoneTorba.Domain.Common;

namespace HermesZoneTorba.Domain.HermesManagement;

public sealed class InvalidHermesStateTransitionException(InstanceStatus from, string attemptedAction)
    : DomainException($"Cannot perform '{attemptedAction}' while the instance is '{from}'.");

public sealed class HermesConfigurationRequiredException()
    : DomainException("The instance must be configured before it can be started.");
