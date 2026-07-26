using HermesZoneTorba.Application.Common.Interfaces;
using HermesZoneTorba.Domain.HermesManagement;
using Microsoft.EntityFrameworkCore;

namespace HermesZoneTorba.Application.UnitTests;

/// <summary>
/// Minimal in-memory-provider implementation of IApplicationDbContext for handler tests — the real
/// ApplicationDbContext (with its jsonb/xmin mapping) lives in Infrastructure and is exercised by
/// HermesZoneTorba.Infrastructure.IntegrationTests instead. Application tests only need the interface
/// contract, not the real persistence mapping.
/// </summary>
public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<HermesInstance> HermesInstances => Set<HermesInstance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<HermesInstance>(builder =>
        {
            builder.Property(i => i.Version)
                .HasConversion(
                    v => v == null ? null : v.ToString(),
                    v => v == null ? null : Domain.Common.SemVer.Parse(v));

            builder.OwnsOne(i => i.Configuration, config => config.OwnsOne(c => c.ResourceLimits));
        });

        base.OnModelCreating(modelBuilder);
    }

    public static TestDbContext CreateInMemory() =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
