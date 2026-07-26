namespace HermesZoneTorba.Domain.Common;

/// <summary>
/// Persistence contract for an aggregate root. Implemented in Infrastructure; Application depends only
/// on this interface, never on EF Core directly — see docs/01-system-architecture.md#layering.
/// </summary>
public interface IRepository<TAggregate, in TId>
    where TAggregate : AggregateRoot<TId>
    where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    void Add(TAggregate aggregate);

    void Update(TAggregate aggregate);

    void Remove(TAggregate aggregate);
}
