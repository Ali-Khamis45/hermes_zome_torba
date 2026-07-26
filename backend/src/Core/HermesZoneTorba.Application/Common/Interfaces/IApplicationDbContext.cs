using HermesZoneTorba.Domain.HermesManagement;
using Microsoft.EntityFrameworkCore;

namespace HermesZoneTorba.Application.Common.Interfaces;

/// <summary>
/// The persistence surface Application handlers depend on. Implemented by
/// HermesZoneTorba.Infrastructure.Persistence.ApplicationDbContext — Application never references
/// EF Core's DbContext type directly, only this interface. See docs/01-system-architecture.md#layering.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<HermesInstance> HermesInstances { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
