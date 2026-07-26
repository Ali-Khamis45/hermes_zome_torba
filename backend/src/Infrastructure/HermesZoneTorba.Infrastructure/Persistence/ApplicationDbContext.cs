using HermesZoneTorba.Application.Common.Interfaces;
using HermesZoneTorba.Domain.HermesManagement;
using Microsoft.EntityFrameworkCore;

namespace HermesZoneTorba.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of IApplicationDbContext. Entity configurations live under
/// Persistence/Configurations/ (one file per aggregate) rather than fluent config inline here — see
/// docs/02-folder-structure.md#backend--clean-architecture-layout.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<HermesInstance> HermesInstances => Set<HermesInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
