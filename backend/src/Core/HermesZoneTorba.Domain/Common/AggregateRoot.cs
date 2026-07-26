namespace HermesZoneTorba.Domain.Common;

/// <summary>
/// Marks the entry point of a consistency boundary. Only aggregate roots are loaded/saved through
/// repositories — child entities are reached only through their root. See docs/03-domain-model.md.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    /// <summary>Optimistic concurrency token, mapped to a Postgres xmin-backed rowversion by EF Core.</summary>
    public uint ConcurrencyVersion { get; protected set; }
}
