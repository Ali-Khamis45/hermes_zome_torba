namespace HermesZoneTorba.Domain.Common;

/// <summary>
/// A fact that has happened inside an aggregate. Raised by aggregate methods, dispatched by the
/// persistence layer after a successful SaveChanges — see docs/01-system-architecture.md.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
